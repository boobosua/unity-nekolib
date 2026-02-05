using System;
using System.Collections.Generic;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace NekoLib.Core
{
    [UnityEngine.Scripting.Preserve]
    /// <summary>Owns and updates timers by injecting an update hook into Unity's PlayerLoop.</summary>
    internal static class TimerPlayerLoopDriver
    {
        private const int DefaultActiveCapacity = 128;
        private const int DefaultRemovalCapacity = 32;
        private const int DefaultPoolCapacity = 8;

        private static readonly List<TimerHandlerBase> ActiveTimers = new(DefaultActiveCapacity);
        private static readonly List<TimerHandlerBase> ToRemove = new(DefaultRemovalCapacity);

        private static readonly List<SlotRecord> Slots = new(DefaultActiveCapacity);
        private static readonly Stack<int> FreeSlots = new(DefaultActiveCapacity);

        private static readonly Stack<CountdownHandler> CountdownPool = new(DefaultPoolCapacity);
        private static readonly Stack<StopwatchHandler> StopwatchPool = new(DefaultPoolCapacity);

        private static int _maxCountdownPoolSize = 128;
        private static int _maxStopwatchPoolSize = 128;

        private static bool _installed;
        private static bool _isUpdating;

        private struct SlotRecord
        {
            public int Id;
            public TimerHandlerBase Timer;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        /// <summary>Resets static state on domain reload and (re)installs the driver into the PlayerLoop.</summary>
        private static void InitDomain()
        {
            ActiveTimers.Clear();
            ToRemove.Clear();
            Slots.Clear();
            FreeSlots.Clear();

            CountdownPool.Clear();
            StopwatchPool.Clear();

            _maxCountdownPoolSize = 128;
            _maxStopwatchPoolSize = 128;

            _installed = false;
            _isUpdating = false;

            TryInstall();
        }

        private static void TryInstall()
        {
            if (_installed) return;

            try
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                InjectUpdateFunction(ref loop);
                PlayerLoop.SetPlayerLoop(loop);
                _installed = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[TimerPlayerLoopDriver] Failed to inject into PlayerLoop: {ex}");
                FallbackBehaviour.EnsureExists();
                _installed = true;
            }
        }

        internal static Countdown CreateCountdown(MonoBehaviour owner, float duration)
        {
            var handler = RentCountdown();
            handler.ReInitialize(owner, duration);

            var slot = AllocateSlot(handler);
            handler.AssignHandle(slot, Slots[slot].Id);

            handler.ActiveIndex = ActiveTimers.Count;
            ActiveTimers.Add(handler);

            return new Countdown(slot, Slots[slot].Id);
        }

        internal static Stopwatch CreateStopwatch(MonoBehaviour owner, Func<bool> stopCondition)
        {
            var handler = RentStopwatch();
            handler.ReInitialize(owner, stopCondition);

            var slot = AllocateSlot(handler);
            handler.AssignHandle(slot, Slots[slot].Id);

            handler.ActiveIndex = ActiveTimers.Count;
            ActiveTimers.Add(handler);

            return new Stopwatch(slot, Slots[slot].Id);
        }

        internal static bool IsAlive(int slot, int id)
        {
            if (slot < 0 || slot >= Slots.Count) return false;
            var rec = Slots[slot];
            return rec.Timer != null && rec.Id == id;
        }

        internal static bool IsRunning(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return false;
            return timer.IsRunning;
        }

        internal static bool IsPaused(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return false;
            return !timer.IsRunning;
        }

        internal static void Start(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.Start();
        }

        internal static void Pause(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.Pause();
        }

        internal static void Resume(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.Resume();
        }

        internal static void Stop(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;

            timer.StopInvoke();
            Unregister(timer);
        }

        internal static void Cancel(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return;

            timer.StopSilent();
            Unregister(timer);
        }

        internal static void SetUnscaledTime(Countdown handler) => SetUnscaledTime(handler.Slot, handler.Id, true);
        internal static void SetScaledTime(Countdown handler) => SetUnscaledTime(handler.Slot, handler.Id, false);

        internal static void SetUnscaledTime(Stopwatch handler) => SetUnscaledTime(handler.Slot, handler.Id, true);
        internal static void SetScaledTime(Stopwatch handler) => SetUnscaledTime(handler.Slot, handler.Id, false);

        private static void SetUnscaledTime(int slot, int id, bool unscaled)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.SetUnscaledTime(unscaled);
        }

        internal static void SetUpdateWhen(Countdown handler, Func<bool> updateWhen) => SetUpdateWhen(handler.Slot, handler.Id, updateWhen);
        internal static void SetUpdateWhen(Stopwatch handler, Func<bool> updateWhen) => SetUpdateWhen(handler.Slot, handler.Id, updateWhen);

        private static void SetUpdateWhen(int slot, int id, Func<bool> updateWhen)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.SetUpdateWhen(updateWhen);
        }

        internal static void AddOnStart(Countdown handler, Action cb) => AddOnStart(handler.Slot, handler.Id, cb);
        internal static void AddOnUpdate(Countdown handler, Action<float> cb) => AddOnUpdate(handler.Slot, handler.Id, cb);
        internal static void AddOnLoop(Countdown handler, Action cb) => AddOnLoop(handler.Slot, handler.Id, cb);
        internal static void AddOnStop(Countdown handler, Action cb) => AddOnStop(handler.Slot, handler.Id, cb);

        internal static void AddOnStart(Stopwatch handler, Action cb) => AddOnStart(handler.Slot, handler.Id, cb);
        internal static void AddOnUpdate(Stopwatch handler, Action<float> cb) => AddOnUpdate(handler.Slot, handler.Id, cb);
        internal static void AddOnStop(Stopwatch handler, Action cb) => AddOnStop(handler.Slot, handler.Id, cb);

        private static void AddOnStart(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnStart(cb);
        }

        private static void AddOnUpdate(int slot, int id, Action<float> cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnUpdate(cb);
        }

        private static void AddOnStop(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            timer.AddOnStop(cb);
        }

        private static void AddOnLoop(int slot, int id, Action cb)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.AddOnLoop(cb);
        }

        internal static void SetCountdownLoopCount(Countdown handler, int loopCount)
        {
            if (!TryGet(handler.Slot, handler.Id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.SetLoop(loopCount);
        }

        internal static void SetCountdownLoopCondition(Countdown handler, Func<bool> stopCondition)
        {
            if (!TryGet(handler.Slot, handler.Id, out var timer)) return;
            if (timer is CountdownHandler cd) cd.SetLoop(stopCondition);
        }

        internal static void AddCountdownTime(int slot, int id, float seconds)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is not CountdownHandler c) return;

            c.AddTime(seconds);
        }

        internal static void ReduceCountdownTime(int slot, int id, float seconds)
        {
            if (!TryGet(slot, id, out var timer)) return;
            if (timer is not CountdownHandler c) return;

            c.ReduceTime(seconds);

            if (c.RemainingTime <= 0f)
            {
                Unregister(timer);
            }
        }


        internal static float GetCountdownRemainingTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is CountdownHandler c ? c.RemainingTime : 0f;
        }

        internal static float GetCountdownTotalTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is CountdownHandler c ? c.TotalTime : 0f;
        }

        internal static int GetCountdownLoopIteration(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0;
            return timer is CountdownHandler c ? c.CurrentLoopIteration : 0;
        }

        internal static float GetStopwatchElapsedTime(int slot, int id)
        {
            if (!TryGet(slot, id, out var timer)) return 0f;
            return timer is StopwatchHandler s ? s.ElapsedTime : 0f;
        }

        internal static void PrewarmCountdown(int count)
        {
            if (count <= 0) return;

            _maxCountdownPoolSize = Mathf.Max(_maxCountdownPoolSize, count);

            while (CountdownPool.Count < count)
            {
                var h = new CountdownHandler();
                h.ResetForPool();
                CountdownPool.Push(h);
            }
        }

        internal static void PrewarmStopwatch(int count)
        {
            if (count <= 0) return;

            _maxStopwatchPoolSize = Mathf.Max(_maxStopwatchPoolSize, count);

            while (StopwatchPool.Count < count)
            {
                var h = new StopwatchHandler();
                h.ResetForPool();
                StopwatchPool.Push(h);
            }
        }

        /// <summary>Called from the PlayerLoop; ticks active timers and processes removals.</summary>
        internal static void UpdateTimers()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            _isUpdating = true;

            for (int i = 0; i < ActiveTimers.Count; i++)
            {
                var timer = ActiveTimers[i];
                if (timer == null)
                {
                    RemoveAtIndex(i);
                    i--;
                    continue;
                }

                if (!timer.IsOwnerValid)
                {
                    MarkForRemoval(timer);
                    continue;
                }

                if (!timer.ShouldTick) continue;

                var dt = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                bool completed;
                try
                {
                    completed = timer.Tick(dt);
                }
                catch (Exception ex)
                {
                    Log.Error($"[TimerPlayerLoopDriver] Timer tick threw: {ex}");
                    completed = true;
                }

                if (completed)
                {
                    MarkForRemoval(timer);
                }
            }

            _isUpdating = false;

            if (ToRemove.Count > 0)
            {
                for (int i = 0; i < ToRemove.Count; i++)
                {
                    var t = ToRemove[i];
                    if (t == null) continue;

                    t.IsPendingRemoval = false;

                    RemoveActiveTimer(t);
                    ReleaseSlot(t);
                    ReturnToPool(t);
                }

                ToRemove.Clear();
            }
        }

        private static void MarkForRemoval(TimerHandlerBase timer)
        {
            if (timer.IsPendingRemoval) return;
            timer.IsPendingRemoval = true;
            ToRemove.Add(timer);
        }

        private static void Unregister(TimerHandlerBase timer)
        {
            if (_isUpdating)
            {
                MarkForRemoval(timer);
                return;
            }

            RemoveActiveTimer(timer);
            ReleaseSlot(timer);
            ReturnToPool(timer);
        }

        private static void RemoveActiveTimer(TimerHandlerBase timer)
        {
            var index = timer.ActiveIndex;
            if (index < 0 || index >= ActiveTimers.Count) return;
            if (!ReferenceEquals(ActiveTimers[index], timer)) return;

            RemoveAtIndex(index);
        }

        private static void RemoveAtIndex(int index)
        {
            var lastIndex = ActiveTimers.Count - 1;
            var removed = ActiveTimers[index];

            if (index != lastIndex)
            {
                var last = ActiveTimers[lastIndex];
                ActiveTimers[index] = last;
                last.ActiveIndex = index;
            }

            ActiveTimers.RemoveAt(lastIndex);

            if (removed != null)
            {
                removed.ActiveIndex = -1;
                removed.IsPendingRemoval = false;
            }
        }

        // Slot/Id are used to invalidate stale struct references after a timer is returned to the pool.
        private static int AllocateSlot(TimerHandlerBase timer)
        {
            if (FreeSlots.Count > 0)
            {
                var slot = FreeSlots.Pop();
                var rec = Slots[slot];

                rec.Timer = timer;
                rec.Id++;
                if (rec.Id == 0) rec.Id = 1;

                Slots[slot] = rec;
                return slot;
            }

            var newSlot = Slots.Count;
            Slots.Add(new SlotRecord { Id = 1, Timer = timer });
            return newSlot;
        }

        private static void ReleaseSlot(TimerHandlerBase timer)
        {
            var slot = timer.Slot;
            if (slot < 0 || slot >= Slots.Count)
            {
                timer.ClearHandle();
                return;
            }

            var rec = Slots[slot];
            if (ReferenceEquals(rec.Timer, timer))
            {
                rec.Timer = null;
                Slots[slot] = rec;
                FreeSlots.Push(slot);
            }

            timer.ClearHandle();
        }

        private static bool TryGet(int slot, int id, out TimerHandlerBase timer)
        {
            timer = null;

            if (slot < 0 || slot >= Slots.Count) return false;

            var rec = Slots[slot];
            if (rec.Timer == null || rec.Id != id) return false;

            timer = rec.Timer;
            return true;
        }

        private static CountdownHandler RentCountdown()
        {
            return CountdownPool.Count > 0 ? CountdownPool.Pop() : new CountdownHandler();
        }

        private static StopwatchHandler RentStopwatch()
        {
            return StopwatchPool.Count > 0 ? StopwatchPool.Pop() : new StopwatchHandler();
        }

        private static void ReturnToPool(TimerHandlerBase timer)
        {
            timer.ResetForPool();

            if (timer is CountdownHandler cd)
            {
                if (CountdownPool.Count < _maxCountdownPoolSize)
                {
                    CountdownPool.Push(cd);
                }

                return;
            }

            if (timer is StopwatchHandler sw)
            {
                if (StopwatchPool.Count < _maxStopwatchPoolSize)
                {
                    StopwatchPool.Push(sw);
                }
            }
        }

        // Injects UpdateTimers into Unity's PlayerLoop Update phase.
        private static void InjectUpdateFunction(ref PlayerLoopSystem root)
        {
            var systems = root.subSystemList;
            if (systems == null) return;

            for (int i = 0; i < systems.Length; i++)
            {
                ref var sys = ref systems[i];
                if (sys.type != typeof(Update)) continue;

                var sub = sys.subSystemList;
                if (sub == null) sub = Array.Empty<PlayerLoopSystem>();

                var list = new List<PlayerLoopSystem>(sub.Length + 1);
                list.AddRange(sub);

                list.Insert(0, new PlayerLoopSystem
                {
                    type = typeof(TimerPlayerLoopDriver),
                    updateDelegate = UpdateTimers
                });

                sys.subSystemList = list.ToArray();
                root.subSystemList = systems;
                return;
            }
        }

        private static class FallbackBehaviour
        {
            private static GameObject _go;

            internal static void EnsureExists()
            {
                if (_go != null) return;

                _go = new GameObject("__TimerFallbackDriver")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };

                UnityEngine.Object.DontDestroyOnLoad(_go);
                _go.AddComponent<FallbackComponent>();
            }

            private sealed class FallbackComponent : MonoBehaviour
            {
                private void Update() => UpdateTimers();
                private void OnDestroy() => _go = null;
            }
        }

#if UNITY_EDITOR
        public enum TimerKind
        {
            Countdown,
            Stopwatch
        }

        public readonly struct TimerDebugInfo
        {
            public readonly TimerKind Kind;
            public readonly ulong Key;
            public readonly GameObject Owner;
            public readonly MonoBehaviour OwnerComponent;
            public readonly bool IsRunning;
            public readonly bool UseUnscaledTime;

            public readonly float TotalTime;
            public readonly float RemainingTime;
            public readonly float ElapsedTime;
            public readonly int CurrentLoopIteration;

            internal TimerDebugInfo(
                TimerKind kind,
                ulong key,
                GameObject owner,
                MonoBehaviour ownerComponent,
                bool isRunning,
                bool useUnscaledTime,
                float totalTime,
                float remainingTime,
                float elapsedTime,
                int currentLoopIteration)
            {
                Kind = kind;
                Key = key;
                Owner = owner;
                OwnerComponent = ownerComponent;
                IsRunning = isRunning;
                UseUnscaledTime = useUnscaledTime;
                TotalTime = totalTime;
                RemainingTime = remainingTime;
                ElapsedTime = elapsedTime;
                CurrentLoopIteration = currentLoopIteration;
            }

            public bool IsOwnerValid => OwnerComponent != null && Owner != null;

            public bool IsOwnerActiveAndEnabled
            {
                get
                {
                    if (!IsOwnerValid) return false;
                    return Owner.activeInHierarchy && OwnerComponent.isActiveAndEnabled;
                }
            }
        }

        public static int ActiveTimerCount => ActiveTimers.Count;
        public static int CountdownPoolCount => CountdownPool.Count;
        public static int StopwatchPoolCount => StopwatchPool.Count;
        public static int MaxPoolSize => Mathf.Max(_maxCountdownPoolSize, _maxStopwatchPoolSize);

        public static TimerDebugInfo[] GetActiveTimersSnapshot()
        {
            if (!Application.isPlaying) return Array.Empty<TimerDebugInfo>();

            var result = new List<TimerDebugInfo>(ActiveTimers.Count);

            for (int i = 0; i < ActiveTimers.Count; i++)
            {
                var timer = ActiveTimers[i];
                if (timer == null) continue;

                var ownerComponent = timer.OwnerComponent;
                var owner = ownerComponent != null ? ownerComponent.gameObject : null;

                var key = BuildKey(timer);

                if (timer is CountdownHandler cd)
                {
                    result.Add(new TimerDebugInfo(
                        TimerKind.Countdown,
                        key,
                        owner,
                        ownerComponent,
                        timer.IsRunning,
                        timer.UseUnscaledTime,
                        cd.TotalTime,
                        cd.RemainingTime,
                        0f,
                        cd.CurrentLoopIteration));
                    continue;
                }

                if (timer is StopwatchHandler sw)
                {
                    result.Add(new TimerDebugInfo(
                        TimerKind.Stopwatch,
                        key,
                        owner,
                        ownerComponent,
                        timer.IsRunning,
                        timer.UseUnscaledTime,
                        0f,
                        0f,
                        sw.ElapsedTime,
                        0));
                }
            }

            return result.ToArray();
        }

        public static void GetActiveTimersSnapshot(List<TimerDebugInfo> results)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));

            results.Clear();
            if (!Application.isPlaying) return;

            results.Capacity = Mathf.Max(results.Capacity, ActiveTimers.Count);

            for (int i = 0; i < ActiveTimers.Count; i++)
            {
                var timer = ActiveTimers[i];
                if (timer == null) continue;

                var ownerComponent = timer.OwnerComponent;
                var owner = ownerComponent != null ? ownerComponent.gameObject : null;

                var key = BuildKey(timer);

                if (timer is CountdownHandler cd)
                {
                    results.Add(new TimerDebugInfo(
                        TimerKind.Countdown,
                        key,
                        owner,
                        ownerComponent,
                        timer.IsRunning,
                        timer.UseUnscaledTime,
                        cd.TotalTime,
                        cd.RemainingTime,
                        0f,
                        cd.CurrentLoopIteration));
                    continue;
                }

                if (timer is StopwatchHandler sw)
                {
                    results.Add(new TimerDebugInfo(
                        TimerKind.Stopwatch,
                        key,
                        owner,
                        ownerComponent,
                        timer.IsRunning,
                        timer.UseUnscaledTime,
                        0f,
                        0f,
                        sw.ElapsedTime,
                        0));
                }
            }
        }

        private static ulong BuildKey(TimerHandlerBase timer)
        {
            unchecked
            {
                return ((ulong)(uint)timer.Slot << 32) | (uint)timer.Id;
            }
        }
#endif
    }
}

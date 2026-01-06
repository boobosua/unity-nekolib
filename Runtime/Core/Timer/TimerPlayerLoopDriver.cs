using System;
using System.Collections.Generic;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace NekoLib.Core
{
    /// <summary>
    /// Internal driver that integrates timers into Unity's PlayerLoop for efficient updating.
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    internal static class TimerPlayerLoopDriver
    {
        // Configuration
        private const int DefaultActiveCapacity = 128;
        private const int DefaultRemovalCapacity = 32;
        private const int DefaultPoolCapacity = 8;
        private const int DefaultMaxPoolSize = 128;

        private static readonly List<TimerBase> ActiveTimers = new(DefaultActiveCapacity);
        private static readonly List<TimerBase> ToRemove = new(DefaultRemovalCapacity);

        private static readonly Stack<Countdown> _countdownPool = new(DefaultPoolCapacity);
        private static readonly Stack<Stopwatch> _stopwatchPool = new(DefaultPoolCapacity);
        private static int _maxPoolSize = DefaultMaxPoolSize;

        private static bool _installed;
        private static bool _isUpdating;

        /// <summary>
        /// Registers a timer to be updated each frame.
        /// </summary>
        internal static void Register(TimerBase timer)
        {
            if (timer == null) return;
            if (!ActiveTimers.Contains(timer)) ActiveTimers.Add(timer);
        }

        /// <summary>
        /// Deregisters a timer so it no longer updates.
        /// </summary>
        internal static void Unregister(TimerBase timer)
        {
            if (timer == null) return;
            if (_isUpdating)
            {
                MarkForRemoval(timer);
                return;
            }
            if (ActiveTimers.Contains(timer)) ActiveTimers.Remove(timer);
        }

        private static void MarkForRemoval(TimerBase timer)
        {
            if (timer == null) return;
            if (!ToRemove.Contains(timer)) ToRemove.Add(timer);
        }

        internal static void SetMaxPoolSize(int maxPoolSize)
        {
            // Pooling is always enabled; this only increases the retention cap.
            if (maxPoolSize <= _maxPoolSize) return;
            _maxPoolSize = maxPoolSize;
        }

        /// <summary>
        /// Gets a Countdown timer from the pool or creates a new one.
        /// </summary>
        internal static Countdown GetCountdown(MonoBehaviour owner, float duration)
        {
            if (_countdownPool.Count > 0)
            {
                var cd = _countdownPool.Pop();
                cd.ReInitialize(owner, duration); // self-registers inside reinitialize
                return cd;
            }
            return new Countdown(owner, duration); // constructor self-registers
        }

        /// <summary>
        /// Gets a Stopwatch timer from the pool or creates a new one.
        /// </summary>
        internal static Stopwatch GetStopwatch(MonoBehaviour owner, Func<bool> stopCondition = null)
        {
            if (_stopwatchPool.Count > 0)
            {
                var sw = _stopwatchPool.Pop();
                sw.ReInitialize(owner, stopCondition); // self-registers
                return sw;
            }
            return new Stopwatch(owner, stopCondition); // constructor self-registers
        }

        /// <summary>
        /// Returns a timer to the pool for reuse.
        /// </summary>
        internal static void ReturnToPool(TimerBase timer)
        {
            if (timer is Countdown countdown && _countdownPool.Count < _maxPoolSize)
            {
                _countdownPool.Push(countdown);
            }
            else if (timer is Stopwatch stopwatch && _stopwatchPool.Count < _maxPoolSize)
            {
                _stopwatchPool.Push(stopwatch);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitDomain()
        {
            ActiveTimers.Clear();
            ToRemove.Clear();

            // Reset pools on domain reload
            _maxPoolSize = DefaultMaxPoolSize;
            _countdownPool.Clear();
            _stopwatchPool.Clear();

            _installed = false;
            TryInstall();
        }

        private static void TryInstall()
        {
            if (_installed) return;
            try
            {
                var loop = PlayerLoop.GetCurrentPlayerLoop();
                InjectUpdateFunction(ref loop, typeof(Update), UpdateTimers);
                PlayerLoop.SetPlayerLoop(loop);
                _installed = true;
            }
            catch (Exception ex)
            {
                Log.Error($"[TimerPlayerLoopDriver] Failed to inject into PlayerLoop: {ex.Message}. Falling back to Update via hidden GameObject.");
                FallbackBehaviour.EnsureExists();
            }
        }

        private static void InjectUpdateFunction(ref PlayerLoopSystem root, Type targetType, PlayerLoopSystem.UpdateFunction updateFn)
        {
            for (int i = 0; i < root.subSystemList?.Length; i++)
            {
                ref var sys = ref root.subSystemList[i];
                if (sys.type == targetType && sys.subSystemList != null)
                {
                    var list = new List<PlayerLoopSystem>(sys.subSystemList)
                    {
                        new() {
                            type = typeof(TimerPlayerLoopDriver),
                            updateDelegate = updateFn
                        }
                    };
                    sys.subSystemList = list.ToArray();
                    return;
                }
                if (sys.subSystemList != null)
                {
                    InjectUpdateFunction(ref sys, targetType, updateFn); // recurse
                }
            }
        }

        private static void UpdateTimers()
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
                    ActiveTimers.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!timer.IsOwnerValid)
                {
                    MarkForRemoval(timer);
                    continue;
                }

                if (!timer.IsOwnerActiveAndEnabled || !timer.IsRunning) continue;

                float dt = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                try
                {
                    timer.Tick(dt);
                }
                catch (Exception ex)
                {
                    Log.Error($"[TimerPlayerLoopDriver] Exception ticking timer: {ex}");
                    MarkForRemoval(timer);
                }
            }

            _isUpdating = false;

            if (ToRemove.Count > 0)
            {
                for (int i = 0; i < ToRemove.Count; i++)
                {
                    var t = ToRemove[i];
                    t?.Dispose();
                    if (ActiveTimers.Contains(t)) ActiveTimers.Remove(t);
                    ReturnToPool(t);
                }
                ToRemove.Clear();
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
                _go.AddComponent<FallbackComponent>();
            }

            private sealed class FallbackComponent : MonoBehaviour
            {
                private void Update() => UpdateTimers();
                private void OnDestroy() => _go = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gets a snapshot of all currently active timers (for editor/debugging purposes only).
        /// </summary>
        internal static List<TimerBase> GetActiveTimersSnapshot()
        {
            return new List<TimerBase>(ActiveTimers);
        }

        // Diagnostics for editor UI (TimerTrackerWindow)
        internal static int MaxPoolSize => _maxPoolSize;
        internal static int CountdownPoolCount => _countdownPool.Count;
        internal static int StopwatchPoolCount => _stopwatchPool.Count;
        internal static int ActiveTimerCount => ActiveTimers.Count;

        [UnityEditor.InitializeOnLoadMethod]
        private static void CleanupOnPlayModeExit()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                // Remove our injected system from PlayerLoop when exiting play mode
                try
                {
                    var loop = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveUpdateFunction(ref loop, typeof(TimerPlayerLoopDriver));
                    PlayerLoop.SetPlayerLoop(loop);
                    _installed = false;
                    Log.Info("[TimerPlayerLoopDriver] Removed from PlayerLoop on play mode exit.");
                }
                catch (Exception ex)
                {
                    Log.Error($"[TimerPlayerLoopDriver] Failed to cleanup PlayerLoop: {ex.Message}");
                }
            }
        }

        private static void RemoveUpdateFunction(ref PlayerLoopSystem root, Type typeToRemove)
        {
            if (root.subSystemList == null) return;

            var filtered = new List<PlayerLoopSystem>();
            foreach (var sys in root.subSystemList)
            {
                if (sys.type != typeToRemove)
                {
                    var copy = sys;
                    if (sys.subSystemList != null)
                    {
                        RemoveUpdateFunction(ref copy, typeToRemove);
                    }
                    filtered.Add(copy);
                }
            }
            root.subSystemList = filtered.ToArray();
        }
#endif
    }
}

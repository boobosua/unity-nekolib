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
    internal static class TimerPlayerLoopDriver
    {
        // Configuration
        private const int INITIAL_ACTIVE_CAPACITY = 128;
        private const int INITIAL_REMOVAL_CAPACITY = 32;

        private static readonly List<TimerBase> ActiveTimers = new(INITIAL_ACTIVE_CAPACITY);
        private static readonly List<TimerBase> ToRemove = new(INITIAL_REMOVAL_CAPACITY);

        // Pooling (disabled by default)
        private static bool _poolingEnabled = false;
        private static int _maxPoolSize = 0;
        private static Stack<Countdown> _countdownPool;
        private static Stack<Stopwatch> _stopwatchPool;

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
        /// Unregisters a timer so it no longer updates.
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
            if (ToRemove.Contains(timer)) ToRemove.Remove(timer);
        }

        private static void MarkForRemoval(TimerBase timer)
        {
            if (timer == null) return;
            if (!ToRemove.Contains(timer)) ToRemove.Add(timer);
        }

        /// <summary>
        /// Enables object pooling for timers with specified maximum pool size.
        /// </summary>
        internal static void EnablePooling(int maxPoolSize = 128)
        {
            if (_poolingEnabled) return;

            _maxPoolSize = Math.Max(16, maxPoolSize); // Minimum 16
            _countdownPool = new Stack<Countdown>(_maxPoolSize / 4);
            _stopwatchPool = new Stack<Stopwatch>(_maxPoolSize / 4);
            _poolingEnabled = true;

            Log.Info($"[TimerPlayerLoopDriver] Pooling enabled with max size: {_maxPoolSize}");
        }

        /// <summary>
        /// Gets a Countdown timer from the pool or creates a new one.
        /// </summary>
        internal static Countdown GetCountdown(MonoBehaviour owner, float duration)
        {
            if (_poolingEnabled && _countdownPool != null && _countdownPool.Count > 0)
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
            if (_poolingEnabled && _stopwatchPool != null && _stopwatchPool.Count > 0)
            {
                var sw = _stopwatchPool.Pop();
                sw.ReInitialize(owner, stopCondition); // self-registers
                return sw;
            }
            return new Stopwatch(owner, stopCondition); // constructor self-registers
        }

        /// <summary>
        /// Returns a timer to the pool for reuse (only if pooling is enabled).
        /// </summary>
        internal static void ReturnToPool(TimerBase timer)
        {
            if (!_poolingEnabled) return;

            if (timer is Countdown countdown && _countdownPool != null && _countdownPool.Count < _maxPoolSize)
            {
                _countdownPool.Push(countdown);
            }
            else if (timer is Stopwatch stopwatch && _stopwatchPool != null && _stopwatchPool.Count < _maxPoolSize)
            {
                _stopwatchPool.Push(stopwatch);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitDomain()
        {
            ActiveTimers.Clear();
            ToRemove.Clear();

            // Reset pooling state on domain reload
            _poolingEnabled = false;
            _countdownPool?.Clear();
            _stopwatchPool?.Clear();
            _countdownPool = null;
            _stopwatchPool = null;

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

                if (timer == null || !timer.IsOwnerValid)
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
        internal static bool IsPoolingEnabled => _poolingEnabled;
        internal static int MaxPoolSize => _maxPoolSize;
        internal static int CountdownPoolCount => _countdownPool?.Count ?? 0;
        internal static int StopwatchPoolCount => _stopwatchPool?.Count ?? 0;
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

        [UnityEditor.InitializeOnLoadMethod]
        private static void EditorInit()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.update -= EditorTick;
                UnityEditor.EditorApplication.update += EditorTick;
            }
        }

        private static void EditorTick()
        {
            if (Application.isPlaying) return; // runtime handled by PlayerLoop
            UpdateTimers();
        }
#endif
    }
}

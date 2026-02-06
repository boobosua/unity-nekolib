#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekoLib.Core
{
    internal static partial class TimerPlayerLoopDriver
    {
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
    }
}
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using NekoLib.Timer;

namespace NekoLib.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Creates a new Countdown timer owned by this MonoBehaviour.
        /// This version provides component-level monitoring for more precise lifecycle management.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour that will own the timer</param>
        /// <param name="duration">The countdown duration in seconds</param>
        /// <returns>A new Countdown timer</returns>
        public static Countdown CreateCountdown(this MonoBehaviour monoBehaviour, float duration)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Countdown(monoBehaviour, duration);
        }

        /// <summary>
        /// Creates a new Stopwatch timer owned by this MonoBehaviour.
        /// This version provides component-level monitoring for more precise lifecycle management.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour that will own the timer</param>
        /// <param name="stopCondition">Optional condition to automatically stop the stopwatch</param>
        /// <returns>A new Stopwatch timer</returns>
        public static Stopwatch CreateStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Stopwatch(monoBehaviour, stopCondition);
        }

        /// <summary>
        /// Gets all timers currently owned by this MonoBehaviour's GameObject.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to check</param>
        /// <returns>Collection of timers owned by this MonoBehaviour</returns>
        public static IEnumerable<TimerBase> GetTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
                return System.Linq.Enumerable.Empty<TimerBase>();

            return TimerManager.Instance.GetTimersForObject(monoBehaviour.gameObject);
        }

        /// <summary>
        /// Gets all timers specifically owned by this MonoBehaviour component.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to check</param>
        /// <returns>Collection of timers owned by this specific component</returns>
        public static IEnumerable<TimerBase> GetComponentTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
                return System.Linq.Enumerable.Empty<TimerBase>();

            return TimerManager.Instance.GetTimersForComponent(monoBehaviour);
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour's GameObject.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to clean up timers for</param>
        public static void CleanupTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            TimerManager.Instance.CleanupTimersForObject(monoBehaviour.gameObject);
        }

        /// <summary>
        /// Stops and removes all timers specifically owned by this MonoBehaviour component.
        /// </summary>
        /// <param name="monoBehaviour">The MonoBehaviour to clean up timers for</param>
        public static void CleanupComponentTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            TimerManager.Instance.CleanupTimersForComponent(monoBehaviour);
        }
    }
}

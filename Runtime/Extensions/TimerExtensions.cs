using System;
using UnityEngine;
using NekoLib.Utilities;

namespace NekoLib.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Creates a new Countdown timer owned by this MonoBehaviour
        /// </summary>
        public static Countdown CreateCountdown(this MonoBehaviour monoBehaviour, float duration)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Countdown(monoBehaviour, duration);
        }

        /// <summary>
        /// Creates a new Stopwatch timer owned by this MonoBehaviour
        /// </summary>
        public static Stopwatch CreateStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Stopwatch(monoBehaviour, stopCondition);
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour's GameObject
        /// </summary>
        public static void CleanupTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            if (TimerManager.HasInstance)
                TimerManager.Instance.CleanupTimersForObject(monoBehaviour.gameObject);
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour component
        /// </summary>
        public static void CleanupComponentTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            if (TimerManager.HasInstance)
                TimerManager.Instance.CleanupTimersForComponent(monoBehaviour);
        }
    }
}

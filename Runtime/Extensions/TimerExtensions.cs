using System;
using UnityEngine;
using NekoLib.Core;

namespace NekoLib.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Creates a new Countdown timer owned by this MonoBehaviour.
        /// </summary>
        public static Countdown CreateCountdown(this MonoBehaviour monoBehaviour, float duration)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Countdown(monoBehaviour, duration);
        }

        /// <summary>
        /// Starts a new Countdown timer owned by this MonoBehaviour.
        /// </summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration)
        {
            var countdown = CreateCountdown(monoBehaviour, duration);
            countdown.Start();
            return countdown;
        }

        /// <summary>
        /// Starts a new Countdown timer owned by this MonoBehaviour.
        /// </summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration, Func<bool> updateWhen)
        {
            var countdown = CreateCountdown(monoBehaviour, duration);
            if (updateWhen != null)
            {
                countdown.SetUpdateWhen(updateWhen);
            }
            countdown.Start();
            return countdown;
        }

        /// <summary>
        /// Creates a new Stopwatch timer owned by this MonoBehaviour.
        /// </summary>
        public static Stopwatch CreateStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            return new Stopwatch(monoBehaviour, stopCondition);
        }

        /// <summary>
        /// Starts a new Stopwatch timer owned by this MonoBehaviour.
        /// </summary>
        public static Stopwatch StartStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            var stopwatch = CreateStopwatch(monoBehaviour, stopCondition);
            stopwatch.Start();
            return stopwatch;
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour's GameObject.
        /// </summary>
        public static void CleanupTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null || TimerManager.HasInstance == false) return;
            TimerManager.Instance.CleanupTimersForObject(monoBehaviour.gameObject);
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour component.
        /// </summary>
        public static void CleanupComponentTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null || TimerManager.HasInstance == false) return;
            TimerManager.Instance.CleanupTimersForComponent(monoBehaviour);
        }
    }
}

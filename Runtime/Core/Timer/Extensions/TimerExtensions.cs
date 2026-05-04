using System;
using UnityEngine;

namespace NekoLib.Timer
{
    public static class TimerExtensions
    {
        /// <summary>Invokes <paramref name="action"/> once after <paramref name="delay"/> seconds.
        /// Returns a <see cref="TimerToken"/> to cancel the call before it fires.</summary>
        public static TimerToken Defer(this MonoBehaviour owner, float delay, Action action,
            bool useUnscaledTime = false)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (delay <= 0f)
            {
                action.Invoke();
                return default;
            }

            var cd = Countdown.Create(owner, delay)
                .SetUnscaledTime(useUnscaledTime)
                .OnElapsed(action);

            cd.Start();
            return cd.AsTimerToken();
        }

        /// <summary>Invokes <paramref name="action"/> repeatedly every <paramref name="interval"/> seconds.
        /// Returns a <see cref="TimerToken"/> to stop the loop permanently.</summary>
        public static TimerToken Recur(this MonoBehaviour owner, float interval, Action action,
            bool useUnscaledTime = false)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (interval <= 0f) throw new ArgumentException("Interval must be greater than zero.", nameof(interval));

            var cd = Countdown.Create(owner, interval)
                .SetUnscaledTime(useUnscaledTime)
                .SetLoop()
                .OnElapsed(action);

            cd.Start();
            return cd.AsTimerToken();
        }
    }
}

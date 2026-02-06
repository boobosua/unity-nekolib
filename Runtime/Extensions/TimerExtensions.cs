using System;
using NekoLib.Core;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>Gets a <see cref="Countdown"/> for configuring and creating a <see cref="Countdown"/>.</summary>
        public static Countdown GetCountdown(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Countdown.Create(monoBehaviour);
        }

        /// <summary>Gets a <see cref="Stopwatch"/> for configuring and creating a <see cref="Stopwatch"/>.</summary>
        public static Stopwatch GetStopwatch(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Stopwatch.Create(monoBehaviour);
        }

        /// <summary>Starts a simple countdown with the specified duration in seconds.</summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration = 1f)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            var cd = Countdown.Create(monoBehaviour).SetDuration(duration);
            cd.Start();
            return cd;
        }

        /// <summary>Starts a simple countdown with the specified duration in seconds and update condition.</summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration = 1f, Func<bool> updateWhen = null)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            var cd = Countdown.Create(monoBehaviour).SetDuration(duration).SetUpdateWhen(updateWhen);
            cd.Start();
            return cd;
        }

        /// <summary>Starts a simple stopwatch with an optional stop condition.</summary>
        public static Stopwatch StartStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            var sw = Stopwatch.Create(monoBehaviour, stopCondition);
            sw.Start();
            return sw;
        }

        /// <summary>Invokes an action after a delay in seconds.</summary>
        public static void InvokeAfterDelay(this MonoBehaviour monoBehaviour, float delay, Action action, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            if (delay <= 0f)
            {
                action?.Invoke();
                return;
            }

            var cd = Countdown.Create(monoBehaviour)
                .SetDuration(delay)
                .OnStop(() =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[{nameof(InvokeAfterDelay)}] Action threw: {ex}");
                    }
                })
                .SetUnscaledTime(useUnscaledTime);

            cd.Start();
        }

        /// <summary>Invokes an action repeatedly at specified intervals in seconds.</summary>
        public static CancelHandler InvokeEvery(this MonoBehaviour monoBehaviour, float interval, Action action, Func<bool> updateWhen = null, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            if (interval <= 0f) throw new ArgumentException("Interval must be greater than zero.", nameof(interval));

            var cd = Countdown.Create(monoBehaviour)
                .SetDuration(interval)
                .SetLoop()
                .OnLoop(() =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[{nameof(InvokeEvery)}] Action threw: {ex}");
                    }
                })
                .SetUnscaledTime(useUnscaledTime);

            if (updateWhen != null) cd = cd.SetUpdateWhen(updateWhen);

            cd.Start();
            return cd.AsCancelHandler();
        }
    }
}

using System;
using NekoLib.Core;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Extensions
{
    public static class TimerExtensions
    {
        /// <summary>Gets a <see cref="CountdownBuilder"/> for configuring and creating a <see cref="Countdown"/>.</summary>
        public static CountdownBuilder GetCountdown(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Countdown.Create(monoBehaviour);
        }

        /// <summary>Gets a <see cref="StopwatchBuilder"/> for configuring and creating a <see cref="Stopwatch"/>.</summary>
        public static StopwatchBuilder GetStopwatch(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Stopwatch.Create(monoBehaviour);
        }

        /// <summary>Starts a simple countdown with the specified duration in seconds.</summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration = 1f)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Countdown.Create(monoBehaviour).SetDuration(duration).Start();
        }

        /// <summary>Starts a simple countdown with the specified duration in seconds and update condition.</summary>
        public static Countdown StartCountdown(this MonoBehaviour monoBehaviour, float duration = 1f, Func<bool> updateWhen = null)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            return Countdown.Create(monoBehaviour).SetDuration(duration).SetUpdateWhen(updateWhen).Start();
        }

        /// <summary>Starts a simple stopwatch with an optional stop condition.</summary>
        public static Stopwatch StartStopwatch(this MonoBehaviour monoBehaviour, Func<bool> stopCondition = null)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));

            var builder = Stopwatch.Create(monoBehaviour);
            if (stopCondition != null) builder.SetStopCondition(stopCondition);

            return builder.Start();
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

            var builder = Countdown.Create(monoBehaviour)
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
                });

            builder.SetUnscaledTime(useUnscaledTime);

            builder.Start();
        }

        /// <summary>Invokes an action repeatedly at specified intervals in seconds.</summary>
        public static CancelHandler InvokeEvery(this MonoBehaviour monoBehaviour, float interval, Action action, Func<bool> updateWhen = null, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null) throw new ArgumentNullException(nameof(monoBehaviour));
            if (interval <= 0f) throw new ArgumentException("Interval must be greater than zero.", nameof(interval));

            var builder = Countdown.Create(monoBehaviour)
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
                });

            if (updateWhen != null) builder.SetUpdateWhen(updateWhen);
            builder.SetUnscaledTime(useUnscaledTime);

            var cd = builder.Start();
            return cd.AsCancelHandler();
        }
    }
}

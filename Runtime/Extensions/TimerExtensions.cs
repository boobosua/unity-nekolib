using System;
using NekoLib.Core;
using NekoLib.Logger;
using UnityEngine;

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
        /// Invokes the specified action after a delay.
        /// </summary>
        public static void InvokeDelayed(this MonoBehaviour monoBehaviour, float delay, Action action, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            if (delay <= 0f)
            {
                Log.Warn("InvokeAfter called with non-positive delay. Invoking action immediately.");
                action?.Invoke();
                return;
            }

            var countdown = CreateCountdown(monoBehaviour, delay);

            if (useUnscaledTime)
            {
                countdown.SetUnscaledTime();
            }

            countdown.OnStop += () =>
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error invoking action after delay: {ex}");
                }
                finally
                {
                    countdown?.Dispose();
                    countdown = null;
                }
            };

            countdown.Start();
        }

        /// <summary>
        /// Invokes the specified action repeatedly at the given interval.
        /// </summary>
        public static IDisposable InvokeEvery(this MonoBehaviour monoBehaviour, float interval, Action action, Func<bool> updateWhen = null, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            if (interval <= 0f)
            {
                throw new ArgumentException("Interval must be greater than zero.", nameof(interval));
            }

            if (interval < 0.02f)
            {
                Log.Warn("Interval too small for InvokeEvery. Consider using Update() or FixedUpdate() instead.");
            }

            var countdown = CreateCountdown(monoBehaviour, interval);

            if (useUnscaledTime)
            {
                countdown.SetUnscaledTime();
            }

            if (updateWhen != null)
            {
                countdown.SetUpdateWhen(updateWhen);
            }

            countdown.SetLoop();

            countdown.OnLoop += () =>
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error invoking action on interval: {ex}");
                }
            };

            countdown.Start();
            return countdown;
        }

        /// <summary>
        /// Invokes the specified action repeatedly at the given interval in seconds.
        /// </summary>
        public static IDisposable TickEverySeconds(this MonoBehaviour monoBehaviour, int intervalSeconds, int durationSeconds, Action<int> onTick, Action onStop, Func<bool> updateWhen = null, bool useUnscaledTime = false)
        {
            if (monoBehaviour == null)
                throw new ArgumentNullException(nameof(monoBehaviour));

            if (intervalSeconds <= 0)
            {
                throw new ArgumentException("Seconds must be greater than zero.", nameof(intervalSeconds));
            }

            if (durationSeconds <= 0)
            {
                throw new ArgumentException("Duration must be greater than zero.", nameof(durationSeconds));
            }

            var elapsedSeconds = 0;
            var countdown = CreateCountdown(monoBehaviour, intervalSeconds);

            countdown.SetLoop();

            if (useUnscaledTime)
            {
                countdown.SetUnscaledTime();
            }

            if (updateWhen != null)
            {
                countdown.SetUpdateWhen(updateWhen);
            }

            countdown.OnLoop += () =>
            {
                elapsedSeconds += intervalSeconds;
                try
                {
                    var tickSeconds = Mathf.Clamp(elapsedSeconds, 0, durationSeconds > 0 ? durationSeconds : int.MaxValue);
                    onTick?.Invoke(tickSeconds);
                }
                catch (Exception ex)
                {
                    Log.Error($"Error invoking action on seconds interval: {ex}");
                }
                finally
                {
                    if (durationSeconds > 0 && elapsedSeconds >= durationSeconds && countdown != null)
                    {
                        countdown.Stop();
                    }
                }
            };

            countdown.OnStop += () =>
            {
                try
                {
                    onStop?.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error invoking onStop action: {ex}");
                }
                finally
                {
                    countdown?.Dispose();
                    countdown = null;
                }
            };

            countdown.Start();
            return countdown;
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour's GameObject.
        /// </summary>
        public static void CleanupTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            if (monoBehaviour.TryGetComponent<TimerRegistry>(out var timerRegistry))
            {
                timerRegistry.CleanUpForObject(monoBehaviour.gameObject);
            }
        }

        /// <summary>
        /// Stops and removes all timers owned by this MonoBehaviour component.
        /// </summary>
        public static void CleanupComponentTimers(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null) return;

            if (monoBehaviour.TryGetComponent<TimerRegistry>(out var timerRegistry))
            {
                timerRegistry.CleanUpForComponent(monoBehaviour);
            }
        }
    }
}

using System;
using UnityEngine;

namespace NekoLib.Timer
{
    /// <summary>
    /// Factory class for creating timers with fluent builder pattern.
    /// </summary>
    public static class TimerFactory
    {
        /// <summary>
        /// Starts building a Countdown timer.
        /// </summary>
        /// <param name="owner">The MonoBehaviour that will own the timer</param>
        /// <returns>CountdownBuilder for configuring the countdown</returns>
        public static CountdownBuilder CreateCountdown(MonoBehaviour owner)
        {
            return new CountdownBuilder(owner);
        }

        /// <summary>
        /// Starts building a Stopwatch timer.
        /// </summary>
        /// <param name="owner">The MonoBehaviour that will own the timer</param>
        /// <returns>StopwatchBuilder for configuring the stopwatch</returns>
        public static StopwatchBuilder CreateStopwatch(MonoBehaviour owner)
        {
            return new StopwatchBuilder(owner);
        }
    }

    /// <summary>
    /// Builder for creating Countdown timers with fluent API.
    /// </summary>
    public class CountdownBuilder
    {
        private readonly MonoBehaviour _owner;
        private float _duration = 1f;
        private bool _useUnscaledTime = false;
        private LoopType _loopType = LoopType.None;
        private int _loopCount = 0;
        private Func<bool> _loopCondition = null;

        internal CountdownBuilder(MonoBehaviour owner)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Sets the countdown duration.
        /// </summary>
        /// <param name="duration">Duration in seconds</param>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetDuration(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            return this;
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetUnscaledTime()
        {
            _useUnscaledTime = true;
            return this;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale). This is the default.
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetScaledTime()
        {
            _useUnscaledTime = false;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop infinitely.
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetLoop()
        {
            _loopType = LoopType.Infinite;
            _loopCount = -1;
            _loopCondition = null;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop a specific number of times.
        /// </summary>
        /// <param name="count">Number of loops (0 = no loop, -1 = infinite)</param>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetLoop(int count)
        {
            if (count < -1)
                throw new ArgumentException("Loop count cannot be less than -1", nameof(count));

            if (count == 0)
            {
                _loopType = LoopType.None;
                _loopCount = 0;
            }
            else if (count == -1)
            {
                _loopType = LoopType.Infinite;
                _loopCount = -1;
            }
            else
            {
                _loopType = LoopType.Count;
                _loopCount = count;
            }

            _loopCondition = null;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop until a condition is met.
        /// </summary>
        /// <param name="stopCondition">Function that returns true when looping should stop</param>
        /// <returns>This builder for method chaining</returns>
        public CountdownBuilder SetLoop(Func<bool> stopCondition)
        {
            _loopCondition = stopCondition ?? throw new ArgumentNullException(nameof(stopCondition));
            _loopType = LoopType.Condition;
            _loopCount = 0;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured Countdown timer.
        /// </summary>
        /// <returns>A new Countdown timer with the specified configuration</returns>
        public Countdown Build()
        {
            var countdown = new Countdown(_owner, _duration);

            if (_useUnscaledTime)
                countdown.SetUnscaledTime();

            // Apply loop configuration
            switch (_loopType)
            {
                case LoopType.None:
                    countdown.SetLoop(0);
                    break;
                case LoopType.Infinite:
                    countdown.SetLoop(-1);
                    break;
                case LoopType.Count:
                    countdown.SetLoop(_loopCount);
                    break;
                case LoopType.Condition:
                    countdown.SetLoop(_loopCondition);
                    break;
            }

            return countdown;
        }
    }

    /// <summary>
    /// Builder for creating Stopwatch timers with fluent API.
    /// </summary>
    public class StopwatchBuilder
    {
        private readonly MonoBehaviour _owner;
        private bool _useUnscaledTime = false;
        private Func<bool> _stopCondition = null;

        internal StopwatchBuilder(MonoBehaviour owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public StopwatchBuilder SetUnscaledTime()
        {
            _useUnscaledTime = true;
            return this;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale). This is the default.
        /// </summary>
        /// <returns>This builder for method chaining</returns>
        public StopwatchBuilder SetScaledTime()
        {
            _useUnscaledTime = false;
            return this;
        }

        /// <summary>
        /// Sets an automatic stop condition for the stopwatch.
        /// </summary>
        /// <param name="stopCondition">Function that returns true when the stopwatch should stop</param>
        /// <returns>This builder for method chaining</returns>
        public StopwatchBuilder SetStopCondition(Func<bool> stopCondition)
        {
            _stopCondition = stopCondition;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured Stopwatch timer.
        /// </summary>
        /// <returns>A new Stopwatch timer with the specified configuration</returns>
        public Stopwatch Build()
        {
            var stopwatch = new Stopwatch(_owner, _stopCondition);

            if (_useUnscaledTime)
                stopwatch.SetUnscaledTime();

            return stopwatch;
        }
    }
}

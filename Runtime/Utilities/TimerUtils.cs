using System;
using NekoLib.Core;
using UnityEngine;

namespace NekoLib.Utilities
{
    public static partial class Utils
    {
        /// <summary>
        /// Enables pooling for Countdown and Stopwatch timers to reduce memory allocations.
        /// </summary>
        public static void EnableTimerPooling(int maxPoolSize = 128)
        {
            TimerPlayerLoopDriver.EnablePooling(maxPoolSize);
        }

        /// <summary>
        /// Starts building a Countdown timer.
        /// </summary>
        public static CountdownBuilder CreateCountdown(MonoBehaviour owner)
        {
            return new CountdownBuilder(owner);
        }

        /// <summary>
        /// Starts building a Stopwatch timer.
        /// </summary>
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
        private Func<bool> _updateCondition = null;
        private Func<bool> _loopStopCondition = null;

        internal CountdownBuilder(MonoBehaviour owner)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Sets the condition for updating the timer.
        /// </summary>
        public CountdownBuilder SetUpdateWhen(Func<bool> updateCondition)
        {
            _updateCondition = updateCondition ?? throw new ArgumentNullException(nameof(updateCondition));
            return this;
        }

        /// <summary>
        /// Sets the countdown duration.
        /// </summary>
        public CountdownBuilder SetDuration(float duration)
        {
            _duration = Mathf.Max(0f, duration);
            return this;
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// </summary>
        public CountdownBuilder SetUnscaledTime()
        {
            _useUnscaledTime = true;
            return this;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale). This is the default.
        /// </summary>
        public CountdownBuilder SetScaledTime()
        {
            _useUnscaledTime = false;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop infinitely.
        /// </summary>
        public CountdownBuilder SetLoop()
        {
            _loopType = LoopType.Infinite;
            _loopCount = -1;
            _loopStopCondition = null;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop a specific number of times.
        /// </summary>
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

            _loopStopCondition = null;
            return this;
        }

        /// <summary>
        /// Sets the countdown to loop until a condition is met.
        /// </summary>
        public CountdownBuilder SetLoop(Func<bool> stopCondition)
        {
            _loopStopCondition = stopCondition ?? throw new ArgumentNullException(nameof(stopCondition));
            _loopType = LoopType.Condition;
            _loopCount = 0;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured Countdown timer.
        /// </summary>
        public Countdown Build()
        {
            var countdown = TimerPlayerLoopDriver.GetCountdown(_owner, _duration);

            if (_useUnscaledTime)
                countdown.SetUnscaledTime();

            if (_updateCondition != null)
            {
                countdown.SetUpdateWhen(_updateCondition);
            }

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
                    countdown.SetLoop(_loopStopCondition);
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
        private Func<bool> _updateCondition = null;
        private Func<bool> _stopCondition = null;

        internal StopwatchBuilder(MonoBehaviour owner)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Sets the condition for updating the stopwatch.
        /// </summary>
        public StopwatchBuilder SetUpdateWhen(Func<bool> updateCondition)
        {
            _updateCondition = updateCondition ?? throw new ArgumentNullException(nameof(updateCondition));
            return this;
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// </summary>
        public StopwatchBuilder SetUnscaledTime()
        {
            _useUnscaledTime = true;
            return this;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale). This is the default.
        /// </summary>
        public StopwatchBuilder SetScaledTime()
        {
            _useUnscaledTime = false;
            return this;
        }

        /// <summary>
        /// Sets an automatic stop condition for the stopwatch.
        /// </summary>
        public StopwatchBuilder SetStopCondition(Func<bool> stopCondition)
        {
            _stopCondition = stopCondition ?? throw new ArgumentNullException(nameof(stopCondition));
            return this;
        }

        /// <summary>
        /// Builds and returns the configured Stopwatch timer.
        /// </summary>
        public Stopwatch Build()
        {
            var stopwatch = TimerPlayerLoopDriver.GetStopwatch(_owner, _stopCondition);

            if (_useUnscaledTime)
                stopwatch.SetUnscaledTime();

            if (_updateCondition != null)
            {
                stopwatch.SetUpdateWhen(_updateCondition);
            }

            return stopwatch;
        }
    }
}
using System;
using NekoLib.Extensions;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>Configures and creates a <see cref="Countdown"/>.</summary>
    public sealed class CountdownBuilder
    {
        private readonly MonoBehaviour _owner;

        private float _duration = 1f;
        private bool _useUnscaledTime;
        private Func<bool> _updateWhen;

        private LoopType _loopType = LoopType.None;
        private int _loopCount;
        private Func<bool> _loopStopCondition;

        private Action _onStart;
        private Action<float> _onUpdate;
        private Action _onLoop;
        private Action _onStop;

        internal CountdownBuilder(MonoBehaviour owner)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>Sets the countdown duration in seconds (clamped to a small, non-zero minimum).</summary>
        public CountdownBuilder SetDuration(float duration = 1f)
        {
            _duration = duration.AtLeast(0f);
            return this;
        }

        /// <summary>Sets an optional predicate that must be true for the timer to tick.</summary>
        public CountdownBuilder SetUpdateWhen(Func<bool> updateCondition)
        {
            _updateWhen = updateCondition ?? throw new ArgumentNullException(nameof(updateCondition));
            return this;
        }

        /// <summary>Sets whether this countdown uses unscaled time (true) or scaled time (false).</summary>
        public CountdownBuilder SetUnscaledTime(bool useUnscaledTime)
        {
            _useUnscaledTime = useUnscaledTime;
            return this;
        }

        /// <summary>Sets looping by count (-1 infinite, 0 none, &gt; 0 fixed count).</summary>
        public CountdownBuilder SetLoop(int loopCount = -1)
        {
            if (loopCount < -1)
                throw new ArgumentException("Loop count cannot be less than -1", nameof(loopCount));

            if (loopCount == 0)
            {
                _loopType = LoopType.None;
                _loopCount = 0;
                _loopStopCondition = null;
                return this;
            }

            if (loopCount == -1)
            {
                _loopType = LoopType.Infinite;
                _loopCount = -1;
                _loopStopCondition = null;
                return this;
            }

            _loopType = LoopType.Count;
            _loopCount = loopCount;
            _loopStopCondition = null;
            return this;
        }

        /// <summary>Configures looping until the predicate returns true (stop condition).</summary>
        public CountdownBuilder SetLoop(Func<bool> stopWhen)
        {
            _loopStopCondition = stopWhen ?? throw new ArgumentNullException(nameof(stopWhen));
            _loopType = LoopType.Condition;
            _loopCount = 0;
            return this;
        }

        /// <summary>Adds a callback invoked when the countdown starts.</summary>
        public CountdownBuilder OnStart(Action callback)
        {
            _onStart += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Adds a callback invoked on each tick with remaining seconds.</summary>
        public CountdownBuilder OnUpdate(Action<float> callback)
        {
            _onUpdate += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Adds a callback invoked when a loop iteration restarts.</summary>
        public CountdownBuilder OnLoop(Action callback)
        {
            _onLoop += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Adds a callback invoked when the countdown stops naturally.</summary>
        public CountdownBuilder OnStop(Action callback)
        {
            _onStop += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Builds the countdown without starting it.</summary>
        public Countdown Build()
        {
            var handler = TimerPlayerLoopDriver.CreateCountdown(_owner, _duration);

            if (_useUnscaledTime) TimerPlayerLoopDriver.SetUnscaledTime(handler);
            else TimerPlayerLoopDriver.SetScaledTime(handler);

            if (_updateWhen != null) TimerPlayerLoopDriver.SetUpdateWhen(handler, _updateWhen);

            switch (_loopType)
            {
                case LoopType.None:
                    TimerPlayerLoopDriver.SetCountdownLoopCount(handler, 0);
                    break;

                case LoopType.Infinite:
                    TimerPlayerLoopDriver.SetCountdownLoopCount(handler, -1);
                    break;

                case LoopType.Count:
                    TimerPlayerLoopDriver.SetCountdownLoopCount(handler, _loopCount);
                    break;

                case LoopType.Condition:
                    TimerPlayerLoopDriver.SetCountdownLoopCondition(handler, _loopStopCondition);
                    break;
            }

            if (_onStart != null) TimerPlayerLoopDriver.AddOnStart(handler, _onStart);
            if (_onUpdate != null) TimerPlayerLoopDriver.AddOnUpdate(handler, _onUpdate);
            if (_onLoop != null) TimerPlayerLoopDriver.AddOnLoop(handler, _onLoop);
            if (_onStop != null) TimerPlayerLoopDriver.AddOnStop(handler, _onStop);

            return handler;
        }

        /// <summary>Builds and starts the countdown.</summary>
        public Countdown Start()
        {
            var handler = Build();
            handler.Start();
            return handler;
        }
    }
}

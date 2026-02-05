using System;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>Configures and creates a <see cref="Stopwatch"/>.</summary>
    public sealed class StopwatchBuilder
    {
        private readonly MonoBehaviour _owner;

        private bool _useUnscaledTime;
        private Func<bool> _updateWhen;
        private Func<bool> _stopCondition;

        private Action _onStart;
        private Action<float> _onUpdate;
        private Action _onStop;

        internal StopwatchBuilder(MonoBehaviour owner)
        {
            _owner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>Sets an optional predicate that must be true for the timer to tick.</summary>
        public StopwatchBuilder SetUpdateWhen(Func<bool> updateCondition)
        {
            _updateWhen = updateCondition ?? throw new ArgumentNullException(nameof(updateCondition));
            return this;
        }

        /// <summary>Sets an optional predicate that stops the stopwatch when it becomes true.</summary>
        public StopwatchBuilder SetStopCondition(Func<bool> stopCondition)
        {
            _stopCondition = stopCondition ?? throw new ArgumentNullException(nameof(stopCondition));
            return this;
        }

        /// <summary>Sets whether this stopwatch uses unscaled time (true) or scaled time (false).</summary>
        public StopwatchBuilder SetUnscaledTime(bool useUnscaledTime)
        {
            _useUnscaledTime = useUnscaledTime;
            return this;
        }

        /// <summary>Adds a callback invoked when the stopwatch starts.</summary>
        public StopwatchBuilder OnStart(Action callback)
        {
            _onStart += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Adds a callback invoked on each tick with elapsed seconds.</summary>
        public StopwatchBuilder OnUpdate(Action<float> callback)
        {
            _onUpdate += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Adds a callback invoked when the stopwatch stops naturally.</summary>
        public StopwatchBuilder OnStop(Action callback)
        {
            _onStop += callback ?? throw new ArgumentNullException(nameof(callback));
            return this;
        }

        /// <summary>Builds the stopwatch without starting it.</summary>
        public Stopwatch Build()
        {
            var handler = TimerPlayerLoopDriver.CreateStopwatch(_owner, _stopCondition);

            if (_useUnscaledTime) TimerPlayerLoopDriver.SetUnscaledTime(handler);
            else TimerPlayerLoopDriver.SetScaledTime(handler);

            if (_updateWhen != null) TimerPlayerLoopDriver.SetUpdateWhen(handler, _updateWhen);

            if (_onStart != null) TimerPlayerLoopDriver.AddOnStart(handler, _onStart);
            if (_onUpdate != null) TimerPlayerLoopDriver.AddOnUpdate(handler, _onUpdate);
            if (_onStop != null) TimerPlayerLoopDriver.AddOnStop(handler, _onStop);

            return handler;
        }

        /// <summary>Builds and starts the stopwatch.</summary>
        public Stopwatch Start()
        {
            var handler = Build();
            handler.Start();
            return handler;
        }
    }
}

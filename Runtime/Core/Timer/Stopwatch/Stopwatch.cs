using System;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>
    /// A timer that measures elapsed time until stopped.
    /// </summary>
    public readonly struct Stopwatch
    {
        private readonly int _slot;
        private readonly int _id;

        internal Stopwatch(int slot, int id)
        {
            _slot = slot;
            _id = id;
        }

        internal int Slot => _slot;
        internal int Id => _id;

        /// <summary>Creates a stopwatch owned by <paramref name="owner"/>. You still need to call <see cref="Start"/>.</summary>
        public static Stopwatch Create(MonoBehaviour owner)
        {
            return TimerPlayerLoopDriver.CreateStopwatch(owner, stopCondition: null);
        }

        /// <summary>Creates a stopwatch owned by <paramref name="owner"/> with a <paramref name="stopCondition"/>. You still need to call <see cref="Start"/>.</summary>
        public static Stopwatch Create(MonoBehaviour owner, Func<bool> stopCondition)
        {
            return TimerPlayerLoopDriver.CreateStopwatch(owner, stopCondition);
        }

        /// <summary>Returns true while this stopwatch exists (not stopped/cancelled and owner still valid).</summary>
        public bool IsAlive => TimerPlayerLoopDriver.IsAlive(_slot, _id);
        /// <summary>Returns true if the stopwatch is currently ticking.</summary>
        public bool IsRunning => TimerPlayerLoopDriver.IsRunning(_slot, _id);
        /// <summary>Returns true if the stopwatch exists but is not running.</summary>
        public bool IsPaused => TimerPlayerLoopDriver.IsPaused(_slot, _id);

        /// <summary>Gets the elapsed time in seconds.</summary>
        public float ElapsedTime => TimerPlayerLoopDriver.GetStopwatchElapsedTime(_slot, _id);

        /// <summary>Sets an optional predicate that must be true for the timer to tick.</summary>
        public Stopwatch SetUpdateWhen(Func<bool> updateCondition)
        {
            TimerPlayerLoopDriver.SetUpdateWhen(_slot, _id, updateCondition);
            return this;
        }

        /// <summary>Sets an optional predicate that stops the stopwatch when it becomes true.</summary>
        public Stopwatch SetStopCondition(Func<bool> stopCondition)
        {
            TimerPlayerLoopDriver.SetStopwatchStopCondition(_slot, _id, stopCondition);
            return this;
        }

        /// <summary>Sets whether this stopwatch uses unscaled time (true) or scaled time (false).</summary>
        public Stopwatch SetUnscaledTime(bool useUnscaledTime)
        {
            TimerPlayerLoopDriver.SetUnscaledTime(_slot, _id, useUnscaledTime);
            return this;
        }

        /// <summary>Adds a callback invoked when the stopwatch starts.</summary>
        public Stopwatch OnStart(Action callback)
        {
            TimerPlayerLoopDriver.AddOnStart(_slot, _id, callback);
            return this;
        }

        /// <summary>Adds a callback invoked on each tick with elapsed seconds.</summary>
        public Stopwatch OnUpdate(Action<float> callback)
        {
            TimerPlayerLoopDriver.AddOnUpdate(_slot, _id, callback);
            return this;
        }

        /// <summary>Adds a callback invoked when the stopwatch stops naturally.</summary>
        public Stopwatch OnStop(Action callback)
        {
            TimerPlayerLoopDriver.AddOnStop(_slot, _id, callback);
            return this;
        }

        /// <summary>Starts the stopwatch.</summary>
        public void Start() => TimerPlayerLoopDriver.Start(_slot, _id);
        /// <summary>Pauses the stopwatch.</summary>
        public void Pause() => TimerPlayerLoopDriver.Pause(_slot, _id);
        /// <summary>Resumes the stopwatch if paused.</summary>
        public void Resume() => TimerPlayerLoopDriver.Resume(_slot, _id);

        /// <summary>Stops the stopwatch and invokes stop callbacks.</summary>
        public void Stop() => TimerPlayerLoopDriver.Stop(_slot, _id);
        /// <summary>Cancels the stopwatch without invoking stop callbacks.</summary>
        public void Cancel() => TimerPlayerLoopDriver.Cancel(_slot, _id);
    }
}

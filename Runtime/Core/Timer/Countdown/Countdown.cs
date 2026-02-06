using System;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>
    /// A timer that counts down from a specified duration to zero.
    /// </summary>
    public readonly struct Countdown
    {
        private readonly int _slot;
        private readonly int _id;

        internal Countdown(int slot, int id)
        {
            _slot = slot;
            _id = id;
        }

        internal int Slot => _slot;
        internal int Id => _id;

        /// <summary>Creates a countdown owned by <paramref name="owner"/> with default duration (1s). You still need to call <see cref="Start"/>.</summary>
        public static Countdown Create(MonoBehaviour owner)
        {
            return TimerPlayerLoopDriver.CreateCountdown(owner, 1f);
        }

        /// <summary>Creates a countdown owned by <paramref name="owner"/> with the provided <paramref name="duration"/>. You still need to call <see cref="Start"/>.</summary>
        public static Countdown Create(MonoBehaviour owner, float duration)
        {
            return TimerPlayerLoopDriver.CreateCountdown(owner, duration);
        }

        /// <summary>Returns true while this countdown exists (not stopped/cancelled and owner still valid).</summary>
        public bool IsAlive => TimerPlayerLoopDriver.IsAlive(_slot, _id);
        /// <summary>Returns true if the countdown is currently ticking.</summary>
        public bool IsRunning => TimerPlayerLoopDriver.IsRunning(_slot, _id);
        /// <summary>Returns true if the countdown exists but is not running.</summary>
        public bool IsPaused => TimerPlayerLoopDriver.IsPaused(_slot, _id);

        /// <summary>Gets the remaining time in seconds.</summary>
        public float RemainingTime => TimerPlayerLoopDriver.GetCountdownRemainingTime(_slot, _id);
        /// <summary>Gets the configured duration in seconds.</summary>
        public float TotalTime => TimerPlayerLoopDriver.GetCountdownTotalTime(_slot, _id);
        /// <summary>Gets the current loop iteration for looping countdowns.</summary>
        public int CurrentLoopIteration => TimerPlayerLoopDriver.GetCountdownLoopIteration(_slot, _id);

        /// <summary>Sets the countdown duration in seconds (clamped to &gt;= 0).</summary>
        public Countdown SetDuration(float duration)
        {
            TimerPlayerLoopDriver.SetCountdownTotalTime(_slot, _id, duration);
            return this;
        }

        /// <summary>Sets an optional predicate that must be true for the timer to tick.</summary>
        public Countdown SetUpdateWhen(Func<bool> updateCondition)
        {
            TimerPlayerLoopDriver.SetUpdateWhen(_slot, _id, updateCondition);
            return this;
        }

        /// <summary>Sets whether this countdown uses unscaled time (true) or scaled time (false).</summary>
        public Countdown SetUnscaledTime(bool useUnscaledTime)
        {
            TimerPlayerLoopDriver.SetUnscaledTime(_slot, _id, useUnscaledTime);
            return this;
        }

        /// <summary>Sets looping by count (-1 infinite, 0 none, &gt; 0 fixed count).</summary>
        public Countdown SetLoop(int loopCount = -1)
        {
            TimerPlayerLoopDriver.SetCountdownLoopCount(_slot, _id, loopCount);
            return this;
        }

        /// <summary>Configures looping until the predicate returns true (stop condition).</summary>
        public Countdown SetLoop(Func<bool> stopWhen)
        {
            TimerPlayerLoopDriver.SetCountdownLoopCondition(_slot, _id, stopWhen);
            return this;
        }

        /// <summary>Adds a callback invoked when the countdown starts.</summary>
        public Countdown OnStart(Action callback)
        {
            TimerPlayerLoopDriver.AddOnStart(_slot, _id, callback);
            return this;
        }

        /// <summary>Adds a callback invoked on each tick with remaining seconds.</summary>
        public Countdown OnUpdate(Action<float> callback)
        {
            TimerPlayerLoopDriver.AddOnUpdate(_slot, _id, callback);
            return this;
        }

        /// <summary>Adds a callback invoked when a loop iteration restarts.</summary>
        public Countdown OnLoop(Action callback)
        {
            TimerPlayerLoopDriver.AddOnLoop(_slot, _id, callback);
            return this;
        }

        /// <summary>Adds a callback invoked when the countdown stops naturally.</summary>
        public Countdown OnStop(Action callback)
        {
            TimerPlayerLoopDriver.AddOnStop(_slot, _id, callback);
            return this;
        }

        /// <summary>Starts the countdown.</summary>
        public void Start() => TimerPlayerLoopDriver.Start(_slot, _id);
        /// <summary>Pauses the countdown.</summary>
        public void Pause() => TimerPlayerLoopDriver.Pause(_slot, _id);
        /// <summary>Resumes the countdown if paused.</summary>
        public void Resume() => TimerPlayerLoopDriver.Resume(_slot, _id);

        /// <summary>Stops the countdown and invokes stop callbacks (if any were registered).</summary>
        public void Stop() => TimerPlayerLoopDriver.Stop(_slot, _id);
        /// <summary>Cancels the countdown without invoking stop callbacks.</summary>
        public void Cancel() => TimerPlayerLoopDriver.Cancel(_slot, _id);

        /// <summary>Adds time in seconds to the remaining time.</summary>
        public void AddTime(float seconds) => TimerPlayerLoopDriver.AddCountdownTime(_slot, _id, seconds);
        /// <summary>Reduces remaining time in seconds.</summary>
        public void ReduceTime(float seconds) => TimerPlayerLoopDriver.ReduceCountdownTime(_slot, _id, seconds);
        /// <summary>Converts this countdown to a <see cref="CancelHandler"/> for cancelling without invoking stop callbacks.</summary>
        public CancelHandler AsCancelHandler() => new(_slot, _id);
    }
}

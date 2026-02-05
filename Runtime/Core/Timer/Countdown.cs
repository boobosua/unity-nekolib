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

        /// <summary>Starts building a countdown owned by <paramref name="owner"/>.</summary>
        public static CountdownBuilder Create(MonoBehaviour owner)
        {
            return new CountdownBuilder(owner);
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

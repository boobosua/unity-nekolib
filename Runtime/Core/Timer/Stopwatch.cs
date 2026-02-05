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

        /// <summary>Starts building a stopwatch owned by <paramref name="owner"/>.</summary>
        public static StopwatchBuilder Create(MonoBehaviour owner)
        {
            return new StopwatchBuilder(owner);
        }

        /// <summary>Returns true while this stopwatch exists (not stopped/cancelled and owner still valid).</summary>
        public bool IsAlive => TimerPlayerLoopDriver.IsAlive(_slot, _id);
        /// <summary>Returns true if the stopwatch is currently ticking.</summary>
        public bool IsRunning => TimerPlayerLoopDriver.IsRunning(_slot, _id);
        /// <summary>Returns true if the stopwatch exists but is not running.</summary>
        public bool IsPaused => TimerPlayerLoopDriver.IsPaused(_slot, _id);

        /// <summary>Gets the elapsed time in seconds.</summary>
        public float ElapsedTime => TimerPlayerLoopDriver.GetStopwatchElapsedTime(_slot, _id);

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

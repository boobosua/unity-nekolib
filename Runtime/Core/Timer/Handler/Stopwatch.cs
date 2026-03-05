using System;
using UnityEngine;

namespace NekoLib.Timer
{
    /// <summary>A timer that measures elapsed time until stopped.</summary>
    public readonly struct Stopwatch
    {
        private readonly TimerHandle _handle;

        internal Stopwatch(TimerHandle handle) => _handle = handle;

        /// <summary>Creates a stopwatch owned by <paramref name="owner"/> (call <see cref="Start"/> to begin).</summary>
        public static Stopwatch Create(MonoBehaviour owner)
        {
            return new Stopwatch(TimerWorld.CreateStopwatch(owner));
        }

        /// <summary>Returns true while this stopwatch exists and its owner is still valid.</summary>
        public bool IsAlive => TimerWorld.IsAlive(_handle);

        /// <summary>Returns true if the stopwatch is currently ticking.</summary>
        public bool IsRunning => TimerWorld.IsRunning(_handle);

        /// <summary>Returns true if the stopwatch exists but is currently paused.</summary>
        public bool IsPaused => TimerWorld.IsPaused(_handle);

        /// <summary>Gets elapsed seconds (0 if not alive).</summary>
        public float ElapsedTime => TimerWorld.GetStopwatchElapsed(_handle);

        /// <summary>Sets whether this stopwatch uses unscaled time.</summary>
        public Stopwatch SetUnscaledTime(bool useUnscaledTime) { TimerWorld.SetUnscaledTime(_handle, useUnscaledTime); return this; }

        /// <summary>Ticks only while <paramref name="predicate"/> is true.</summary>
        public Stopwatch OnUpdateWhen(Func<bool> predicate) { TimerWorld.SetUpdateWhen(_handle, predicate); return this; }

        /// <summary>Ticks only while <paramref name="predicate"/> is true (non-capturing style).</summary>
        public Stopwatch OnUpdateWhen<T>(T target, Func<T, bool> predicate) where T : class { TimerWorld.SetUpdateWhen(_handle, target, predicate); return this; }

        /// <summary>Stops when <paramref name="stopWhen"/> becomes true.</summary>
        public Stopwatch SetStopWhen(Func<bool> stopWhen) { TimerWorld.SetStopwatchStopCondition(_handle, stopWhen); return this; }

        /// <summary>Stops when <paramref name="stopWhen"/> becomes true (non-capturing style).</summary>
        public Stopwatch SetStopWhen<T>(T target, Func<T, bool> stopWhen) where T : class { TimerWorld.SetStopwatchStopCondition(_handle, target, stopWhen); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the stopwatch starts.</summary>
        public Stopwatch OnStart(Action callback) { TimerWorld.SetOnStart(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the stopwatch starts (non-capturing style).</summary>
        public Stopwatch OnStart<T>(T target, Action<T> callback) where T : class { TimerWorld.SetOnStart(_handle, target, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> every tick with elapsed seconds.</summary>
        public Stopwatch OnUpdate(Action<float> callback) { TimerWorld.SetOnUpdate(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> every tick with elapsed seconds (non-capturing style).</summary>
        public Stopwatch OnUpdate<T>(T target, Action<T, float> callback) where T : class { TimerWorld.SetOnUpdate(_handle, target, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the stopwatch stops naturally.</summary>
        public Stopwatch OnStop(Action callback) { TimerWorld.SetOnStop(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the stopwatch stops naturally (non-capturing style).</summary>
        public Stopwatch OnStop<T>(T target, Action<T> callback) where T : class { TimerWorld.SetOnStop(_handle, target, callback); return this; }

        /// <summary>Starts the stopwatch.</summary>
        public void Start() => TimerWorld.Start(_handle);

        /// <summary>Pauses the stopwatch.</summary>
        public void Pause() => TimerWorld.Pause(_handle);

        /// <summary>Resumes the stopwatch.</summary>
        public void Resume() => TimerWorld.Resume(_handle);

        /// <summary>Stops the stopwatch and invokes stop callbacks.</summary>
        public void Stop() => TimerWorld.Stop(_handle);

        /// <summary>Cancels the stopwatch without invoking stop callbacks.</summary>
        public void Cancel() => TimerWorld.Cancel(_handle);

        /// <summary>Returns concise debugging info.</summary>
        public override string ToString()
        {
            if (!IsAlive) return "Stopwatch{dead}";
            return $"Stopwatch{{t={ElapsedTime:0.###}, run={IsRunning}}}";
        }
    }
}

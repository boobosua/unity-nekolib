using System;
using UnityEngine;

namespace NekoLib.Timer
{
    /// <summary>A timer that counts down from a duration to zero.</summary>
    public readonly struct Countdown
    {
        private readonly TimerHandle _handle;

        internal Countdown(TimerHandle handle) => _handle = handle;

        /// <summary>Creates a countdown owned by <paramref name="owner"/> (call <see cref="Start"/> to begin).</summary>
        public static Countdown Create(MonoBehaviour owner, float durationSeconds)
        {
            return new Countdown(TimerWorld.CreateCountdown(owner, durationSeconds));
        }

        /// <summary>Returns true while this countdown exists and its owner is still valid.</summary>
        public bool IsAlive => TimerWorld.IsAlive(_handle);

        /// <summary>Returns true if the countdown is currently ticking.</summary>
        public bool IsRunning => TimerWorld.IsRunning(_handle);

        /// <summary>Returns true if the countdown exists but is currently paused.</summary>
        public bool IsPaused => TimerWorld.IsPaused(_handle);

        /// <summary>Gets the remaining seconds (0 if not alive).</summary>
        public float RemainingTime => TimerWorld.GetCountdownRemaining(_handle);

        /// <summary>Gets the configured duration seconds (0 if not alive).</summary>
        public float TotalTime => TimerWorld.GetCountdownTotal(_handle);

        /// <summary>Gets the current loop iteration (0 if not alive).</summary>
        public int CurrentLoopIteration => TimerWorld.GetCountdownLoopIteration(_handle);

        /// <summary>Sets whether this countdown uses unscaled time.</summary>
        public Countdown SetUnscaledTime(bool useUnscaledTime) { TimerWorld.SetUnscaledTime(_handle, useUnscaledTime); return this; }

        /// <summary>Ticks only while <paramref name="predicate"/> is true.</summary>
        public Countdown OnUpdateWhen(Func<bool> predicate) { TimerWorld.SetUpdateWhen(_handle, predicate); return this; }

        /// <summary>Ticks only while <paramref name="predicate"/> is true (non-capturing style).</summary>
        public Countdown OnUpdateWhen<T>(T target, Func<T, bool> predicate) where T : class { TimerWorld.SetUpdateWhen(_handle, target, predicate); return this; }

        /// <summary>Sets looping by count (-1 infinite, 0 none, &gt;0 fixed).</summary>
        public Countdown SetLoop(int loopCount = -1) { TimerWorld.SetCountdownLoopCount(_handle, loopCount); return this; }

        /// <summary>Loops until <paramref name="stopWhen"/> becomes true.</summary>
        public Countdown SetLoop(Func<bool> stopWhen) { TimerWorld.SetCountdownLoopCondition(_handle, stopWhen); return this; }

        /// <summary>Loops until <paramref name="stopWhen"/> becomes true (non-capturing style).</summary>
        public Countdown SetLoop<T>(T target, Func<T, bool> stopWhen) where T : class { TimerWorld.SetCountdownLoopCondition(_handle, target, stopWhen); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the countdown starts.</summary>
        public Countdown OnStart(Action callback) { TimerWorld.SetOnStart(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the countdown starts (non-capturing style).</summary>
        public Countdown OnStart<T>(T target, Action<T> callback) where T : class { TimerWorld.SetOnStart(_handle, target, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> every tick with remaining seconds.</summary>
        public Countdown OnUpdate(Action<float> callback) { TimerWorld.SetOnUpdate(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> every tick with remaining seconds (non-capturing style).</summary>
        public Countdown OnUpdate<T>(T target, Action<T, float> callback) where T : class { TimerWorld.SetOnUpdate(_handle, target, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when a loop iteration restarts.</summary>
        public Countdown OnLoop(Action callback) { TimerWorld.SetOnLoop(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when a loop iteration restarts (non-capturing style).</summary>
        public Countdown OnLoop<T>(T target, Action<T> callback) where T : class { TimerWorld.SetOnLoop(_handle, target, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the countdown stops naturally.</summary>
        public Countdown OnStop(Action callback) { TimerWorld.SetOnStop(_handle, callback); return this; }

        /// <summary>Invokes <paramref name="callback"/> when the countdown stops naturally (non-capturing style).</summary>
        public Countdown OnStop<T>(T target, Action<T> callback) where T : class { TimerWorld.SetOnStop(_handle, target, callback); return this; }

        /// <summary>Starts the countdown.</summary>
        public void Start() => TimerWorld.Start(_handle);

        /// <summary>Pauses the countdown.</summary>
        public void Pause() => TimerWorld.Pause(_handle);

        /// <summary>Resumes the countdown.</summary>
        public void Resume() => TimerWorld.Resume(_handle);

        /// <summary>Stops the countdown and invokes stop callbacks.</summary>
        public void Stop() => TimerWorld.Stop(_handle);

        /// <summary>Cancels the countdown without invoking stop callbacks.</summary>
        public void Cancel() => TimerWorld.Cancel(_handle);

        /// <summary>Adds time to the remaining seconds.</summary>
        public void AddTime(float seconds) => TimerWorld.AddCountdownTime(_handle, seconds);

        /// <summary>Reduces time from the remaining seconds.</summary>
        public void ReduceTime(float seconds) => TimerWorld.ReduceCountdownTime(_handle, seconds);

        /// <summary>Returns a <see cref="TimerToken"/> that can only cancel this countdown — prevents misuse of the full timer API.</summary>
        public TimerToken AsTimerToken() => new(_handle);

        /// <summary>Returns concise debugging info.</summary>
        public override string ToString()
        {
            if (!IsAlive) return "Countdown{dead}";
            return $"Countdown{{rem={RemainingTime:0.###}, total={TotalTime:0.###}, run={IsRunning}, loop={CurrentLoopIteration}}}";
        }
    }
}

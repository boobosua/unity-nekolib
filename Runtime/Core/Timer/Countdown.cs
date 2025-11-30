using System;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Core
{
    public enum LoopType
    {
        None,           // No looping
        Count,          // Loop a specific number of times
        Infinite,       // Loop infinitely
        Condition       // Loop until condition is met
    }

    /// <summary>
    /// A timer that counts down from a given initial time.
    /// </summary>
    public class Countdown : TimerBase
    {
        public event Action OnLoop;

        private float _totalTime;
        private LoopType _loopType = LoopType.None;
        private int _loopCount = 0;
        private int _currentLoopIteration = 0;
        private Func<bool> _loopStopCondition = null;

        /// <summary>
        /// The current progress of the countdown.
        /// </summary>
        public float Progress
        {
            get
            {
                return _elapsedTime.PercentageOf(_totalTime);
            }
        }

        /// <summary>
        /// The inverse progress of the countdown (count up).
        /// </summary>
        public float InverseProgress
        {
            get
            {
                return 1f - Progress;
            }
        }

        /// <summary>
        /// The current loop iteration (0-based). Only meaningful for count-based looping.
        /// </summary>
        public int CurrentLoopIteration => _currentLoopIteration;

        /// <summary>
        /// Whether the countdown is configured to loop.
        /// </summary>
        public bool IsLooping => _loopType != LoopType.None;

        /// <summary>
        /// The current loop type being used.
        /// </summary>
        public LoopType CurrentLoopType => _loopType;

        /// <summary>
        /// The current total time for the countdown (may be modified from original).
        /// </summary>
        public float TotalTime => _totalTime;

        public string InverseClockFormat
        {
            get
            {
                return (_totalTime - _elapsedTime).ToClock();
            }
        }

        public Countdown(MonoBehaviour ownerComponent, float totalTime = 1f) : base(ownerComponent)
        {
            _totalTime = totalTime.AtLeast(0f);
            _loopType = LoopType.None;
            _loopCount = 0;
            _currentLoopIteration = 0;
            _loopStopCondition = null;
        }

        /// <summary>
        /// Internal method to reinitialize a pooled countdown timer.
        /// </summary>
        internal void ReInitialize(MonoBehaviour ownerComponent, float totalTime)
        {
            ReInitializeBase(ownerComponent);
            _totalTime = totalTime.AtLeast(0f);
            _loopType = LoopType.None;
            _loopCount = 0;
            _currentLoopIteration = 0;
            _loopStopCondition = null;
            OnLoop = null;
        }

        public override void Start()
        {
            _elapsedTime = _totalTime;
            base.Start();
        }

        /// <summary>
        /// Sets the countdown to loop a specific number of times.
        /// </summary>
        public void SetLoop(int loopCount = -1)
        {
            if (loopCount < -1)
                throw new ArgumentException("Loop count cannot be less than -1", nameof(loopCount));

            _currentLoopIteration = 0;
            _loopStopCondition = null; // Clear condition-based looping

            if (loopCount == 0)
            {
                _loopType = LoopType.None;
                _loopCount = 0;
            }
            else if (loopCount == -1)
            {
                _loopType = LoopType.Infinite;
                _loopCount = -1;
            }
            else
            {
                _loopType = LoopType.Count;
                _loopCount = loopCount;
            }
        }

        /// <summary>
        /// Sets the countdown to loop infinitely until a condition is met.
        /// </summary>
        public void SetLoop(Func<bool> stopWhen)
        {
            _loopStopCondition = stopWhen ?? throw new ArgumentNullException(nameof(stopWhen));
            _loopType = LoopType.Condition;
            _loopCount = 0; // Not used for condition-based looping
            _currentLoopIteration = 0;
        }

        /// <summary>
        /// Extends the countdown by adding time to the total duration.
        /// </summary>
        public void ExtendTime(float additionalTime)
        {
            if (additionalTime < 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Cannot extend time with {"negative value".Colorize(Swatch.VR)}: {additionalTime.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            _totalTime += additionalTime;

            if (!IsRunning) return;
            _elapsedTime = (_elapsedTime + additionalTime).AtMost(_totalTime);
        }

        /// <summary>
        /// Shortens the countdown by subtracting time from the total duration.
        /// </summary>
        public void ShortenTime(float timeToReduce)
        {
            if (timeToReduce < 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Cannot reduce time with {"negative value".Colorize(Swatch.VR)}: {timeToReduce.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            _totalTime = (_totalTime - timeToReduce).AtLeast(0f);

            if (!IsRunning) return;
            _elapsedTime = (_elapsedTime - timeToReduce).AtLeast(0f);
        }

        /// <summary>
        /// Sets the total time for the countdown.
        /// </summary>
        public void SetTotalTime(float newTotalTime)
        {
            _totalTime = newTotalTime.AtLeast(0f);
            _elapsedTime = _elapsedTime.AtMost(_totalTime);
        }

        public override void Tick(float deltaTime)
        {
            if (!ShouldTick)
                return;

            if (_elapsedTime <= 0)
            {
                // Check if we should continue looping based on the loop type
                bool shouldContinueLooping = false;

                switch (_loopType)
                {
                    case LoopType.None:
                        shouldContinueLooping = false;
                        break;

                    case LoopType.Infinite:
                        shouldContinueLooping = true;
                        break;

                    case LoopType.Count:
                        _currentLoopIteration++;
                        shouldContinueLooping = _currentLoopIteration < _loopCount;
                        break;

                    case LoopType.Condition:
                        // Loop until condition returns true
                        shouldContinueLooping = _loopStopCondition != null && !_loopStopCondition.Invoke();
                        break;
                }

                if (shouldContinueLooping)
                {
                    OnLoop?.Invoke(); // Signal completion of current loop
                    _elapsedTime = _totalTime; // Reset timer for next loop
                }
                else
                {
                    _elapsedTime = 0f;
                    base.Stop(); // Stop completely - this will invoke OnStop
                }
            }
            else
            {
                _elapsedTime -= deltaTime;
                InvokeUpdate(_elapsedTime);
            }
        }

        public override void Dispose()
        {
            OnLoop = null;
            _loopStopCondition = null;
            base.Dispose();
        }
    }
}
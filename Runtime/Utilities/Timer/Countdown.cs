using System;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Utilities
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

        private readonly float _originalTotalTime; // Total time for countdown
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
        /// The original total time when the countdown was first created.
        /// </summary>
        public float OriginalTotalTime => _originalTotalTime;

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
            _totalTime = _originalTotalTime = totalTime.AtLeast(0f);
            _loopType = LoopType.None;
            _loopCount = 0;
            _currentLoopIteration = 0;
            _loopStopCondition = null;
        }

        public override void Start()
        {
            _elapsedTime = _totalTime;
            base.Start();
        }

        /// <summary>
        /// Sets the countdown to loop a specific number of times.
        /// </summary>
        /// <param name="loopCount">Number of loops. -1 for infinite, 0 to disable looping, >0 for specific count</param>
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
        /// <param name="stopWhen">Function that returns true when looping should stop</param>
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
        /// <param name="additionalTime">Time to add in seconds</param>
        public void ExtendTime(float additionalTime)
        {
            if (additionalTime < 0f)
            {
                Debug.LogWarning($"[{nameof(Countdown)}] Cannot extend time with {"negative value".Colorize(Swatch.VR)}: {additionalTime.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            _totalTime += additionalTime;
        }

        /// <summary>
        /// Reduces the countdown by subtracting time from the total duration.
        /// </summary>
        /// <param name="timeToReduce">Time to subtract in seconds</param>
        public void ReduceTime(float timeToReduce)
        {
            if (timeToReduce < 0f)
            {
                Debug.LogWarning($"[{nameof(Countdown)}] Cannot reduce time with {"negative value".Colorize(Swatch.VR)}: {timeToReduce.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            _totalTime = Mathf.Max(0f, _totalTime - timeToReduce);
            _elapsedTime = Mathf.Min(_elapsedTime, _totalTime);
        }

        /// <summary>
        /// Resets the total time back to the original value when the countdown was created.
        /// </summary>
        public void ResetToOriginalTime()
        {
            _totalTime = _originalTotalTime;
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning)
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
                InvokeUpdate();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            OnLoop = null;
            _loopStopCondition = null;
        }
    }
}
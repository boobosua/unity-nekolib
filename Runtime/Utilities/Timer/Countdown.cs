using NekoLib.Extensions;

namespace NekoLib.Timer
{
    /// <summary>
    /// A timer that counts down from a given initial time.
    /// </summary>
    public class Countdown : TimerBase
    {
        private float _totalTime;
        private bool _isLoop = false;

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

        public string InverseClockFormat
        {
            get
            {
                return (_totalTime - _elapsedTime).ToClock();
            }
        }

        public Countdown(float initTime = 1f) : base()
        {
            _totalTime = initTime.AtLeast(0f);
            _isLoop = false;
        }

        public override void Start()
        {
            _elapsedTime = _totalTime;
            base.Start();
        }

        /// <summary>
        /// Sets a new initial time for the countdown.
        /// </summary>
        public Countdown SetTime(float newTime)
        {
            _totalTime = newTime.AtLeast(0f);
            return this;
        }

        /// <summary>
        /// Sets whether the countdown should loop or not.
        /// </summary>
        public Countdown SetLoop(bool loop)
        {
            _isLoop = loop;
            return this;
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            if (_elapsedTime <= 0)
            {
                if (_isLoop)
                {
                    InvokeStop();
                    _elapsedTime = _totalTime;
                }
                else
                {
                    _elapsedTime = 0f;
                    base.Stop();
                }
            }
            else
            {
                _elapsedTime -= deltaTime;
            }
        }
    }
}
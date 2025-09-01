using System;
using UnityEngine;
using UnityEngine.Events;
using NekoLib.Extensions;
using NekoLib.Utilities;

namespace NekoLib.Components
{
    [AddComponentMenu("NekoLib/Ticker")]
    public class Ticker : MonoBehaviour
    {
        [Tooltip("Time to wait before timeout (seconds)")]
        [SerializeField, Min(0f)] private float _waitTime = 1f;
        [Tooltip("Start timer automatically on Awake")]
        [SerializeField] private bool _autoStart = false;
        [Tooltip("If true, timer stops after timeout; otherwise, repeats")]
        [SerializeField] private bool _oneShot = true;
        [Tooltip("If true, timer ignores Time.timeScale")]
        [SerializeField] private bool _ignoreTimeScale = false;

        private float _elapsedTime = 0f;
        public float ElapsedTime => _elapsedTime;
        public int ElapsedSeconds => (int)_elapsedTime;

        private bool _isStopped = true;
        public bool IsStopped => _isStopped;

        private bool _paused = false;
        public bool Paused => _paused;

        public float TimeLeft
        {
            get
            {
                return Mathf.Max(0f, _waitTime - _elapsedTime);
            }
        }

        public int SecondsLeft
        {
            get
            {
                return Math.Max(0, Mathf.FloorToInt(_waitTime - _elapsedTime));
            }
        }

        public float Progress
        {
            get
            {
                return Mathf.Clamp01(_elapsedTime / _waitTime);
            }
        }

        public string ClockFormat
        {
            get
            {
                return _elapsedTime.ToClock();
            }
        }

        public string ShortClockFormat
        {
            get
            {
                return _elapsedTime.ToShortClock();
            }
        }

        [Space(5), SerializeField] private UnityEvent _onBegin;
        [Space(5), SerializeField] private FloatEvent _onUpdate;
        [Space(5), SerializeField] private UnityEvent _onTimeOut;

        public UnityEvent OnBegin => _onBegin;
        public FloatEvent OnUpdate => _onUpdate;
        public UnityEvent OnTimeOut => _onTimeOut;

        private void Awake()
        {
            if (_autoStart)
                StartTimer();
            else
                Stop();
        }

        private void Update()
        {
            if (_isStopped || _paused)
                return;

            float deltaTime = _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            _elapsedTime += deltaTime;

            _onUpdate?.Invoke(_elapsedTime);

            if (_elapsedTime >= _waitTime)
            {
                _onTimeOut?.Invoke();

                if (_oneShot)
                {
                    _isStopped = true;
                }
                else
                {
                    Reset();
                }
            }
        }

        /// <summary>
        /// Starts the timer, or resets the timer if it was started already.
        /// </summary>
        public void StartTimer(float timeSec = -1f)
        {
            if (timeSec > 0f)
                _waitTime = timeSec;

            _isStopped = false;
            _paused = false;
            Reset();
            _onBegin?.Invoke();
        }

        /// <summary>
        /// Resumes the timer if it is paused.
        /// </summary>
        public void Resume()
        {
            if (_paused)
            {
                _paused = false;
            }
        }

        /// <summary>
        /// Pauses the timer if it is running.
        /// </summary>
        public void Pause()
        {
            if (!_paused)
            {
                _paused = true;
            }
        }

        /// <summary>
        /// Stops the timer and resets its state.
        /// </summary>
        public void Stop()
        {
            _isStopped = true;
            Reset();
        }

        /// <summary>
        /// Sets whether the timer is one-shot.
        /// </summary>
        public void SetOneShot(bool oneShot)
        {
            _oneShot = oneShot;
        }

        /// <summary>
        /// Sets the wait time for the timer.
        /// </summary>
        public void SetWaitTime(float waitTime)
        {
            _waitTime = Mathf.Max(waitTime, 0f);
        }

        /// <summary>
        /// Gets the current wait time setting.
        /// </summary>
        public float GetWaitTime()
        {
            return _waitTime;
        }

        /// <summary>
        /// Sets whether the timer ignores time scale.
        /// </summary>
        public void SetIgnoreTimeScale(bool ignore)
        {
            _ignoreTimeScale = ignore;
        }

        /// <summary>
        /// Gets whether the timer ignores time scale.
        /// </summary>
        public bool IsIgnoringTimeScale()
        {
            return _ignoreTimeScale;
        }

        /// <summary>
        /// Gets whether the timer is configured as one-shot.
        /// </summary>
        public bool IsOneShot()
        {
            return _oneShot;
        }

        /// <summary>
        /// Resets the timer's elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            _elapsedTime = 0f;
        }
    }
}


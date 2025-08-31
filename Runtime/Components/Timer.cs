using System;
using UnityEngine;
using UnityEngine.Events;
using NekoLib.Extensions;
#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace NekoLib.Components
{
    [AddComponentMenu("NekoLib/Timer")]
    public class Timer : MonoBehaviour
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

        [Space(5), SerializeField] private UnityEvent _onBegin;
        [Space(5), SerializeField] private UnityEvent _onUpdate;
        [Space(5), SerializeField] private UnityEvent _onTimeOut;

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

            _onUpdate?.Invoke();

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

        public void AddBeginListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(_onBegin, action);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                _onBegin.AddListener(action);
            }
#else
            _onBegin.AddListener(action);
#endif
        }

        public void RemoveBeginListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(_onBegin, action);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                _onBegin.RemoveListener(action);
            }
#else
            _onBegin.RemoveListener(action);
#endif
        }

        public void AddTimeOutListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(_onTimeOut, action);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                _onTimeOut.AddListener(action);
            }
#else
            _onTimeOut.AddListener(action);
#endif
        }

        public void RemoveTimeOutListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(_onTimeOut, action);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                _onTimeOut.RemoveListener(action);
            }
#else
            _onTimeOut.RemoveListener(action);
#endif
        }

        public void AddUpdateListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(_onUpdate, action);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                _onUpdate.AddListener(action);
            }
#else
            _onUpdate.AddListener(action);
#endif
        }

        public void RemoveUpdateListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(_onUpdate, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets
                }
            }
            else
            {
                _onUpdate.RemoveListener(action);
            }
#else
            _onUpdate.RemoveListener(action);
#endif
        }

        public void RemoveAllListeners()
        {
            _onBegin.RemoveAllListeners();
            _onUpdate.RemoveAllListeners();
            _onTimeOut.RemoveAllListeners();
#if UNITY_EDITOR
            for (int i = _onBegin.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(_onBegin, i);
            }

            for (int i = _onUpdate.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(_onUpdate, i);
            }

            for (int i = _onTimeOut.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(_onTimeOut, i);
            }
#endif
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


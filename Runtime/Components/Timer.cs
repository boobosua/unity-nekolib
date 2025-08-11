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
        /// <summary>
        /// Gets whether the timer is paused. A paused timer does not process.
        /// </summary>
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

        [Space(5)] public UnityEvent OnBegin;
        [Space(5)] public UnityEvent OnUpdate;
        [Space(5)] public UnityEvent OnTimeOut;

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

            OnUpdate?.Invoke();

            if (_elapsedTime >= _waitTime)
            {
                OnTimeOut?.Invoke();

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
            // Only add persistent listener if the target is a Unity Object
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(OnBegin, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets (lambdas, non-Unity objects)
                }
            }
            else
            {
                // For non-Unity objects, fall back to runtime listener
                OnBegin.AddListener(action);
            }
#else
            // In builds, always use runtime listeners
            OnBegin.AddListener(action);
#endif
        }

        public void RemoveBeginListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(OnBegin, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets
                }
            }
            else
            {
                OnBegin.RemoveListener(action);
            }
#else
            OnBegin.RemoveListener(action);
#endif
        }

        public void AddTimeOutListener(UnityAction action)
        {
#if UNITY_EDITOR
            // Only add persistent listener if the target is a Unity Object
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(OnTimeOut, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets (lambdas, non-Unity objects)
                }
            }
            else
            {
                // For non-Unity objects, fall back to runtime listener
                OnTimeOut.AddListener(action);
            }
#else
            // In builds, always use runtime listeners
            OnTimeOut.AddListener(action);
#endif
        }

        public void RemoveTimeOutListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(OnTimeOut, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets
                }
            }
            else
            {
                OnTimeOut.RemoveListener(action);
            }
#else
            OnTimeOut.RemoveListener(action);
#endif
        }

        public void AddUpdateListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.AddPersistentListener(OnUpdate, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets
                }
            }
            else
            {
                // For non-Unity objects, fall back to runtime listener
                OnUpdate.AddListener(action);
            }
#else
            // In builds, always use runtime listeners
            OnUpdate.AddListener(action);
#endif
        }

        public void RemoveUpdateListener(UnityAction action)
        {
#if UNITY_EDITOR
            if (action.Target is UnityEngine.Object || action.Method.IsStatic)
            {
                try
                {
                    UnityEventTools.RemovePersistentListener(OnUpdate, action);
                }
                catch (Exception)
                {
                    // Silently fail for non-serializable targets
                }
            }
            else
            {
                OnUpdate.RemoveListener(action);
            }
#else
            OnUpdate.RemoveListener(action);
#endif
        }

        public void RemoveAllListeners()
        {
            OnBegin.RemoveAllListeners();
            OnUpdate.RemoveAllListeners();
            OnTimeOut.RemoveAllListeners();
#if UNITY_EDITOR
            for (int i = OnBegin.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(OnBegin, i);
            }

            for (int i = OnUpdate.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(OnUpdate, i);
            }

            for (int i = OnTimeOut.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(OnTimeOut, i);
            }
#endif
        }

        /// <summary>
        /// Starts the timer, or resets the timer if it was started already.
        /// If timeSec is greater than 0, this value is used for the wait_time.
        /// </summary>
        /// <param name="timeSec">Optional time to override wait_time</param>
        public void StartTimer(float timeSec = -1f)
        {
            if (timeSec > 0f)
                _waitTime = timeSec;

            _isStopped = false;
            _paused = false;
            Reset();
            OnBegin?.Invoke();
        }

        public void Resume()
        {
            if (_paused)
            {
                _paused = false;
            }
        }

        public void Pause()
        {
            if (!_paused)
            {
                _paused = true;
            }
        }

        public void Stop()
        {
            _isStopped = true;
            Reset();
        }

        public void SetOneShot(bool oneShot)
        {
            _oneShot = oneShot;
        }

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


using System;
using UnityEngine;
using UnityEngine.Events;
using NekoLib.Extensions;

namespace NekoLib.Components
{
    public class Timer : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _waitTime = 1f;
        [SerializeField] private bool _autoStart = false;
        [SerializeField] private bool _oneShot = true;

        private float _elapsedTime = 0f;
        public float ElapsedTime => _elapsedTime;
        public int ElapsedSeconds => (int)_elapsedTime;

        private bool _isStopped = true;
        public bool IsStopped => _isStopped;

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
        [Space(5)] public UnityEvent OnTimeOut;

        private void Awake()
        {
            if (_autoStart)
                Begin();
            else
                Stop();
        }

        private void Update()
        {
            if (_isStopped)
                return;

            _elapsedTime += Time.deltaTime;

            if (_elapsedTime >= _waitTime)
            {
                if (_oneShot)
                {
                    _isStopped = true;
                }

                Reset();

                OnTimeOut?.Invoke();
            }
        }

        public void Begin()
        {
            _isStopped = false;
            Reset();

            OnBegin?.Invoke();
        }

        public void Resume()
        {
            _isStopped = false;
        }

        public void Pause()
        {
            _isStopped = true;
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

        private void Reset()
        {
            _elapsedTime = 0f;
        }
    }
}


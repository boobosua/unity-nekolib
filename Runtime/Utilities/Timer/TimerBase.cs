using System;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Timer
{
    public abstract class TimerBase
    {
        public event Action OnStart;
        public event Action OnStop;

        protected float _elapsedTime;
        private readonly GameObject _owner;
        private readonly MonoBehaviour _ownerComponent; // Track the specific component

        public float ElapsedTime => _elapsedTime;
        public GameObject Owner => _owner;
        public MonoBehaviour OwnerComponent => _ownerComponent;

        public bool IsRunning { get; protected set; } = false;

        private bool _useUnscaledTime = false;
        /// <summary>
        /// Whether this timer uses unscaled time (ignores Time.timeScale).
        /// When true, the timer will continue running even when the game is paused.
        /// </summary>
        public bool UseUnscaledTime => _useUnscaledTime;

        /// <summary>
        /// Returns true if the owner component and GameObject still exist (not destroyed).
        /// </summary>
        public bool IsOwnerValid
        {
            get
            {
                // Only check if the component and GameObject exist, not if they're active/enabled
                return _ownerComponent != null && _ownerComponent.gameObject != null;
            }
        }

        /// <summary>
        /// Returns true if the owner component is enabled and its GameObject is active in hierarchy.
        /// </summary>
        public bool IsOwnerActiveAndEnabled
        {
            get
            {
                return IsOwnerValid &&
                    _ownerComponent.enabled &&
                    _ownerComponent.gameObject.activeInHierarchy;
            }
        }

        /// <summary>
        /// Returns true if the timer is paused due to the owner being disabled or inactive.
        /// </summary>
        public bool IsPausedDueToOwner
        {
            get
            {
                return IsOwnerValid && !IsOwnerActiveAndEnabled;
            }
        }

        public string ClockFormat
        {
            get
            {
                return _elapsedTime.ToClock();
            }
        }

        protected TimerBase(MonoBehaviour ownerComponent)
        {
            if (ownerComponent == null)
            {
                throw new ArgumentNullException(nameof(ownerComponent), "Timer owner component cannot be null");
            }

            _ownerComponent = ownerComponent;
            _owner = ownerComponent.gameObject;
            _useUnscaledTime = false;
            IsRunning = false;

            TimerManager.Instance.RegisterTimer(this);
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// Useful for UI timers, pause menus, or any timer that should continue when the game is paused.
        /// </summary>
        public void SetUnscaledTime()
        {
            _useUnscaledTime = true;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale).
        /// This is the default behavior.
        /// </summary>
        public void SetScaledTime()
        {
            _useUnscaledTime = false;
        }

        public virtual void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                InvokeStop();
            }
        }

        /// <summary>
        /// Stops the timer and removes it from the TimerManager.
        /// </summary>
        public void StopAndDestroy()
        {
            Stop();
            TimerManager.Instance.UnregisterTimer(this);
        }

        protected void InvokeStart()
        {
            OnStart?.Invoke();
        }

        protected void InvokeStop()
        {
            OnStop?.Invoke();
        }

        public void Resume()
        {
            IsRunning = true;
        }

        public void Pause()
        {
            IsRunning = false;
        }

        public virtual void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;
                InvokeStart();
            }
        }

        public abstract void Tick(float deltaTime);
    }
}
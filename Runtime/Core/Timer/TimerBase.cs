using System;
using NekoLib.Extensions;
using UnityEngine;

namespace NekoLib.Core
{
    public abstract class TimerBase : IDisposable
    {
        public event Action OnStart;
        public event Action<float> OnUpdate;
        public event Action OnStop;

        protected Func<bool> _updateCondition = null;

        protected float _elapsedTime;
        private GameObject _owner;
        private MonoBehaviour _ownerComponent;
        private bool _disposed;
        public float RemainTime => _elapsedTime;
        public GameObject Owner => _owner;
        public MonoBehaviour OwnerComponent => _ownerComponent;

        public bool IsRunning { get; protected set; } = false;

        private bool _useUnscaledTime = false;
        /// <summary>
        /// Whether this timer uses unscaled time (ignores Time.timeScale).
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

        /// <summary>
        /// Determines whether the timer should tick (update).
        /// </summary>
        protected bool ShouldTick
        {
            get
            {
                return IsRunning && (_updateCondition == null || _updateCondition.Invoke());
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

        protected TimerBase(MonoBehaviour ownerComponent)
        {
            if (ownerComponent == null)
                throw new ArgumentNullException(nameof(ownerComponent), "Timer owner component cannot be null");

            _ownerComponent = ownerComponent;
            _owner = ownerComponent.gameObject;
            _useUnscaledTime = false;
            IsRunning = false;
            _updateCondition = null;
            _disposed = false;

            // Register immediately with the global driver
            TimerPlayerLoopDriver.Register(this);
        }

        /// <summary>
        /// Internal method to reinitialize a pooled timer with a new owner.
        /// </summary>
        internal void ReInitializeBase(MonoBehaviour ownerComponent)
        {
            if (ownerComponent == null)
                throw new ArgumentNullException(nameof(ownerComponent), "Timer owner component cannot be null");

            _ownerComponent = ownerComponent;
            _owner = ownerComponent.gameObject;

            _elapsedTime = 0f;
            _useUnscaledTime = false;
            IsRunning = false;
            _updateCondition = null;

            OnStart = null;
            OnUpdate = null;
            OnStop = null;
            _disposed = false;

            // Re-register after reinitialization
            TimerPlayerLoopDriver.Register(this);
        }

        /// <summary>
        /// Sets the timer to use unscaled time (ignores Time.timeScale).
        /// </summary>
        public void SetUnscaledTime()
        {
            _useUnscaledTime = true;
        }

        /// <summary>
        /// Sets the timer to use scaled time (affected by Time.timeScale).
        /// </summary>
        public void SetScaledTime()
        {
            _useUnscaledTime = false;
        }

        /// <summary>
        /// Sets the condition for updating the timer.
        /// </summary>
        public void SetUpdateWhen(Func<bool> condition)
        {
            _updateCondition = condition;
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public virtual void Stop(bool invokeStopEvent = true)
        {
            if (IsRunning)
            {
                IsRunning = false;
                if (invokeStopEvent)
                {
                    InvokeStop();
                }
            }
        }

        /// <summary>
        /// Stops the timer and unregisters it from the global timer driver.
        /// </summary>
        public void StopAndDestroy(bool invokeStopEvent = true)
        {
            Stop(invokeStopEvent);
            Dispose();
        }

        protected void InvokeStart()
        {
            OnStart?.Invoke();
        }

        protected void InvokeStop()
        {
            OnStop?.Invoke();
        }

        protected void InvokeUpdate(float elapsedTime)
        {
            OnUpdate?.Invoke(elapsedTime);
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

        public virtual void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            IsRunning = false;

            OnStart = null;
            OnStop = null;
            OnUpdate = null;

            _updateCondition = null;

            // Unregister from driver; driver will remove on next cycle or immediately if not updating
            TimerPlayerLoopDriver.Unregister(this);
        }
    }
}
using System;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Core
{
    internal abstract class TimerHandlerBase
    {
        public event Action OnStart;
        public event Action<float> OnUpdate;
        public event Action OnStop;

        protected Func<bool> _updateWhen;

        protected MonoBehaviour _ownerComponent;
        internal MonoBehaviour OwnerComponent => _ownerComponent;

        protected float _elapsedTime;
        protected bool _useUnscaledTime;

        private bool _stopInvoked;

        internal int Slot { get; private set; } = -1;
        internal int Id { get; private set; } = -1;

        internal int ActiveIndex { get; set; } = -1;
        internal bool IsPendingRemoval { get; set; }

        internal bool IsRunning { get; private set; }

        internal bool IsOwnerValid => _ownerComponent != null;

        internal bool ShouldTick
        {
            get
            {
                if (!IsRunning) return false;
                if (_updateWhen == null) return true;

                try
                {
                    return _updateWhen.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error($"[Timer] updateWhen threw; disabling predicate. {ex}");
                    _updateWhen = null;
                    return false;
                }
            }
        }

        internal bool UseUnscaledTime => _useUnscaledTime;

        internal void AssignHandle(int slot, int id)
        {
            Slot = slot;
            Id = id;
        }

        internal void ClearHandle()
        {
            Slot = -1;
            Id = -1;
            ActiveIndex = -1;
            IsPendingRemoval = false;
        }

        internal void ReInitializeBase(MonoBehaviour ownerComponent)
        {
            if (ownerComponent == null) throw new ArgumentNullException(nameof(ownerComponent));

            _ownerComponent = ownerComponent;

            _elapsedTime = 0f;
            _useUnscaledTime = false;
            _updateWhen = null;

            OnStart = null;
            OnUpdate = null;
            OnStop = null;

            _stopInvoked = false;

            IsRunning = false;
            IsPendingRemoval = false;
        }

        internal void SetUnscaledTime(bool unscaled) => _useUnscaledTime = unscaled;
        internal void SetUpdateWhen(Func<bool> updateWhen) => _updateWhen = updateWhen;

        internal void AddOnStart(Action cb) => OnStart += cb;
        internal void AddOnUpdate(Action<float> cb) => OnUpdate += cb;
        internal void AddOnStop(Action cb) => OnStop += cb;

        internal void Start()
        {
            if (IsPendingRemoval) return;
            if (IsRunning) return;

            _stopInvoked = false;
            IsRunning = true;

            OnInitialize();

            if (!IsRunning) return;

            OnStart?.Invoke();
        }

        internal void Pause() => IsRunning = false;

        internal void Resume()
        {
            if (IsPendingRemoval) return;
            IsRunning = true;
        }

        internal void StopInvoke()
        {
            if (_stopInvoked) return;

            _stopInvoked = true;
            IsRunning = false;
            OnStop?.Invoke();
        }

        internal void StopSilent()
        {
            if (_stopInvoked) return;

            _stopInvoked = true;
            IsRunning = false;
        }

        protected void InvokeUpdate(float value) => OnUpdate?.Invoke(value);

        protected virtual void OnInitialize() { }

        protected void ClearForPool()
        {
            IsRunning = false;
            _elapsedTime = 0f;

            _useUnscaledTime = false;
            _updateWhen = null;

            OnStart = null;
            OnUpdate = null;
            OnStop = null;

            _ownerComponent = null;

            _stopInvoked = false;

            IsPendingRemoval = false;
            ActiveIndex = -1;
        }

        internal abstract bool Tick(float deltaTime);
        internal abstract void ResetForPool();
    }
}

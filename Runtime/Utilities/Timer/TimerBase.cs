using System;
using NekoLib.Extensions;

namespace NekoLib.Timer
{
    public abstract class TimerBase
    {
        public event Action OnStart;
        public event Action OnStop;

        protected float _elapsedTime;

        public float ElapsedTime => _elapsedTime;

        public bool IsRunning { get; protected set; } = false;

        public string ClockFormat
        {
            get
            {
                return _elapsedTime.ToClock();
            }
        }

        protected TimerBase()
        {
            IsRunning = false;
        }

        public virtual void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                InvokeStop();
            }
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
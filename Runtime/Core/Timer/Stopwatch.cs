using System;
using NekoLib.Extensions;
using UnityEngine;

namespace NekoLib.Core
{
    public class Stopwatch : TimerBase
    {
        private Func<bool> _stopCondition = null;

        /// <summary>
        /// A timer that counts up until manually stopped or based on a certain predicate.
        /// </summary>
        public Stopwatch(MonoBehaviour ownerComponent, Func<bool> stopCondition = null) : base(ownerComponent)
        {
            _stopCondition = stopCondition;
        }

        /// <summary>
        /// Internal method to reinitialize a pooled stopwatch timer.
        /// </summary>
        internal void ReInitialize(MonoBehaviour ownerComponent, Func<bool> stopCondition)
        {
            ReInitializeBase(ownerComponent);
            _stopCondition = stopCondition;
        }

        /// <summary>
        /// Starts the stopwatch.
        /// </summary>
        public override void Start()
        {
            _elapsedTime = 0f;
            base.Start();
        }

        public override void Tick(float deltaTime)
        {
            if (!ShouldTick)
                return;

            if (_stopCondition != null && _stopCondition.Invoke())
            {
                base.Stop();
                return;
            }

            _elapsedTime += deltaTime;
            InvokeUpdate(_elapsedTime);
        }

        /// <summary>
        /// Stops the stopwatch and returns the elapsed time.
        /// </summary>
        public float StopAndGetTime(bool invokeStopEvent = true)
        {
            base.Stop(invokeStopEvent);
            return _elapsedTime.AtLeast(0f);
        }

        public override void Dispose()
        {
            _stopCondition = null;
            base.Dispose();
        }
    }
}
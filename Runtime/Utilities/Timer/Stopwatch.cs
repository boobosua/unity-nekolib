using System;
using UnityEngine;

namespace NekoLib.Timer
{
    public class Stopwatch : TimerBase
    {
        private readonly Func<bool> _stopCondition;

        /// <summary>
        /// A timer that counts up until manually stopped or based on a certain predicate.
        /// </summary>
        public Stopwatch(MonoBehaviour ownerComponent, Func<bool> stopCondition = null) : base(ownerComponent)
        {
            _stopCondition = stopCondition;
        }

        public override void Start()
        {
            _elapsedTime = 0f;
            base.Start();
        }

        public override void Tick(float deltaTime)
        {
            if (!IsRunning)
                return;

            if (_stopCondition != null && _stopCondition.Invoke())
            {
                base.Stop();
                return;
            }

            _elapsedTime += deltaTime;
        }

        public void StopAndGetTime(out float endTime)
        {
            base.Stop();
            endTime = _elapsedTime;
        }
    }
}
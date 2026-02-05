using System;
using UnityEngine;

namespace NekoLib.Core
{
    internal sealed class StopwatchHandler : TimerHandlerBase
    {
        private Func<bool> _stopCondition;

        internal float ElapsedTime => _elapsedTime;

        internal void ReInitialize(MonoBehaviour ownerComponent, Func<bool> stopCondition)
        {
            ReInitializeBase(ownerComponent);
            _stopCondition = stopCondition;
        }

        protected override void OnStartAfter()
        {
            _elapsedTime = 0f;
        }

        internal override bool Tick(float deltaTime)
        {
            _elapsedTime += deltaTime;
            InvokeUpdate(_elapsedTime);

            if (_stopCondition != null && _stopCondition.Invoke())
            {
                StopInvoke();
                return true;
            }

            return false;
        }

        internal override void ResetForPool()
        {
            _stopCondition = null;
            ClearForPool();
        }
    }
}

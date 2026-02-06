using System;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Core
{
    internal sealed class CountdownHandler : TimerHandlerBase
    {
        public event Action OnLoop;

        private float _totalTime;
        private LoopType _loopType;
        private int _loopCount;
        private int _currentLoopIteration;
        private Func<bool> _loopStopCondition;

        internal float RemainingTime => _elapsedTime;
        internal float TotalTime => _totalTime;
        internal int CurrentLoopIteration => _currentLoopIteration;

        internal void ReInitialize(MonoBehaviour ownerComponent, float totalTime)
        {
            ReInitializeBase(ownerComponent);

            _totalTime = totalTime.AtLeast(0f);
            _loopType = LoopType.None;
            _loopCount = 0;
            _currentLoopIteration = 0;
            _loopStopCondition = null;

            OnLoop = null;
        }

        internal void SetTotalTime(float totalTime)
        {
            _totalTime = totalTime.AtLeast(0f);
        }

        internal void SetLoop(int loopCount)
        {
            if (loopCount < -1)
                throw new ArgumentException("Loop count cannot be less than -1", nameof(loopCount));

            _currentLoopIteration = 0;
            _loopStopCondition = null;

            if (loopCount == 0)
            {
                _loopType = LoopType.None;
                _loopCount = 0;
            }
            else if (loopCount == -1)
            {
                _loopType = LoopType.Infinite;
                _loopCount = -1;
            }
            else
            {
                _loopType = LoopType.Count;
                _loopCount = loopCount;
            }
        }

        internal void SetLoop(Func<bool> stopWhen)
        {
            _loopStopCondition = stopWhen ?? throw new ArgumentNullException(nameof(stopWhen));
            _loopType = LoopType.Condition;
            _loopCount = 0;
            _currentLoopIteration = 0;
        }

        internal void AddOnLoop(Action cb) => OnLoop += cb;

        internal void AddTime(float seconds)
        {
            if (seconds < 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Cannot add time with {"negative value".Colorize(Swatch.VR)}: {seconds.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            if (!IsOwnerValid) return;

            _elapsedTime += seconds;
            InvokeUpdate(_elapsedTime);
        }

        internal void ReduceTime(float seconds)
        {
            if (seconds < 0f)
            {
                Log.Warn($"[{nameof(Countdown)}] Cannot reduce time with {"negative value".Colorize(Swatch.VR)}: {seconds.ToString().Colorize(Swatch.SG)}. Operation ignored.");
                return;
            }

            if (!IsOwnerValid) return;

            _elapsedTime = (_elapsedTime - seconds).AtLeast(0f);
            InvokeUpdate(_elapsedTime);

            if (_elapsedTime <= 0f)
            {
                StopInvoke();
            }
        }

        protected override void OnStartAfter()
        {
            _elapsedTime = _totalTime;

            if (_elapsedTime <= 0f)
            {
                InvokeUpdate(0f);
                StopInvoke();
            }
        }

        internal override bool Tick(float deltaTime)
        {
            _elapsedTime -= deltaTime;

            if (_elapsedTime > 0f)
            {
                InvokeUpdate(_elapsedTime);
                return false;
            }

            _elapsedTime = 0f;
            InvokeUpdate(_elapsedTime);

            bool shouldContinueLooping;

            switch (_loopType)
            {
                case LoopType.Infinite:
                    shouldContinueLooping = true;
                    break;

                case LoopType.Count:
                    _currentLoopIteration++;
                    shouldContinueLooping = _currentLoopIteration < _loopCount;
                    break;

                case LoopType.Condition:
                    shouldContinueLooping = _loopStopCondition != null && !_loopStopCondition.Invoke();
                    break;

                default:
                    shouldContinueLooping = false;
                    break;
            }

            if (shouldContinueLooping)
            {
                OnLoop?.Invoke();
                _elapsedTime = _totalTime;
                return false;
            }

            StopInvoke();
            return true;
        }

        internal override void ResetForPool()
        {
            OnLoop = null;
            _loopStopCondition = null;

            _totalTime = 0f;
            _loopType = LoopType.None;
            _loopCount = 0;
            _currentLoopIteration = 0;
            _loopStopCondition = null;

            ClearForPool();
        }
    }
}

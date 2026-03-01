using System;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.Events;

namespace NekoLib.Components
{
    public enum SpriteAnimatorLoopMode { Once, Loop, PingPong }

    public abstract class SpriteAnimatorBase : MonoBehaviour
    {
        [Serializable]
        protected sealed class FrameEvent
        {
            [SerializeField] private int _frameIndex;
            [SerializeField, Space(6)] private UnityEvent _onFrame;

            public int FrameIndex => _frameIndex;
            public UnityEvent OnFrame => _onFrame;

            public FrameEvent(int frameIndex)
            {
                _frameIndex = frameIndex;
                _onFrame = new UnityEvent();
            }
        }

        private const int MaxCatchUpStepsPerUpdate = 8;

        [Tooltip("The sprites to animate through.")]
        [SerializeField] protected Sprite[] _sprites;

        [Tooltip("The frame rate of the animation. 0 pauses playback.")]
        [SerializeField, Min(0f)] protected float _frameRate = 12f;

        [Tooltip("The speed multiplier for the animation. 0 pauses playback.")]
        [SerializeField, Min(0f)] protected float _speedMultiplier = 1f;

        [Tooltip("The loop mode of the animation.")]
        [SerializeField] protected SpriteAnimatorLoopMode _loopMode = SpriteAnimatorLoopMode.Loop;

        [Tooltip("Should the animation play on awake?")]
        [SerializeField] protected bool _playOnAwake = true;

        [Tooltip("Should the animation use unscaled time?")]
        [SerializeField] protected bool _useUnscaledTime = false;

        [Tooltip("Should the animation start at a random frame?")]
        [SerializeField] protected bool _startAtRandomFrame = false;

        [SerializeField] protected FrameEvent[] _frameEvents;

        [SerializeField] protected UnityEvent _onCycleComplete;

        public UnityEvent OnCycleComplete => _onCycleComplete;

        protected int _currentFrame = 0;
        protected bool _isPlaying = false;
        protected bool _isReversed = false;

        protected float _frameTimer = 0f;
        protected int _spriteCount;
        protected float FrameDuration
        {
            get
            {
                float framesPerSecond = _frameRate * _speedMultiplier;
                return framesPerSecond > 0f ? 1f / framesPerSecond : float.PositiveInfinity;
            }
        }

        public bool IsPlaying => _isPlaying;
        public int CurrentFrame => _currentFrame;
        public int FrameCount => _spriteCount;

        protected virtual void Awake()
        {
            CacheSpritesInfo();
            SetInitialSprite();
        }

        protected virtual void Start()
        {
            if (_playOnAwake)
            {
                Play();
            }
        }

        protected virtual void Update()
        {
            if (!_isPlaying || _spriteCount == 0)
                return;

            if (!ShouldAnimate())
                return;

            // Treat 0 as paused (eg. designer sets Frame Rate to 0 in the inspector)
            if (_frameRate <= 0f || _speedMultiplier <= 0f)
                return;

            _frameTimer += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            float frameDuration = FrameDuration;

            if (_frameTimer < frameDuration)
            {
                return;
            }

            int catchUpSteps = Mathf.Min((int)(_frameTimer / frameDuration), MaxCatchUpStepsPerUpdate);
            _frameTimer -= catchUpSteps * frameDuration;

            for (int i = 0; i < catchUpSteps; i++)
            {
                if (!_isPlaying)
                {
                    break;
                }

                NextFrame();
            }
        }

        protected abstract bool ShouldAnimate();
        protected abstract void SetInitialSprite();
        protected abstract void UpdateSprite();

        protected void CacheSpritesInfo()
        {
            _spriteCount = (_sprites != null) ? _sprites.Length : 0;
        }

        protected void NextFrame()
        {
            if (!_isReversed)
            {
                _currentFrame++;
                if (_currentFrame >= _spriteCount)
                {
                    HandleEndReached();
                }
            }
            else
            {
                _currentFrame--;
                if (_currentFrame < 0)
                {
                    HandleEndReached();
                }
            }

            UpdateSprite();
            CheckFrameEvents();
        }

        protected void HandleEndReached()
        {
            switch (_loopMode)
            {
                case SpriteAnimatorLoopMode.Once:
                    _currentFrame = _isReversed ? 0 : _spriteCount - 1;
                    Stop();
                    _onCycleComplete?.Invoke();
                    break;

                case SpriteAnimatorLoopMode.Loop:
                    _currentFrame = _isReversed ? _spriteCount - 1 : 0;
                    _onCycleComplete?.Invoke();
                    break;

                case SpriteAnimatorLoopMode.PingPong:
                    _isReversed = !_isReversed;
                    _currentFrame = _isReversed ? _spriteCount - 2 : 1;
                    _currentFrame = Mathf.Clamp(_currentFrame, 0, _spriteCount - 1);
                    _onCycleComplete?.Invoke();
                    break;
            }
        }

        protected void CheckFrameEvents()
        {
            if (_frameEvents == null) return;

            for (int i = 0; i < _frameEvents.Length; i++)
            {
                var frameEvent = _frameEvents[i];

                if (frameEvent.FrameIndex < 0 || frameEvent.FrameIndex >= _spriteCount)
                    continue;

                if (frameEvent.FrameIndex == _currentFrame)
                {
                    frameEvent.OnFrame?.Invoke();
                }
            }
        }

        /// <summary>Set the loop mode for the animation.</summary>
        protected void SetLoopMode(SpriteAnimatorLoopMode loopMode)
        {
            _loopMode = loopMode;
        }

        /// <summary>Play the animation.</summary>
        public virtual void Play()
        {
            if (_spriteCount == 0)
            {
                Log.Warn("No sprites assigned to " + GetType().Name + " on " + gameObject.name);
                return;
            }

            if (_startAtRandomFrame && !_isPlaying)
            {
                _currentFrame = _sprites.RandIndex();
                UpdateSprite();
            }

            _isPlaying = true;
            _frameTimer = 0f;
        }

        /// <summary>Play the animation in reverse.</summary>
        public virtual void PlayReverse()
        {
            if (_spriteCount == 0) return;

            _isReversed = true;
            if (!_isPlaying)
            {
                _currentFrame = _spriteCount - 1;
                UpdateSprite();
            }
            Play();
        }

        /// <summary>Stop the animation.</summary>
        public virtual void Stop()
        {
            _isPlaying = false;
            _frameTimer = 0f;
        }

        /// <summary>Restart the animation.</summary>
        public virtual void Restart()
        {
            _currentFrame = 0;
            _frameTimer = 0f;
            _isReversed = false;
            UpdateSprite();
            Play();
        }

        /// <summary>Set the frame rate of the animation.</summary>
        public void SetFrameRate(float newFrameRate)
        {
            _frameRate = Mathf.Max(0f, newFrameRate);
        }

        /// <summary>Go to a specific frame in the animation.</summary>
        public void GoToFrame(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _spriteCount) return;

            _currentFrame = frameIndex;
            _frameTimer = 0f;
            UpdateSprite();
        }

        /// <summary>Play the animation once.</summary>
        public void PlayOneShot()
        {
            SetLoopMode(SpriteAnimatorLoopMode.Once);
            Restart();
        }
    }
}
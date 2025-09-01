using System;
using UnityEngine;
using UnityEngine.Events;
using NekoLib.Extensions;

namespace NekoLib.Components
{
    public abstract class BaseSpriteAnimator : MonoBehaviour
    {
        [Tooltip("The sprites to animate through.")]
        [SerializeField] protected Sprite[] _sprites;

        [Tooltip("The frame rate of the animation.")]
        [SerializeField] protected float _frameRate = 12f;

        [Tooltip("The loop mode of the animation.")]
        [SerializeField] protected LoopMode _loopMode = LoopMode.Loop;

        [Tooltip("Should the animation play on awake?")]
        [SerializeField] protected bool _playOnAwake = true;

        [Tooltip("The speed multiplier for the animation.")]
        [SerializeField, Min(0.1f)] protected float _speedMultiplier = 1f;

        [Tooltip("Should the animation use unscaled time?")]
        [SerializeField] protected bool _useUnscaledTime = false;

        [Tooltip("Should the animation start at a random frame?")]
        [SerializeField] protected bool _startAtRandomFrame = false;

        [SerializeField] protected FrameEvent[] _frameEvents;

        [SerializeField] protected UnityEvent _onAnimationComplete;
        [SerializeField] protected UnityEvent _onLoopComplete;

        public UnityEvent OnAnimationComplete => _onAnimationComplete;
        public UnityEvent OnLoopComplete => _onLoopComplete;

        [SerializeField] protected int _currentFrame = 0;
        [SerializeField] protected bool _isPlaying = false;
        [SerializeField] protected bool _isReversed = false;

        public enum LoopMode { Once, Loop, PingPong }

        [Serializable]
        public class FrameEvent
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

        protected float _frameTimer = 0f;
        protected int _spriteCount;
        protected float FrameDuration => 1f / (_frameRate * _speedMultiplier);

        public bool IsPlaying => _isPlaying;
        public int CurrentFrame => _currentFrame;
        public int FrameCount => _spriteCount;
        public Sprite[] Sprites => _sprites;

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

            _frameTimer += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_frameTimer >= FrameDuration)
            {
                _frameTimer -= FrameDuration;
                NextFrame();
            }
        }

        protected virtual bool ShouldAnimate()
        {
            return true;
        }

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
                case LoopMode.Once:
                    _currentFrame = _isReversed ? 0 : _spriteCount - 1;
                    Stop();
                    _onAnimationComplete?.Invoke();
                    break;

                case LoopMode.Loop:
                    _currentFrame = _isReversed ? _spriteCount - 1 : 0;
                    _onLoopComplete?.Invoke();
                    break;

                case LoopMode.PingPong:
                    _isReversed = !_isReversed;
                    _currentFrame = _isReversed ? _spriteCount - 2 : 1;
                    _currentFrame = Mathf.Clamp(_currentFrame, 0, _spriteCount - 1);
                    _onLoopComplete?.Invoke();
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

        /// <summary>
        /// Play the animation.
        /// </summary>
        public virtual void Play()
        {
            if (_spriteCount == 0)
            {
                Debug.LogWarning("No sprites assigned to " + GetType().Name + " on " + gameObject.name);
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

        /// <summary>
        /// Play the animation in reverse.
        /// </summary>
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

        /// <summary>
        /// Stop the animation.
        /// </summary>
        public virtual void Stop()
        {
            _isPlaying = false;
            _frameTimer = 0f;
        }

        /// <summary>
        /// Restart the animation.
        /// </summary>
        public virtual void Restart()
        {
            _currentFrame = 0;
            _frameTimer = 0f;
            _isReversed = false;
            UpdateSprite();
            Play();
        }

        /// <summary>
        /// Set the frame rate of the animation.
        /// </summary>
        public void SetFrameRate(float newFrameRate)
        {
            _frameRate = Mathf.Max(0.1f, newFrameRate);
        }

        /// <summary>
        /// Set the speed multiplier for the animation.
        /// </summary>
        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        /// <summary>
        /// Set whether to use unscaled time for the animation.
        /// </summary>
        public void SetUseUnscaledTime(bool useUnscaled)
        {
            _useUnscaledTime = useUnscaled;
        }

        /// <summary>
        /// Set whether to start the animation at a random frame.
        /// </summary>
        public void SetStartAtRandomFrame(bool randomStart)
        {
            _startAtRandomFrame = randomStart;
        }

        /// <summary>
        /// Set the loop mode for the animation.
        /// </summary>
        public void SetLoopMode(LoopMode loopMode)
        {
            _loopMode = loopMode;
        }

        /// <summary>
        /// Set the sprites for the animation.
        /// </summary>
        public void SetSprites(Sprite[] newSprites)
        {
            _sprites = newSprites;
            CacheSpritesInfo();
            _currentFrame = 0;
            _isReversed = false;
            UpdateSprite();
        }

        /// <summary>
        /// Go to a specific frame in the animation.
        /// </summary>
        public void GoToFrame(int frameIndex)
        {
            if (frameIndex < 0 || frameIndex >= _spriteCount) return;

            _currentFrame = frameIndex;
            _frameTimer = 0f;
            UpdateSprite();
        }

        /// <summary>
        /// Play the animation once.
        /// </summary>
        public void PlayOneShot()
        {
            SetLoopMode(LoopMode.Once);
            Restart();
        }
    }
}
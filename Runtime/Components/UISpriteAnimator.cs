using UnityEngine;
using UnityEngine.UI;
using NekoLib.Extensions;

namespace NekoLib.Components
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("NekoLib/UI Sprite Animator")]
    public class UISpriteAnimator : BaseSpriteAnimator
    {
        [Tooltip("Should the aspect ratio be preserved?")]
        [SerializeField] private bool _preserveAspect = true;
        [Tooltip("Should the animation pause when invisible?")]
        [SerializeField] private bool _pauseWhenInvisible = false;
        [Tooltip("Optional CanvasGroup to check for visibility.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        private Image _imageComponent;
        private bool _isVisible = true;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_sprites.IsNullOrEmpty())
            {
                if (_sprites[0] != null)
                {
                    if (_imageComponent == null)
                        _imageComponent = GetComponent<Image>();

                    _imageComponent.sprite = _sprites[0];
                }
            }
        }
#endif

        protected override void Awake()
        {
            _imageComponent = GetComponent<Image>();
            _imageComponent.preserveAspect = _preserveAspect;
            base.Awake();
        }

        protected override bool ShouldAnimate()
        {
            if (_pauseWhenInvisible)
            {
                _isVisible = IsUIVisible();
                return _isVisible;
            }
            return true;
        }

        protected override void SetInitialSprite()
        {
            if (_sprites != null && _spriteCount > 0 && _sprites[0] != null)
            {
                _imageComponent.sprite = _sprites[0];
            }
        }

        protected override void UpdateSprite()
        {
            if (_currentFrame >= 0 && _currentFrame < _spriteCount && _sprites[_currentFrame] != null)
            {
                _imageComponent.sprite = _sprites[_currentFrame];
            }
        }

        private bool IsUIVisible()
        {
            if (_canvasGroup != null && _canvasGroup.alpha <= 0f)
                return false;

            if (!_imageComponent.enabled || _imageComponent.color.a <= 0f)
                return false;

            return true;
        }

        /// <summary>
        /// Set whether to preserve the aspect ratio.
        /// </summary>
        public void SetPreserveAspect(bool preserve)
        {
            _preserveAspect = preserve;
            if (_imageComponent != null)
            {
                _imageComponent.preserveAspect = preserve;
            }
        }

        /// <summary>
        /// Set whether to pause the animation when invisible.
        /// </summary>
        public void SetPauseWhenInvisible(bool pause, CanvasGroup canvasGroup)
        {
            _pauseWhenInvisible = pause;
            _canvasGroup = canvasGroup;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace NekoLib.Components
{
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("NekoLib/UI Sprite Animator")]
    public class UISpriteAnimator : SpriteAnimatorBase
    {
        [Tooltip("Optional Canvas Groups used for visibility checks. If any assigned group has alpha <= 0, animation pauses.")]
        [SerializeField] private CanvasGroup[] _canvasGroups;

        private Image _imageComponent;

        protected override void Awake()
        {
            _imageComponent = GetComponent<Image>();
            base.Awake();
        }

        protected override bool ShouldAnimate()
        {
            if (!_imageComponent.enabled || _imageComponent.color.a <= 0f)
                return false;

            if (_canvasGroups == null || _canvasGroups.Length == 0)
                return true;

            for (int i = 0; i < _canvasGroups.Length; i++)
            {
                if (_canvasGroups[i] == null)
                    continue;

                if (_canvasGroups[i].alpha <= 0f)
                    return false;
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
    }
}
using UnityEngine;

namespace NekoLib.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("NekoLib/Sprite Animator")]
    public class SpriteAnimator : SpriteAnimatorBase
    {
        private SpriteRenderer _spriteRenderer;

        protected override void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            base.Awake();
        }

        protected override void SetInitialSprite()
        {
            if (_sprites != null && _spriteCount > 0 && _sprites[0] != null)
            {
                _spriteRenderer.sprite = _sprites[0];
            }
        }

        protected override bool ShouldAnimate()
        {
            return _spriteRenderer.enabled && _spriteRenderer.color.a > 0f;
        }

        protected override void UpdateSprite()
        {
            if (_currentFrame >= 0 && _currentFrame < _spriteCount && _sprites[_currentFrame] != null)
            {
                _spriteRenderer.sprite = _sprites[_currentFrame];
            }
        }
    }
}
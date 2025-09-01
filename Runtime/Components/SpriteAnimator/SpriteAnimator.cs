using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("NekoLib/Sprite Animator")]
    public class SpriteAnimator : SpriteAnimatorBase
    {
        private SpriteRenderer _spriteRenderer;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_sprites.IsNullOrEmpty())
            {
                if (_sprites[0] != null)
                {
                    if (_spriteRenderer == null)
                        _spriteRenderer = GetComponent<SpriteRenderer>();

                    _spriteRenderer.sprite = _sprites[0];
                }
            }
        }
#endif

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

        protected override void UpdateSprite()
        {
            if (_currentFrame >= 0 && _currentFrame < _spriteCount && _sprites[_currentFrame] != null)
            {
                _spriteRenderer.sprite = _sprites[_currentFrame];
            }
        }
    }
}
using UnityEngine;

namespace NekoLib.Components
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Scrolling Sprite Renderer")]
    public class ScrollingSpriteRenderer : ScrollingBackgroundBase
    {
        private Material _material;

        private void Awake()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            _material = spriteRenderer.material = Instantiate(spriteRenderer.material);
        }

        protected override void ApplyOffset(Vector2 offset)
        {
            _material.mainTextureOffset = offset;
        }

        protected override void ResetOffset()
        {
            if (_material != null)
            {
                _material.mainTextureOffset = Vector2.zero;
            }
        }

        private void OnDestroy()
        {
            ResetOffset();
        }
    }
}
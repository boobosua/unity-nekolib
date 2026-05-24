using UnityEngine;
using UnityEngine.UI;

namespace TRnK.Components
{
    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    [AddComponentMenu("TRnK.Toolkit/Scrolling Image")]
    public class ScrollingImage : ScrollingBackgroundBase
    {
        private Material _material;

        private void Awake()
        {
            var image = GetComponent<Image>();
            _material = image.material = Instantiate(image.material);
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
            Destroy(_material);
            _material = null;
        }
    }
}
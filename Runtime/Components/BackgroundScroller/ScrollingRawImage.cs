using UnityEngine;
using UnityEngine.UI;

namespace NekoLib.Components
{
    [RequireComponent(typeof(RawImage))]
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Scrolling Raw Image")]
    public class ScrollingRawImage : ScrollingBackgroundBase
    {
        private RawImage _image;

        private void Awake()
        {
            _image = GetComponent<RawImage>();
        }

        protected override void ApplyOffset(Vector2 offset)
        {
            var rect = _image.uvRect;
            _image.uvRect = new Rect(offset, rect.size);
        }

        protected override void ResetOffset()
        {
            var rect = _image.uvRect;
            _image.uvRect = new Rect(Vector2.zero, rect.size);
        }
    }
}
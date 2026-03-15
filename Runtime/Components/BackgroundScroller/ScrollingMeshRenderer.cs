using UnityEngine;

namespace NekoLib.Components
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Scrolling Mesh Renderer")]
    public class ScrollingMeshRenderer : ScrollingBackgroundBase
    {
        private Material _material;
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _material = _meshRenderer.material = Instantiate(_meshRenderer.material);
        }

        protected override void ApplyOffset(Vector2 offset)
        {
            if (_material != null)
            {
                _material.mainTextureOffset = offset;
            }
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
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                if (meshRenderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
                {
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;
                    meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    meshRenderer.allowOcclusionWhenDynamic = false;
                }
            }
        }
#endif

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
        }
    }
}
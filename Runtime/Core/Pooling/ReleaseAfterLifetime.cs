using NekoLib.Logger;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PoolableObject))]
#if ODIN_INSPECTOR
    public sealed class ReleaseAfterLifetime : SerializedMonoBehaviour
#else
    public sealed class ReleaseAfterLifetime : MonoBehaviour
#endif
    {
        [SerializeField] private float _lifetime = 3f;

        public float Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        private PoolableObject _poolable;
        private float _releaseAt;

        private void Awake() => _poolable = GetComponent<PoolableObject>();
        private void OnEnable() => _releaseAt = _lifetime > 0f ? Time.time + _lifetime : Time.time;
        private void Update() { if (Time.time >= _releaseAt) _poolable.Release(); }
    }
}

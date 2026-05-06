using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling
{
    [RequireComponent(typeof(PoolableObject))]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public sealed class DespawnAfterLifetime : SerializedMonoBehaviour
#else
    public sealed class DespawnAfterLifetime : MonoBehaviour
#endif
    {
        [SerializeField] private float _lifetime = 3f;

        public float Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        private PoolableObject _poolable;
        private float _despawnAt;

        private void Awake() => _poolable = GetComponent<PoolableObject>();
        private void OnEnable() => _despawnAt = _lifetime > 0f ? Time.time + _lifetime : Time.time;
        private void Update() { if (Time.time >= _despawnAt) _poolable.Despawn(); }
    }
}

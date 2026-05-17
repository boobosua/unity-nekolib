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
        [SerializeField] private bool _useUnscaledTime = false;

        public float Lifetime
        {
            get => _lifetime;
            set => _lifetime = Mathf.Max(0f, value);
        }

        public bool UseUnscaledTime
        {
            get => _useUnscaledTime;
            set => _useUnscaledTime = value;
        }

        private PoolableObject _poolable;
        private float _releaseAt;

        private void Awake() => _poolable = GetComponent<PoolableObject>();
        private void OnEnable() => _releaseAt = Now() + (_lifetime > 0f ? _lifetime : 0f);
        private void Update() { if (Now() >= _releaseAt) _poolable.Release(); }

        private float Now() => _useUnscaledTime ? Time.unscaledTime : Time.time;
    }
}

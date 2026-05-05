using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling.Components
{
    [RequireComponent(typeof(PoolableObject))]
    [RequireComponent(typeof(ParticleSystem))]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public sealed class DespawnOnParticleStopped : SerializedMonoBehaviour
#else
    public sealed class DespawnOnParticleStopped : MonoBehaviour
#endif
    {
        private PoolableObject _poolable;
        private ParticleSystem _particle;

        private void Awake()
        {
            _poolable = GetComponent<PoolableObject>();
            _particle = GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = _particle.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;
        }

        private void OnParticleSystemStopped()
        {
            if (_poolable.IsSpawned) _poolable.Despawn();
        }
    }
}

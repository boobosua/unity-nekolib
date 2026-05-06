using UnityEngine;

namespace NekoLib.Pooling
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ParticleSystem))]
    public class PoolableParticle : PoolableObject
    {
        protected ParticleSystem _particle;

        private void Awake()
        {
            _particle = GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = _particle.main;
            main.stopAction = ParticleSystemStopAction.Callback;
            main.playOnAwake = false;
        }

        protected virtual void OnParticleSystemStopped()
        {
            if (IsActive) Release();
        }

        public virtual void Play()
        {
            if (!_particle.isPlaying)
                _particle.Play();
        }
    }
}

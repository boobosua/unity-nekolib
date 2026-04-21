using System;
using NekoLib.Timer;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling
{
#if ODIN_INSPECTOR
    public abstract class PoolableObject : SerializedMonoBehaviour, IPoolable
#else
    public abstract class PoolableObject : MonoBehaviour, IPoolable
#endif
    {
        private static readonly Action<PoolableObject> s_delayedRelease =
            target => target._pool?.Despawn(target);

        private IPoolReleaser _pool;
        private TimerToken _releaseToken;

        public void ReleaseSelf()
        {
            _releaseToken.Cancel();

            if (_pool == null)
            {
                Destroy(gameObject);
                return;
            }

            _pool.Despawn(this);
        }

        public void ReleaseSelf(float delay)
        {
            _releaseToken.Cancel();

            if (_pool == null)
            {
                Destroy(gameObject, delay);
                return;
            }

            if (delay <= 0f)
            {
                _pool.Despawn(this);
                return;
            }

            _releaseToken = this.CallAfter(delay, this, s_delayedRelease);
        }

        internal void SetPool(IPoolReleaser pool) => _pool = pool;

        internal void CancelPendingRelease() => _releaseToken.Cancel();

        public abstract void OnSpawned();
        public abstract void OnDespawned();
    }
}

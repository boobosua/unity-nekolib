using NekoLib.Extensions;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling
{
#if ODIN_INSPECTOR
    public abstract class PoolableBehaviour : SerializedMonoBehaviour, IPoolable
#else
    public abstract class PoolableBehaviour : MonoBehaviour, IPoolable
#endif
    {
        private IPoolReleaser _pool;

        public void ReleaseSelf()
        {
            if (_pool == null)
            {
                Destroy(gameObject);
                return;
            }

            _pool.Despawn(this);
        }

        public void ReleaseSelf(float delay)
        {
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

            this.InvokeAfterDelay(delay, () => _pool.Despawn(this), false);
        }

        internal void SetPool(IPoolReleaser pool) => _pool = pool;

        public abstract void OnSpawned();
        public abstract void OnDespawned();
    }
}

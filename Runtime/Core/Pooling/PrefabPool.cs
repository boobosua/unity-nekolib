using System;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEngine;
using UnityEngine.Pool;

namespace NekoLib.Pooling
{
    /// <summary>
    /// Generic prefab pool for IPoolable MonoBehaviour instances.
    /// Handles spawning, despawning, and pooling logic.
    /// </summary>
    public sealed class PrefabPool<T> : IPoolReleaser where T : MonoBehaviour, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _poolRoot;
        private readonly ObjectPool<T> _pool;

        public int CountInactive => _pool.CountInactive;
        public bool IsValid => _poolRoot != null;

        public PrefabPool(
            T prefab,
            Transform poolRoot,
            int defaultCapacity = 16,
            int maxSize = 256,
            bool collectionCheck = true)
        {
            if (poolRoot == null) throw new ArgumentNullException(nameof(poolRoot));
            if (maxSize <= 0) throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be > 0.");

            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _poolRoot = poolRoot;

            _pool = new ObjectPool<T>(
                createFunc: Create,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                collectionCheck: collectionCheck,
                defaultCapacity: Mathf.Max(0, defaultCapacity),
                maxSize: maxSize
            );
        }

        /// <summary>Spawns an instance from the pool.</summary>
        public T Spawn()
        {
            var instance = _pool.Get();
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>Spawns an instance from the pool and parents it under <paramref name="parent"/>.</summary>
        public T Spawn(Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var instance = _pool.Get();
            instance.transform.SetParent(parent, worldPositionStays: false);
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>Spawns an instance and sets its world position and rotation.</summary>
        public T Spawn(Vector3 position, Quaternion rotation)
        {
            var instance = _pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>Spawns an instance, parents it under <paramref name="parent"/>, then sets world position and rotation.</summary>
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            var instance = _pool.Get();

            var tr = instance.transform;
            tr.SetParent(parent, worldPositionStays: false);
            tr.SetPositionAndRotation(position, rotation);

            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>Despawns an instance back to the pool.</summary>
        public void Despawn(T instance)
        {
            if (instance == null) return;
            _pool.Release(instance);
        }

        /// <summary>Despawns an instance back to the pool after <paramref name="delay"/> seconds.</summary>
        public void Despawn(T instance, float delay)
        {
            if (instance == null) return;

            if (delay <= 0f)
            {
                _pool.Release(instance);
                return;
            }

            instance.InvokeAfterDelay(delay, () =>
            {
                if (instance != null)
                {
                    _pool.Release(instance);
                }
            });
        }

        /// <summary>Pre-creates and returns <paramref name="count"/> instances to populate the pool.</summary>
        public void Prewarm(int count)
        {
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var instance = _pool.Get();
                _pool.Release(instance);
            }
        }

        /// <summary>Destroys all currently pooled inactive instances.</summary>
        public void Clear() => _pool.Clear();

        bool IPoolReleaser.IsValid => IsValid;

        void IPoolReleaser.Despawn(Component instance)
        {
            if (instance == null) return;

            if (!IsValid)
            {
                Log.Warn($"[Pooling] Pool root missing; Destroying '{instance.name}'.", instance);
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (instance is not T)
            {
                Log.Warn($"[Pooling] Despawn wrong type for '{instance.name}'. Destroying.", instance);
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }
#endif
            _pool.Release((T)instance);
        }

        private T Create()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
            instance.gameObject.SetActive(false);

            if (instance is PoolableBehaviour poolableBehaviour)
            {
                poolableBehaviour.SetPool(this);
            }

            return instance;
        }

        private static void OnGet(T instance) => instance.OnSpawned();

        private void OnRelease(T instance)
        {
            instance.OnDespawned();
            instance.transform.SetParent(_poolRoot, worldPositionStays: false);
            instance.gameObject.SetActive(false);
        }

        private static void OnDestroy(T instance)
        {
            if (instance == null) return;
            UnityEngine.Object.Destroy(instance.gameObject);
        }
    }
}

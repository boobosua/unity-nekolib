using System;
using UnityEngine;
using UnityEngine.Pool;

namespace NekoLib.Pooling
{
    /// <summary>
    /// High-performance prefab pool with deterministic lifecycle hooks.
    /// </summary>
    public sealed class PrefabPool<T> where T : Component, IPoolable
    {
        private readonly T _prefab;
        private readonly Transform _poolRoot;
        private readonly ObjectPool<T> _pool;

        public int CountInactive => _pool.CountInactive;

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

        /// <summary>
        /// Gets an instance from the pool.
        /// </summary>
        public T Get()
        {
            var instance = _pool.Get();
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Gets an instance from the pool and sets its position and rotation.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var instance = _pool.Get();

            var tr = instance.transform;
            if (parent != null) tr.SetParent(parent, worldPositionStays: false);
            tr.SetPositionAndRotation(position, rotation);

            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Releases an instance back to the pool.
        /// </summary>
        public void Release(T instance)
        {
            if (instance == null) return;
            _pool.Release(instance);
        }

        /// <summary>
        /// Prewarms the pool by creating a specified number of instances.
        /// </summary>
        public void Prewarm(int count)
        {
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var instance = _pool.Get();
                _pool.Release(instance);
            }
        }

        public void Clear() => _pool.Clear();

        private T Create()
        {
            var instance = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private static void OnGet(T instance)
        {
            instance.OnSpawned();
        }

        private void OnRelease(T instance)
        {
            instance.OnDespawned();

            var tr = instance.transform;
            tr.SetParent(_poolRoot, worldPositionStays: false);
            instance.gameObject.SetActive(false);
        }

        private static void OnDestroy(T instance)
        {
            if (instance == null) return;
            UnityEngine.Object.Destroy(instance.gameObject);
        }
    }
}

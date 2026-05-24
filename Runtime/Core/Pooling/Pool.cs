using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRnK.Pooling
{
    /// <summary> A generic object pool for MonoBehaviour-derived types. Provides methods to get and release instances, with optional prewarming and max size limits. </summary>
    /// <typeparam name="T"> The type of objects to pool. Must derive from PoolableObject. </typeparam>
    public sealed class Pool<T> where T : PoolableObject
    {
        private const int DefaultCapacity = 16;
        private const int DefaultMaxSize = 256;

        private readonly T Prefab;
        private readonly int MaxStackSize;
        private readonly Stack<T> InactiveStack;
        private readonly Transform Root;
        private readonly Action<PoolableObject> ReleaseCallback;

        private Transform _staging;

        public Pool(T prefab, int capacity = DefaultCapacity, int maxSize = DefaultMaxSize)
            : this(prefab, capacity, maxSize, null) { }

        public Pool(T prefab, int capacity, int maxSize, Transform root)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize));

            Prefab = prefab;
            MaxStackSize = maxSize;
            InactiveStack = new Stack<T>(capacity);

            Root = root != null ? root : CreateAutoRoot(prefab.name);
            ReleaseCallback = ReleaseFromCallback;
        }

        /// <summary> Gets an instance from the pool. If the pool is empty, a new instance will be created. </summary>
        public T Get() =>
            GetInternal(Vector3.zero, Quaternion.identity, Root);

        /// <summary> Gets an instance from the pool and sets its parent. If the pool is empty, a new instance will be created. </summary>
        public T Get(Transform parent) =>
            GetInternal(Vector3.zero, Quaternion.identity, parent != null ? parent : Root);

        /// <summary> Gets an instance from the pool and sets its position and rotation. If the pool is empty, a new instance will be created. </summary>
        public T Get(Vector3 position, Quaternion rotation) =>
            GetInternal(position, rotation, Root);

        /// <summary> Gets an instance from the pool and sets its position, rotation, and parent. If the pool is empty, a new instance will be created. </summary>
        public T Get(Vector3 position, Quaternion rotation, Transform parent) =>
            GetInternal(position, rotation, parent != null ? parent : Root);

        /// <summary> Releases an instance back to the pool. The instance will be deactivated and parented to the pool's root. </summary>
        public void Release(T instance)
        {
            if (instance == null) return;
            instance.Release();
        }

        /// <summary> Prewarms the pool by creating and releasing a number of instances. </summary>
        public void Prewarm(int count)
        {
            if (count <= 0) return;

            int room = MaxStackSize - InactiveStack.Count;
            if (room <= 0) return;

            Transform staging = EnsureStaging();

            int toCreate = Math.Min(count, room);
            for (int i = 0; i < toCreate; i++)
            {
                T instance = UnityEngine.Object.Instantiate(Prefab, staging);
                instance.Bind(ReleaseCallback);
                InactiveStack.Push(instance);
            }
        }

        /// <summary> Clears the pool by destroying all inactive instances and the staging object. </summary>
        public void Clear()
        {
            while (InactiveStack.Count > 0)
            {
                T instance = InactiveStack.Pop();
                if (instance != null)
                    UnityEngine.Object.Destroy(instance.gameObject);
            }
            InactiveStack.TrimExcess();

            if (_staging != null)
            {
                UnityEngine.Object.Destroy(_staging.gameObject);
                _staging = null;
            }
        }

        private T GetInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = PopValidOrCreate();

            Transform instanceTransform = instance.transform;
            instanceTransform.SetPositionAndRotation(position, rotation);
            if (instanceTransform.parent != parent) instanceTransform.SetParent(parent, true);

            instance.MarkActive();
            instance.gameObject.SetActive(true);
            return instance;
        }

        private T PopValidOrCreate()
        {
            if (InactiveStack.TryPop(out T candidate) && candidate != null)
                return candidate;
            return CreateInstance();
        }

        private T CreateInstance()
        {
            Transform staging = EnsureStaging();
            T instance = UnityEngine.Object.Instantiate(Prefab, staging);
            instance.Bind(ReleaseCallback);
            return instance;
        }

        private void ReleaseInternal(T instance)
        {
            instance.MarkInactive();
            instance.gameObject.SetActive(false);

            if (InactiveStack.Count >= MaxStackSize)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

            Transform instanceTransform = instance.transform;
            if (instanceTransform.parent != Root) instanceTransform.SetParent(Root, false);
            InactiveStack.Push(instance);
        }

        private void ReleaseFromCallback(PoolableObject instance) => ReleaseInternal((T)instance);

        private Transform EnsureStaging()
        {
            if (_staging != null) return _staging;
            var stagingGO = new GameObject($"[Pool Staging] {Prefab.name}");
            stagingGO.SetActive(false);
            _staging = stagingGO.transform;
            return _staging;
        }

        private static Transform CreateAutoRoot(string prefabName)
        {
            var rootObject = new GameObject($"[Pool] {prefabName}");
            return rootObject.transform;
        }
    }
}

using System;
using System.Collections.Generic;
using NekoLib.Logger;
using UnityEngine;

namespace NekoLib.Pooling
{
    public sealed class Pool<T> : IDisposable where T : PoolableObject
    {
        private const int DefaultCapacity = 16;
        private const int DefaultMaxSize = 256;

        private readonly T Prefab;
        private readonly int MaxStackSize;
        private readonly Stack<T> InactiveStack;
        private readonly Transform Root;
        private readonly bool OwnsRoot;
        private readonly Action<PoolableObject> ReleaseCallback;

        private int _countActive;
        private bool _isDisposed;

        public int CountInactive => InactiveStack.Count;
        public int CountActive => _countActive;
        public int CountAll => _countActive + InactiveStack.Count;
        public int MaxSize => MaxStackSize;
        public bool IsDisposed => _isDisposed || (OwnsRoot && Root == null);

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

            if (root != null)
            {
                Root = root;
                OwnsRoot = false;
            }
            else
            {
                Root = CreateAutoRoot(prefab.name);
                OwnsRoot = true;
            }

            ReleaseCallback = ReleaseFromCallback;
        }

        public T Spawn() =>
            SpawnInternal(Prefab.transform.position, Prefab.transform.rotation, Root);

        public T Spawn(Transform parent) =>
            SpawnInternal(Prefab.transform.position, Prefab.transform.rotation, parent != null ? parent : Root);

        public T Spawn(Vector3 position, Quaternion rotation) =>
            SpawnInternal(position, rotation, Root);

        public T Spawn(Vector3 position, Quaternion rotation, Transform parent) =>
            SpawnInternal(position, rotation, parent != null ? parent : Root);

        public void Despawn(T instance)
        {
            if (instance == null) return;
            instance.Despawn();
        }

        public void Clear()
        {
            while (InactiveStack.Count > 0)
            {
                T instance = InactiveStack.Pop();
                if (instance != null)
                    UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Clear();

            if (OwnsRoot && Root != null)
                UnityEngine.Object.Destroy(Root.gameObject);
        }

        private T SpawnInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (_isDisposed)
            {
                Log.Error($"Cannot spawn from disposed pool '{Prefab.name}'.");
                return null;
            }

            T instance = PopValidOrCreate();

            Transform instanceTransform = instance.transform;
            if (instanceTransform.parent != parent) instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);

            instance.MarkSpawned();
            _countActive++;
            instance.gameObject.SetActive(true);
            return instance;
        }

        private T PopValidOrCreate()
        {
            while (InactiveStack.Count > 0)
            {
                T candidate = InactiveStack.Pop();
                if (candidate != null) return candidate;
            }
            return CreateInstance();
        }

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(Prefab, Root);
            instance.Bind(ReleaseCallback);
            return instance;
        }

        private void DespawnInternal(T instance)
        {
            _countActive--;

            instance.MarkDespawned();
            instance.gameObject.SetActive(false);

            if (_isDisposed || InactiveStack.Count >= MaxStackSize)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
                return;
            }

            Transform instanceTransform = instance.transform;
            if (instanceTransform.parent != Root) instanceTransform.SetParent(Root, false);
            InactiveStack.Push(instance);
        }

        private void ReleaseFromCallback(PoolableObject instance) => DespawnInternal((T)instance);

        private static Transform CreateAutoRoot(string prefabName)
        {
            var rootObject = new GameObject($"[Pool] {prefabName}")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            return rootObject.transform;
        }
    }
}

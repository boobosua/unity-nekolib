using System;
using System.Collections.Generic;
using UnityEngine;

namespace NekoLib.Pooling
{
    public sealed class Pool<T> where T : PoolableObject
    {
        private const int DefaultCapacity = 16;
        private const int DefaultMaxSize = 256;

        private readonly T Prefab;
        private readonly int MaxStackSize;
        private readonly Stack<T> InactiveStack;
        private readonly Transform Root;
        private readonly Action<PoolableObject> ReleaseCallback;

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

        public T Get() =>
            GetInternal(Vector3.zero, Quaternion.identity, Root);

        public T Get(Transform parent) =>
            GetInternal(Vector3.zero, Quaternion.identity, parent != null ? parent : Root);

        public T Get(Vector3 position, Quaternion rotation) =>
            GetInternal(position, rotation, Root);

        public T Get(Vector3 position, Quaternion rotation, Transform parent) =>
            GetInternal(position, rotation, parent != null ? parent : Root);

        public void Release(T instance)
        {
            if (instance == null) return;
            instance.Release();
        }

        public void Clear()
        {
            while (InactiveStack.Count > 0)
            {
                T instance = InactiveStack.Pop();
                if (instance != null)
                    UnityEngine.Object.Destroy(instance.gameObject);
            }
            InactiveStack.TrimExcess();
        }

        private T GetInternal(Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = PopValidOrCreate();

            Transform instanceTransform = instance.transform;
            if (instanceTransform.parent != parent) instanceTransform.SetParent(parent, false);
            instanceTransform.SetPositionAndRotation(position, rotation);

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
            T instance = UnityEngine.Object.Instantiate(Prefab, Root);
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

        private static Transform CreateAutoRoot(string prefabName)
        {
            var rootObject = new GameObject($"[Pool] {prefabName}");
            return rootObject.transform;
        }
    }
}

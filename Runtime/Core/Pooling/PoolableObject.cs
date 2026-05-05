using System;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace NekoLib.Pooling
{
#if ODIN_INSPECTOR
    public abstract class PoolableObject : SerializedMonoBehaviour
#else
    public abstract class PoolableObject : MonoBehaviour
#endif
    {
        private Action<PoolableObject> _releaseCallback;
        private bool _despawning;

        public bool IsSpawned { get; private set; }

        public void Despawn()
        {
            if (_despawning) return;
            _despawning = true;

            if (_releaseCallback != null)
                _releaseCallback(this);
            else
                Destroy(gameObject);
        }

        internal void Bind(Action<PoolableObject> callback) => _releaseCallback = callback;
        internal void MarkSpawned() { _despawning = false; IsSpawned = true; }
        internal void MarkDespawned() { IsSpawned = false; }
    }
}

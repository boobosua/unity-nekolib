using System;

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
        private bool _releasing;

        public bool IsActive { get; private set; }

        public void Release()
        {
            if (_releasing) return;
            _releasing = true;

            if (_releaseCallback != null)
                _releaseCallback(this);
            else
                Destroy(gameObject);
        }

        internal void Bind(Action<PoolableObject> callback) => _releaseCallback = callback;
        internal void MarkActive() { _releasing = false; IsActive = true; }
        internal void MarkInactive() { IsActive = false; }
    }
}

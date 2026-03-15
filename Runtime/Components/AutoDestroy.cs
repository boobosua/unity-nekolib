using NekoLib.Timer;
using UnityEngine;
using UnityEngine.Events;

namespace NekoLib.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Auto Destroy")]
    public sealed class AutoDestroy : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Time in seconds before the object is destroyed")]
        private float _destroyAfter = 5f;

        [SerializeField, Space(6), Tooltip("Event triggered just before destruction")]
        private UnityEvent _onBeforeDestroy;
        public UnityEvent OnBeforeDestroy => _onBeforeDestroy;

        private void Start()
        {
            this.CallAfter(_destroyAfter, OnDestroyAfterDelay);
        }

        private void OnDestroyAfterDelay()
        {
            _onBeforeDestroy?.Invoke();
            Destroy(gameObject);
        }
    }
}
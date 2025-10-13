using NekoLib.Logger;
using UnityEngine;
using UnityEngine.Events;

namespace NekoLib.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Auto Destroy")]
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Time in seconds before the object is destroyed")]
        private float _destroyAfter = 5f;

        [SerializeField, Space(6), Tooltip("Event triggered just before destruction")]
        private UnityEvent _onBeforeDestroy;
        public UnityEvent OnBeforeDestroy => _onBeforeDestroy;

        private void Start()
        {
            if (_destroyAfter <= 0f)
            {
                Log.Warn("[AutoDestroy] Destroy delay must be greater than zero. Object will not be destroyed.");
                enabled = false;
                return;
            }

            Destroy(gameObject, _destroyAfter);
        }

        private void OnDestroy()
        {
            _onBeforeDestroy?.Invoke();
        }
    }
}
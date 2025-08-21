using UnityEngine;

namespace NekoLib.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Auto Destroy")]
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Time in seconds before the object is destroyed")] private float _lifetime = 5f;

        private void Start()
        {
            if (_lifetime <= 0f)
            {
                Debug.LogWarning("[AutoDestroy] Lifetime must be greater than zero. Object will not be destroyed.");
                return;
            }

            Destroy(gameObject, _lifetime);
        }
    }
}

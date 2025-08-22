using UnityEngine;
using NekoLib.Extensions;
using NekoLib.Utilities;

namespace NekoLib
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Auto Orbit Around")]
    public class AutoOrbitAround : MonoBehaviour
    {
        private enum OrbitMode
        {
            AutoHorizontalOnly = 0,  // OrbitAround - only horizontal rotation
            AutoVerticalOnly = 1,    // OrbitAroundVertical - only vertical rotation
        }

        [SerializeField] private Transform _target;
        [SerializeField] private float _distance = 5f;
        [SerializeField] private OrbitMode _mode = OrbitMode.AutoHorizontalOnly;
        [SerializeField] private float _horizontalSpeed = 30f; // degrees per second
        [SerializeField] private float _staticVerticalAngle = 30f; // for horizontal-only mode
        [SerializeField] private float _verticalSpeed = 15f;   // degrees per second
        [SerializeField] private float _staticHorizontalAngle = 0f;

        private float _currentHorizontalAngle = 0f;
        private float _currentVerticalAngle = 30f;

        private void Update()
        {
            if (_target == null)
                return;

            switch (_mode)
            {
                case OrbitMode.AutoHorizontalOnly:
                    transform.OrbitAround(_target, Orientation.Horizontal, _horizontalSpeed, _staticVerticalAngle, _distance, ref _currentHorizontalAngle);
                    break;

                case OrbitMode.AutoVerticalOnly:
                    transform.OrbitAround(_target, Orientation.Vertical, _verticalSpeed, _staticHorizontalAngle, _distance, ref _currentVerticalAngle);
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_target == null)
                return;

            Gizmos.color = Swatch.ME;
            Gizmos.DrawLine(_target.position, transform.position);
            // Draw a circle representing the orbit path
            // Draw a circle representing the orbit path (horizontal plane)
            const int segments = 64;
            Vector3 center = _target.position;
            float radius = _distance;

            if (_mode == OrbitMode.AutoHorizontalOnly)
            {
                // Orbit in horizontal plane at static vertical angle
                float verticalAngle = _staticVerticalAngle;
                Quaternion tilt = Quaternion.Euler(verticalAngle, 0f, 0f);
                Vector3 orbitNormal = tilt * Vector3.up;
                Vector3 orbitForward = tilt * Vector3.forward;

                Vector3 prevPoint = center + orbitForward * radius;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = 360f / segments * i;
                    Quaternion segmentRotation = Quaternion.AngleAxis(angle, orbitNormal);
                    Vector3 nextPoint = center + segmentRotation * orbitForward * radius;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }
            }
            else // AutoVerticalOnly
            {
                // Orbit in vertical plane at static horizontal angle
                float horizontalAngle = _staticHorizontalAngle;
                Quaternion yaw = Quaternion.Euler(0f, horizontalAngle, 0f);
                Vector3 orbitRight = yaw * Vector3.right;
                Vector3 orbitUp = yaw * Vector3.up;

                Vector3 prevPoint = center + orbitUp * radius;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = 360f / segments * i;
                    Quaternion segmentRotation = Quaternion.AngleAxis(angle, orbitRight);
                    Vector3 nextPoint = center + segmentRotation * orbitUp * radius;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }
            }
        }
#endif
    }
}

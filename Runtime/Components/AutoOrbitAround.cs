using NekoLib.ColorPalette;
using NekoLib.Constant;
using NekoLib.Extensions;
using NekoLib.Utilities;
using UnityEngine;

namespace NekoLib
{
    [DisallowMultipleComponent]
    [AddComponentMenu("NekoLib/Auto Orbit Around")]
    public sealed class AutoOrbitAround : MonoBehaviour
    {
        private enum OrbitMode
        {
            AutoHorizontalOnly = 0,
            AutoVerticalOnly = 1,
        }

        private enum FacingMode
        {
            FaceTarget = 0,
            FaceAwayFromTarget = 1,
            FaceHeading = 2,
            FaceOppositeHeading = 3,
            None = 4,
        }

        [SerializeField] private Transform _target;
        [SerializeField] private float _distance = 5f;
        [SerializeField] private OrbitMode _mode = OrbitMode.AutoHorizontalOnly;
        [SerializeField, Tooltip("Degrees per second on the moving axis.")]
        private float _speed = 30f;
        [SerializeField, Tooltip("Initial angle offset (degrees). Use this to evenly space multiple orbiters.")]
        private float _startAngle = 0f;
        [SerializeField, Tooltip("Elevation angle above the target (degrees). 0 = same height, 90 = directly above.")]
        private float _elevationAngle = 30f;
        [SerializeField, Tooltip("Bearing of the vertical orbit plane around Y (degrees). 0 = orbit in the Z plane.")]
        private float _bearingAngle = 0f;
        [SerializeField, Tooltip("How the orbiting object orients itself.")]
        private FacingMode _facing = FacingMode.FaceTarget;

        private float _currentAngle = 0f;

        private void Awake()
        {
            _currentAngle = _startAngle;
        }

        private void Update()
        {
            if (_target == null)
                return;

            var orientation = _mode == OrbitMode.AutoHorizontalOnly ? Orientation.Horizontal : Orientation.Vertical;
            float staticAngle = _mode == OrbitMode.AutoHorizontalOnly ? _elevationAngle : _bearingAngle;
            transform.OrbitAround(_target.position, orientation, _speed, staticAngle, _distance, ref _currentAngle);
            ApplyFacing();
        }

        private void ApplyFacing()
        {
            switch (_facing)
            {
                case FacingMode.FaceTarget:
                    transform.LookAt(_target);
                    break;

                case FacingMode.FaceAwayFromTarget:
                    transform.forward = (transform.position - _target.position).normalized;
                    break;

                case FacingMode.FaceHeading:
                case FacingMode.FaceOppositeHeading:
                    float staticAngle = _mode == OrbitMode.AutoHorizontalOnly ? _elevationAngle : _bearingAngle;
                    Vector3 orbitAxis = _mode == OrbitMode.AutoHorizontalOnly
                        ? Vector3.up
                        : Quaternion.AngleAxis(staticAngle, Vector3.up) * Vector3.right;
                    Vector3 heading = Vector3.Cross(orbitAxis, transform.position - _target.position) * Mathf.Sign(_speed);
                    if (heading.sqrMagnitude > 0.0001f)
                        transform.forward = _facing == FacingMode.FaceHeading ? heading.normalized : -heading.normalized;
                    break;

                case FacingMode.None:
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
            const int segments = 64;
            Vector3 center = _target.position;
            float radius = _distance;

            if (_mode == OrbitMode.AutoHorizontalOnly)
            {
                // Flat circle elevated by elevationAngle. Sweeps around Vector3.up.
                Vector3 baseOfs = Quaternion.AngleAxis(_elevationAngle, Vector3.right) * (Vector3.back * radius);
                Vector3 prevPoint = center + baseOfs;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = Constants.FullRotation / segments * i;
                    Vector3 nextPoint = center + Quaternion.AngleAxis(angle, Vector3.up) * baseOfs;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }
            }
            else // AutoVerticalOnly
            {
                // Vertical circle in the plane at bearingAngle around Y.
                Vector3 orbitAxis = Quaternion.AngleAxis(_bearingAngle, Vector3.up) * Vector3.right;
                Vector3 baseOfs = Quaternion.AngleAxis(_bearingAngle, Vector3.up) * (Vector3.back * radius);
                Vector3 prevPoint = center + baseOfs;
                for (int i = 1; i <= segments; i++)
                {
                    float angle = Constants.FullRotation / segments * i;
                    Vector3 nextPoint = center + Quaternion.AngleAxis(angle, orbitAxis) * baseOfs;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }
            }
        }
#endif
    }
}

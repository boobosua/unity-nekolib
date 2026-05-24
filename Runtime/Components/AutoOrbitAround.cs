using TRnK.ColorPalette;
using TRnK.Constant;
using TRnK.Extensions;
using UnityEngine;

namespace TRnK.Components
{
    [DisallowMultipleComponent]
    [AddComponentMenu("TRnK.Toolkit/Auto Orbit Around")]
    public sealed class AutoOrbitAround : MonoBehaviour
    {
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
        [SerializeField, Tooltip("Degrees per second. Negative values reverse direction.")]
        private float _speed = 30f;
        [SerializeField, Tooltip("Starting angle offset (degrees). Use this to evenly space multiple orbiters around the same target.")]
        private float _startAngle = 0f;

        [SerializeField, Tooltip("Tilts the orbit plane up or down (degrees). 0 = flat horizontal ring, 90 = vertical loop.")]
        private float _elevationAngle = 0f;
        [SerializeField, Tooltip("Rotates the tilt direction around the world Y axis (degrees). Only visible when Elevation Angle is between 0 and 90.")]
        private float _bearingAngle = 0f;

        [SerializeField, Tooltip("How the orbiting object orients itself.")]
        private FacingMode _facing = FacingMode.FaceTarget;

        private float _currentAngle = 0f;

        private Vector3 OrbitAxis => Vector3.up.RotateX(_elevationAngle).RotateY(_bearingAngle);

        private static Vector3 GetPerp(Vector3 axis) =>
            Mathf.Abs(Vector3.Dot(axis, Vector3.forward)) < 0.99f
                ? Vector3.Cross(axis, Vector3.forward).normalized
                : Vector3.Cross(axis, Vector3.right).normalized;

        private void Awake()
        {
            _currentAngle = _startAngle;
        }

        private void Update()
        {
            if (_target == null)
                return;

            _currentAngle += _speed * Time.deltaTime;
            if (_currentAngle >= Constants.FullRotation) _currentAngle -= Constants.FullRotation;
            if (_currentAngle < 0f) _currentAngle += Constants.FullRotation;

            Vector3 orbitAxis = OrbitAxis;
            Vector3 perp = GetPerp(orbitAxis);
            transform.position = _target.position + Quaternion.AngleAxis(_currentAngle, orbitAxis) * perp * _distance;
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
                    Vector3 orbitAxis = OrbitAxis;
                    Vector3 radial = transform.position - _target.position;
                    Vector3 heading = Vector3.Cross(orbitAxis, radial) * Mathf.Sign(_speed);
                    if (heading.sqrMagnitude > Constants.NearZeroSqrMagnitude)
                        transform.forward = _facing == FacingMode.FaceHeading ? heading.normalized : -heading.normalized;
                    break;

                case FacingMode.None:
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_target == null || Application.isPlaying)
                return;

            Vector3 orbitAxis = OrbitAxis;
            Vector3 perp = GetPerp(orbitAxis);
            transform.position = _target.position + Quaternion.AngleAxis(_startAngle, orbitAxis) * perp * _distance;
            UnityEditor.SceneView.RepaintAll();
        }

        private void OnDrawGizmosSelected()
        {
            if (_target == null)
                return;

            Gizmos.color = Swatch.ME;
            Gizmos.DrawWireSphere(_target.position, 0.08f);
            Gizmos.DrawLine(_target.position, transform.position);

            const int segments = 64;
            Vector3 center = _target.position;

            Vector3 orbitAxis = OrbitAxis;
            Vector3 perp = GetPerp(orbitAxis);

            Vector3 prevPoint = center + perp * _distance;
            for (int i = 1; i <= segments; i++)
            {
                float angle = Constants.FullRotation / segments * i;
                Vector3 nextPoint = center + Quaternion.AngleAxis(angle, orbitAxis) * perp * _distance;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            // Tick mark at start angle.
            Gizmos.color = Color.yellow;
            Vector3 startOfs = Quaternion.AngleAxis(_startAngle, orbitAxis) * perp;
            Gizmos.DrawLine(center + startOfs * (_distance * 0.85f), center + startOfs * (_distance * 1.15f));
        }
#endif
    }
}

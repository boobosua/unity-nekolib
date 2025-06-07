using UnityEngine;

namespace NekoLib.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return (Vector2)vector;
        }

        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
        }

        public static Vector3 Subtract(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x - (x ?? 0), vector.y - (y ?? 0), vector.z - (z ?? 0));
        }

        public static Vector3 Multiply(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x * (x ?? 1), vector.y * (y ?? 1), vector.z * (z ?? 1));
        }

        public static Vector3 Divide(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                x.HasValue && x.Value != 0 ? vector.x / x.Value : vector.x,
                y.HasValue && y.Value != 0 ? vector.y / y.Value : vector.y,
                z.HasValue && z.Value != 0 ? vector.z / z.Value : vector.z
            );
        }

        /// <summary>
        /// Converts a world position to a screen position.
        /// </summary>
        public static Vector3 WorldToScreen(this Vector3 vector, bool useCameraDistance = false)
        {
            var pos = Camera.main.WorldToScreenPoint(vector);
            pos.z = useCameraDistance ? pos.z : 0f;
            return pos;
        }

        /// <summary>
        /// Converts a viewport position to a screen position.
        /// </summary>
        public static Vector3 ViewportToScreen(this Vector3 vector)
        {
            var pos = Camera.main.ViewportToScreenPoint(vector);
            pos.z = 0f;
            return pos;
        }

        /// <summary>
        /// Converts a screen position to a world position.
        /// </summary>
        public static Vector3 ScreenToWorld(this Vector3 vector, bool useCameraDistance = false)
        {
            var nearClipPlanePos = vector.With(z: Camera.main.nearClipPlane + 1);
            var worldPos = Camera.main.ScreenToWorldPoint(nearClipPlanePos);
            worldPos.z = useCameraDistance ? worldPos.z : 0f;
            return worldPos;
        }

        /// <summary>
        /// Converts a viewport position to a world position.
        /// </summary>
        public static Vector3 ViewportToWorld(this Vector3 vector, bool useCameraDistance = false)
        {
            var nearClipPlanePos = vector.With(z: Camera.main.nearClipPlane + 1);
            var worldPos = Camera.main.ViewportToWorldPoint(nearClipPlanePos);
            worldPos.z = useCameraDistance ? worldPos.z : 0f;
            return worldPos;
        }

        /// <summary>
        /// Converts a screen position to a viewport position.
        /// </summary>
        public static Vector3 ScreenToViewport(this Vector3 vector)
        {
            return Camera.main.ScreenToViewportPoint(vector.With(z: 0f));
        }

        /// <summary>
        /// Converts a world position to a viewport position.
        /// </summary>
        public static Vector3 WorldToViewport(this Vector3 vector)
        {
            return Camera.main.WorldToViewportPoint(vector.With(z: 0f));
        }

        /// <summary>
        /// Returns a Boolean indicating whether the current Vector3 is in a given range from another Vector3
        /// </summary>
        public static bool InRangeOf(this Vector3 current, Vector3 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Computes a random point in an annulus (a ring-shaped area) based on minimum and 
        /// maximum radius values around a central Vector3 point (origin).
        /// </summary>
        public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Converting the 2D direction vector to a 3D position vector
            Vector3 position = new Vector3(direction.x, 0, direction.y) * distance;
            return origin + position;
        }
    }
}


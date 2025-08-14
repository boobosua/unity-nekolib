using UnityEngine;

namespace NekoLib.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }

        public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x + (x ?? 0), vector.y + (y ?? 0));
        }

        public static Vector2 Subtract(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x - (x ?? 0), vector.y - (y ?? 0));
        }

        public static Vector2 Multiply(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x * (x ?? 1), vector.y * (y ?? 1));
        }

        public static Vector2 Divide(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(
                x.HasValue && x.Value != 0 ? vector.x / x.Value : vector.x,
                y.HasValue && y.Value != 0 ? vector.y / y.Value : vector.y
            );
        }

        /// <summary>
        /// Returns a Boolean indicating whether the current Vector2 is in a given range from another Vector2
        /// </summary>
        public static bool InRangeOf(this Vector2 current, Vector2 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Returns a vector with the same direction but clamped to a maximum magnitude.
        /// </summary>
        public static Vector2 ClampMagnitude(this Vector2 vector, float maxMagnitude)
        {
            return Vector2.ClampMagnitude(vector, maxMagnitude);
        }

        /// <summary>
        /// Returns a vector with the specified magnitude in the same direction.
        /// Returns zero vector if original vector is zero.
        /// </summary>
        public static Vector2 WithMagnitude(this Vector2 vector, float magnitude)
        {
            return vector.sqrMagnitude > 0.0001f ? vector.normalized * magnitude : Vector2.zero;
        }

        /// <summary>
        /// Returns the direction from this vector to the target vector.
        /// </summary>
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
        {
            return (to - from).normalized;
        }

        /// <summary>
        /// Returns the distance to the target vector.
        /// </summary>
        public static float DistanceTo(this Vector2 from, Vector2 to)
        {
            return Vector2.Distance(from, to);
        }

        /// <summary>
        /// Returns a vector perpendicular to this one (rotated 90 degrees counterclockwise).
        /// </summary>
        public static Vector2 Perpendicular(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>
        /// Returns a vector perpendicular to this one (rotated 90 degrees clockwise).
        /// </summary>
        public static Vector2 PerpendicularClockwise(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>
        /// Rotates the vector by the specified angle in degrees.
        /// </summary>
        public static Vector2 Rotate(this Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        /// <summary>
        /// Reflects the vector across a surface with the given normal.
        /// </summary>
        public static Vector2 Reflect(this Vector2 vector, Vector2 normal)
        {
            return Vector2.Reflect(vector, normal);
        }

        /// <summary>
        /// Returns the largest component of the vector.
        /// </summary>
        public static float MaxComponent(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>
        /// Returns the smallest component of the vector.
        /// </summary>
        public static float MinComponent(this Vector2 vector)
        {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>
        /// Checks if this point is inside a circle.
        /// </summary>
        public static bool IsInsideCircle(this Vector2 point, Vector2 center, float radius)
        {
            return point.InRangeOf(center, radius);
        }

        /// <summary>
        /// Checks if this point is inside a rectangle defined by center and size.
        /// </summary>
        /// <summary>
        /// Checks if this point is inside a rectangle defined by center and size.
        /// </summary>
        public static bool IsInsideRect(this Vector2 point, Vector2 center, Vector2 size)
        {
            Vector2 halfSize = size * 0.5f;
            Vector2 min = center - halfSize;
            Vector2 max = center + halfSize;
            return point.x >= min.x && point.x <= max.x &&
                    point.y >= min.y && point.y <= max.y;
        }

        /// <summary>
        /// Checks if this point is inside a Unity Rect.
        /// </summary>
        public static bool IsInsideRect(this Vector2 point, Rect rect)
        {
            return rect.Contains(point);
        }

        /// <summary>
        /// Computes a random point in an annulus (a ring-shaped area) based on minimum and 
        /// maximum radius values around a central Vector2 point (origin).
        /// </summary>
        public static Vector2 RandomPointInAnnulus(this Vector2 origin, float minRadius, float maxRadius)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Calculate the position vector
            Vector2 position = direction * distance;
            return origin + position;
        }
    }
}


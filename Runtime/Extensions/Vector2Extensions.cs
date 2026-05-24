using System;
using TRnK.Constant;
using UnityEngine;

namespace TRnK.Extensions
{
    public static class Vector2Extensions
    {
        /// <summary>Returns a new Vector2 with the specified components replaced.</summary>
        public static Vector2 With(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(x ?? vector.x, y ?? vector.y);
        }

        /// <summary>Adds the specified components to the current vector.</summary>
        public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x + (x ?? 0), vector.y + (y ?? 0));
        }

        /// <summary>Subtracts the specified components from the current vector.</summary>
        public static Vector2 Subtract(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x - (x ?? 0), vector.y - (y ?? 0));
        }

        /// <summary>Multiplies the current vector by the specified components.</summary>
        public static Vector2 Multiply(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(vector.x * (x ?? 1), vector.y * (y ?? 1));
        }

        /// <summary>Divides by the specified components (zero/null divisors are ignored).</summary>
        public static Vector2 Divide(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(
                x.HasValue && x.Value != 0 ? vector.x / x.Value : vector.x,
                y.HasValue && y.Value != 0 ? vector.y / y.Value : vector.y
            );
        }

        /// <summary>Returns true if within range of the target (negative range returns false).</summary>
        public static bool InRangeOf(this Vector2 current, Vector2 target, float range)
        {
            if (range < 0f) return false;
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>Returns a vector with the given magnitude in the same direction (zero if original is zero).</summary>
        public static Vector2 WithMagnitude(this Vector2 vector, float magnitude)
        {
            return vector.sqrMagnitude > Constants.NearZeroSqrMagnitude ? vector.normalized * magnitude : Vector2.zero;
        }

        /// <summary>Returns the unit direction to the target, or zero if equal.</summary>
        public static Vector2 DirectionTo(this Vector2 from, Vector2 to)
        {
            Vector2 dir = to - from;
            return dir.sqrMagnitude > 0f ? dir.normalized : Vector2.zero;
        }

        /// <summary>Returns the distance to the target vector.</summary>
        public static float DistanceTo(this Vector2 from, Vector2 to)
        {
            return Vector2.Distance(from, to);
        }

        /// <summary>Returns a vector perpendicular to this one (rotated 90 degrees counterclockwise).</summary>
        public static Vector2 Perpendicular(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>Returns a vector perpendicular to this one (rotated 90 degrees clockwise).</summary>
        public static Vector2 PerpendicularClockwise(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        /// <summary>Rotates the vector by the specified angle in degrees.</summary>
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

        /// <summary>Returns the largest component of the vector.</summary>
        public static float MaxComponent(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }

        /// <summary>Returns the smallest component of the vector.</summary>
        public static float MinComponent(this Vector2 vector)
        {
            return Mathf.Min(vector.x, vector.y);
        }

        /// <summary>Checks if this point is inside a circle.</summary>
        public static bool IsInsideCircle(this Vector2 point, Vector2 center, float radius)
        {
            return point.InRangeOf(center, radius);
        }

        /// <summary>Checks if this point is inside a rectangle defined by center and size.</summary>
        public static bool IsInsideRect(this Vector2 point, Vector2 center, Vector2 size)
        {
            Vector2 halfSize = size * 0.5f;
            Vector2 min = center - halfSize;
            Vector2 max = center + halfSize;
            return point.x >= min.x && point.x <= max.x &&
                    point.y >= min.y && point.y <= max.y;
        }

        /// <summary>Returns a random point on a circle of the given radius around the origin.</summary>
        public static Vector2 RandomPointOnCircle(this Vector2 origin, float radius)
        {
            if (radius < 0f)
                throw new ArgumentException("radius cannot be negative", nameof(radius));

            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            return origin + direction * radius;
        }

        /// <summary>Returns a random point inside a disk of the given radius around the origin.</summary>
        public static Vector2 RandomPointInDisk(this Vector2 origin, float radius)
        {
            if (radius < 0f)
                throw new ArgumentException("radius cannot be negative", nameof(radius));

            return origin + UnityEngine.Random.insideUnitCircle * radius;
        }

        /// <summary>Returns a random point in an annulus (ring) between minRadius and maxRadius around the origin.</summary>
        public static Vector2 RandomPointInAnnulus(this Vector2 origin, float minRadius, float maxRadius)
        {
            if (minRadius < 0f)
                throw new ArgumentException("minRadius cannot be negative", nameof(minRadius));
            if (maxRadius < minRadius)
                throw new ArgumentException("maxRadius cannot be less than minRadius", nameof(maxRadius));

            float angle = UnityEngine.Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(UnityEngine.Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Calculate the position vector
            Vector2 position = direction * distance;
            return origin + position;
        }
    }
}


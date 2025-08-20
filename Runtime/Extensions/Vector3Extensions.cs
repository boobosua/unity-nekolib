using UnityEngine;

namespace NekoLib.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Converts a Vector3 to a Vector2 by dropping the z component.
        /// </summary>
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return (Vector2)vector;
        }

        /// <summary>
        /// Returns a new Vector3 with the specified components replaced.
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        /// <summary>
        /// Adds the specified components to the current vector.
        /// </summary>
        public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
        }

        /// <summary>
        /// Subtracts the specified components from the current vector.
        /// </summary>
        public static Vector3 Subtract(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x - (x ?? 0), vector.y - (y ?? 0), vector.z - (z ?? 0));
        }

        /// <summary>
        /// Multiplies the current vector by the specified components.
        /// </summary>
        public static Vector3 Multiply(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(vector.x * (x ?? 1), vector.y * (y ?? 1), vector.z * (z ?? 1));
        }

        /// <summary>
        /// Divides the current vector by the specified components.
        /// </summary>
        public static Vector3 Divide(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                x.HasValue && x.Value != 0 ? vector.x / x.Value : vector.x,
                y.HasValue && y.Value != 0 ? vector.y / y.Value : vector.y,
                z.HasValue && z.Value != 0 ? vector.z / z.Value : vector.z
            );
        }

        /// <summary>
        /// Rotates the vector around the X-axis by the given angle in degrees.
        /// </summary>
        public static Vector3 RotateX(this Vector3 vector, float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.right) * vector;
        }

        /// <summary>
        /// Rotates the vector around the Y-axis by the given angle in degrees.
        /// </summary>
        public static Vector3 RotateY(this Vector3 vector, float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.up) * vector;
        }

        /// <summary>
        /// Rotates the vector around the Z-axis by the given angle in degrees.
        /// </summary>
        public static Vector3 RotateZ(this Vector3 vector, float degrees)
        {
            return Quaternion.AngleAxis(degrees, Vector3.forward) * vector;
        }

        /// <summary>
        /// Returns a Boolean indicating whether the current Vector3 is in a given range from another Vector3
        /// </summary>
        public static bool InRangeOf(this Vector3 current, Vector3 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Returns a vector with the same direction but clamped to a maximum magnitude.
        /// </summary>
        public static Vector3 ClampMagnitude(this Vector3 vector, float maxMagnitude)
        {
            return Vector3.ClampMagnitude(vector, maxMagnitude);
        }

        /// <summary>
        /// Returns a vector with the specified magnitude in the same direction (0 if original vector is zero).
        /// </summary>
        public static Vector3 WithMagnitude(this Vector3 vector, float magnitude)
        {
            return vector.sqrMagnitude > 0.0001f ? vector.normalized * magnitude : Vector3.zero;
        }

        /// <summary>
        /// Returns the direction from this vector to the target vector.
        /// </summary>
        public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
        {
            return (to - from).normalized;
        }

        /// <summary>
        /// Returns the distance to the target vector.
        /// </summary>
        public static float DistanceTo(this Vector3 from, Vector3 to)
        {
            return Vector3.Distance(from, to);
        }

        /// <summary>
        /// Reflects the vector across a surface with the given normal.
        /// </summary>
        public static Vector3 Reflect(this Vector3 vector, Vector3 normal)
        {
            return Vector3.Reflect(vector, normal);
        }

        /// <summary>
        /// Projects this vector onto another vector.
        /// </summary>
        public static Vector3 ProjectOnto(this Vector3 vector, Vector3 onto)
        {
            return Vector3.Project(vector, onto);
        }

        /// <summary>
        /// Returns the largest component of the vector.
        /// </summary>
        public static float MaxComponent(this Vector3 vector)
        {
            return Mathf.Max(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns the smallest component of the vector.
        /// </summary>
        public static float MinComponent(this Vector3 vector)
        {
            return Mathf.Min(vector.x, vector.y, vector.z);
        }

        /// <summary>
        /// Checks if this point is inside a sphere.
        /// </summary>
        public static bool IsInsideSphere(this Vector3 point, Vector3 center, float radius)
        {
            return point.InRangeOf(center, radius);
        }

        /// <summary>
        /// Checks if this point is inside a box defined by center and size.
        /// </summary>
        public static bool IsInsideBox(this Vector3 point, Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size * 0.5f;
            Vector3 min = center - halfSize;
            Vector3 max = center + halfSize;
            return point.x >= min.x && point.x <= max.x &&
                    point.y >= min.y && point.y <= max.y &&
                    point.z >= min.z && point.z <= max.z;
        }

        /// <summary>
        /// Checks if this point is inside Unity Bounds.
        /// </summary>
        public static bool IsInsideBounds(this Vector3 point, Bounds bounds)
        {
            return bounds.Contains(point);
        }

        /// <summary>
        /// Returns the closest point on the surface of bounds to this point.
        /// </summary>
        public static Vector3 ClosestPointOnBounds(this Vector3 point, Bounds bounds)
        {
            return bounds.ClosestPoint(point);
        }

        /// <summary>
        /// Checks if this point is inside a Collider's bounds.
        /// </summary>
        public static bool IsInsideCollider(this Vector3 point, Collider collider)
        {
            return collider.bounds.Contains(point);
        }

        /// <summary>
        /// Computes a random point in an annulus (ring-shaped area) around a central Vector3 point.
        /// </summary>
        /// <param name="plane">The plane to generate the point on (XY, XZ, or YZ)</param>
        public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius, Plane2D plane = Plane2D.XZ)
        {
            if (minRadius < 0f)
                throw new System.ArgumentException("minRadius cannot be negative", nameof(minRadius));
            if (maxRadius < minRadius)
                throw new System.ArgumentException("maxRadius cannot be less than minRadius", nameof(maxRadius));

            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Calculate the position vector based on the specified plane
            Vector3 position = plane switch
            {
                Plane2D.XY => new Vector3(direction.x, direction.y, 0) * distance,
                Plane2D.XZ => new Vector3(direction.x, 0, direction.y) * distance,
                Plane2D.YZ => new Vector3(0, direction.x, direction.y) * distance,
                _ => new Vector3(direction.x, 0, direction.y) * distance
            };

            return origin + position;
        }
    }

    public enum Plane2D
    {
        XY, // Horizontal plane (standard 2D)
        XZ, // Ground plane (typical for 3D games)
        YZ  // Vertical plane
    }
}


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
        /// Returns a Boolean indicating whether the current Vector3 is in a given range from another Vector3
        /// </summary>
        public static bool InRangeOf(this Vector3 current, Vector3 target, float range)
        {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Computes a random point in an annulus (ring-shaped area) around a central Vector3 point.
        /// </summary>
        /// <param name="plane">The plane to generate the point on (XY, XZ, or YZ)</param>
        public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius, AnnulusPlane plane = AnnulusPlane.XZ)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

            // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Calculate the position vector based on the specified plane
            Vector3 position = plane switch
            {
                AnnulusPlane.XY => new Vector3(direction.x, direction.y, 0) * distance,
                AnnulusPlane.XZ => new Vector3(direction.x, 0, direction.y) * distance,
                AnnulusPlane.YZ => new Vector3(0, direction.x, direction.y) * distance,
                _ => new Vector3(direction.x, 0, direction.y) * distance
            };

            return origin + position;
        }
    }

    public enum AnnulusPlane
    {
        XY, // Horizontal plane (standard 2D)
        XZ, // Ground plane (typical for 3D games)
        YZ  // Vertical plane
    }
}


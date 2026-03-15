namespace NekoLib.Constant
{
    /// <summary>Constants referenced in two or more scripts across NekoLib.</summary>
    public static class Constants
    {
        // Vector math
        /// <summary>Minimum sqrMagnitude to treat a vector as non-zero (avoids division by zero in normalisation).</summary>
        public const float NearZeroSqrMagnitude = 0.0001f;

        // Rotation
        /// <summary>Full rotation in degrees.</summary>
        public const float FullRotation = 360f;
    }
}

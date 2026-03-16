namespace NekoLib.Constant
{
    /// <summary>Constants referenced in two or more scripts across NekoLib.</summary>
    public static class Constants
    {
        /// <summary>Minimum sqrMagnitude to treat a vector as non-zero (avoids division by zero in normalisation).</summary>
        public const float NearZeroSqrMagnitude = 0.0001f;

        /// <summary>Full rotation in degrees.</summary>
        public const float FullRotation = 360f;
    }
}

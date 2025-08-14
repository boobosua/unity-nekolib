using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NekoLib.Extensions
{
    public static class NumberExtensions
    {
        public static bool IsEven(this int num)
        {
            return num % 2 == 0;
        }

        public static bool IsOdd(this int num)
        {
            return num % 2 != 0;
        }

        public static float PercentageOf(this int current, int total)
        {
            if (total == 0)
                return 0;

            return Mathf.Clamp01((float)current / total);
        }

        public static float PercentageOf(this float current, float total)
        {
            if (total == 0)
                return 0;

            return Mathf.Clamp01(current / total);
        }

        /// <summary>
        /// Prevents a number from going below a minimum.
        /// </summary>
        public static int AtLeast(this int num, int min)
        {
            return Math.Max(num, min);
        }

        /// <summary>
        /// Prevents a number from going above a maximum.
        /// </summary>
        public static int AtMost(this int num, int max)
        {
            return Math.Min(num, max);
        }

        /// <summary>
        /// Prevents a number from going below a minimum.
        /// </summary>
        public static float AtLeast(this float num, float min)
        {
            return Mathf.Max(num, min);
        }

        /// <summary>
        /// Prevents a number from going above a maximum.
        /// </summary>
        public static float AtMost(this float num, float max)
        {
            return Mathf.Min(num, max);
        }

        /// <summary>
        /// Returns true if a random roll succeeds based on the probability rate.
        /// </summary>
        /// <param name="probability">The probability value (0.0 = never, 1.0 = always)</param>
        /// <param name="min">Minimum range value</param>
        /// <param name="max">Maximum range value</param>
        public static bool RollChance(this float probability, float min = 0f, float max = 1f)
        {
            if (probability < min || probability > max)
            {
                throw new ArgumentException($"probability '{probability}' must be in range [{min}, {max}]");
            }

            var randomRoll = Random.Range(min, max);
            return randomRoll <= probability;
        }

        /// <summary>
        /// Returns true if a random roll succeeds based on the percentage rate.
        /// </summary>
        /// <param name="probability">The percentage value (0 = never, 100 = always)</param>
        /// <param name="min">Minimum range value</param>
        /// <param name="max">Maximum range value</param>
        public static bool RollChance(this int probability, int min = 0, int max = 100)
        {
            if (probability < min || probability > max)
            {
                throw new ArgumentException($"percentage '{probability}' must be in range [{min}, {max}]");
            }

            // For int Random.Range, max is exclusive, so we add 1 to include our max value
            var randomRoll = Random.Range(min, max + 1);
            return randomRoll <= probability;
        }
    }
}


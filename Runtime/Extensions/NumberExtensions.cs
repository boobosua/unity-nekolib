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
            return num % 2 == 1;
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
        /// Returns 'true' if the random roll between min and max is less than or equal to the target roll.
        /// </summary>
        public static bool HasChance(this float rate, float min = 0f, float max = 1f)
        {
            if (rate < min || rate > max)
            {
                throw new ArgumentException($"rate '{rate}' is not in range of min '{min}' and max '{max}'");
            }

            var randomRoll = Random.Range(min, max);
            return randomRoll <= rate;
        }

        /// <summary>
        /// Returns 'true' if the random roll between min and max is less than or equal to the target roll.
        /// </summary>
        public static bool HasChance(int rate, int min = 0, int max = 100)
        {
            if (rate < min || rate > max)
            {
                throw new ArgumentException($"rate '{rate}' is not in range of min '{min}' and max '{max}'");
            }

            var randomRoll = Random.Range(min, max);
            return randomRoll <= rate;
        }
    }
}


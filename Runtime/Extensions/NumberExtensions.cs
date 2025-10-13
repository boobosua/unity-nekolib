using System;
using NekoLib.Logger;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NekoLib.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Returns true if the number is even.
        /// </summary>
        public static bool IsEven(this int num)
        {
            return num % 2 == 0;
        }

        /// <summary>
        /// Returns true if the number is odd.
        /// </summary>
        public static bool IsOdd(this int num)
        {
            return num % 2 != 0;
        }

        /// <summary>
        /// Returns the percentage of the current value relative to the total.
        /// </summary>
        public static float PercentageOf(this int current, int total)
        {
            if (total == 0)
                return 0;

            return Mathf.Clamp01((float)current / total);
        }

        /// <summary>
        /// Returns the percentage of the current value relative to the total.
        /// </summary>
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

        /// <summary>
        /// Converts an integer to the specified enum type.
        /// </summary>
        public static T ToEnum<T>(this int value) where T : struct, Enum
        {
            var stringValue = value.ToString();
            if (Enum.TryParse<T>(stringValue, out var result))
                return result;

            throw new ArgumentException($"Unable to parse {value} as {typeof(T).Name}");
        }

        /// <summary>
        /// Converts an integer to the specified enum type, or returns a default value if the conversion fails.
        /// </summary>
        public static T ToEnumOrDefault<T>(this int value, T defaultValue = default) where T : struct, Enum
        {
            var stringValue = value.ToString();
            if (Enum.TryParse<T>(stringValue, out var result))
                return result;

            Log.Warn($"Failed to parse int {value} to enum {typeof(T).Name}, using default: {defaultValue}");
            return defaultValue;
        }
    }
}


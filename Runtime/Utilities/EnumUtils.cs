using System;
using System.Linq;
using System.Collections.Generic;
using NekoLib.Extensions;

using Random = UnityEngine.Random;

namespace NekoLib.Utilities
{
    public static class EnumUtils
    {
        /// <summary>
        /// Get a random enum value.
        /// </summary>
        public static T GetRandom<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Range(0, values.Length));
        }

        /// <summary>
        /// Get a random enum value excluding specific ones.
        /// </summary>
        public static T GetRandomExcept<T>(params T[] excludeEnums) where T : Enum
        {
            var allEnums = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            if (excludeEnums.IsNullOrEmpty())
                return GetRandom<T>();

            var excludeSet = new HashSet<T>(excludeEnums);
            var availableEnums = allEnums.Where(value => !excludeSet.Contains(value)).ToArray();

            if (availableEnums.IsNullOrEmpty())
                throw new InvalidOperationException("No available enum values after exclusion.");

            return availableEnums[Random.Range(0, availableEnums.Length)];
        }

        /// <summary>
        /// Get count of enum values.
        /// </summary>
        public static int Count<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Length;
        }

        /// <summary>
        /// Get all enum values excluding specific ones.
        /// </summary>
        public static T[] AllExcept<T>(params T[] excludeEnums) where T : Enum
        {
            var allEnums = Enum.GetValues(typeof(T)).Cast<T>().ToArray();

            if (excludeEnums.IsNullOrEmpty())
                return allEnums;

            var excludeSet = new HashSet<T>(excludeEnums);
            return allEnums.Where(value => !excludeSet.Contains(value)).ToArray();
        }

        /// <summary>
        /// Iterates over all enum values and applies the given action.
        /// </summary>
        public static void ForEach<T>(Action<T> action) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            for (int i = 0; i < values.Length; i++)
            {
                action?.Invoke((T)values.GetValue(i));
            }
        }
    }
}

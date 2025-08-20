using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace NekoLib.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns a random element from the array.
        /// </summary>
        public static T Rand<T>(this T[] arr)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random element from null or empty array");
            return arr[Random.Range(0, arr.Length)];
        }

        /// <summary>
        /// Returns a random index from the array.
        /// </summary>
        public static int RandIndex<T>(this T[] arr)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random index from null or empty array");
            return Random.Range(0, arr.Length);
        }

        /// <summary>
        /// Shuffles the elements of the array.
        /// </summary>
        public static T[] Shuffle<T>(this T[] arr)
        {
            T[] result = new T[arr.Length];
            arr.CopyTo(result, 0);

            for (int i = 0; i < result.Length; i++)
            {
                T temp = result[i];
                int randomIndex = Random.Range(i, result.Length);
                result[i] = result[randomIndex];
                result[randomIndex] = temp;
            }

            return result;
        }

        /// <summary>
        /// Swaps two elements in the array.
        /// </summary>
        public static T[] Swap<T>(this T[] arr, int aIndex, int bIndex)
        {
            if (aIndex < 0 || aIndex >= arr.Length || bIndex < 0 || bIndex >= arr.Length)
                throw new IndexOutOfRangeException($"Index {aIndex} or {bIndex} is out of bounds of array of length {arr.Length}");

            if (aIndex == bIndex)
                return arr.Clone() as T[];

            T[] result = arr.Clone() as T[];
            (result[bIndex], result[aIndex]) = (result[aIndex], result[bIndex]);
            return result;
        }

        /// <summary>
        /// Swaps two elements in the array.
        /// </summary>
        public static T[] Swap<T>(this T[] arr, T a, T b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException("a or b cannot be null");

            int aIndex = Array.IndexOf(arr, a);
            int bIndex = Array.IndexOf(arr, b);

            if (aIndex == -1)
                throw new ArgumentException($"Element '{a}' not found in array");
            if (bIndex == -1)
                throw new ArgumentException($"Element '{b}' not found in array");

            return Swap(arr, aIndex, bIndex);
        }

        /// <summary>
        /// Checks if the array is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] arr)
        {
            return arr == null || arr.Length == 0;
        }

        /// <summary>
        /// Returns a string representation of an array.
        /// Does not work on nested arrays or array that is nested in other structures.
        /// </summary>
        public static string Format<T>(this T[] arr)
        {
            // Handle null.
            if (arr == null)
                return "null";

            // Handle empty.
            if (arr.Length == 0)
                return "[]";

            // Format the array.
            return "[" + string.Join(", ", arr) + "]";
        }

        /// <summary>
        /// Returns a random element from the list.
        /// </summary>
        public static T Rand<T>(this List<T> list)
        {
            if (list.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random element from null or empty list");
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random index from the list.
        /// </summary>
        public static int RandIndex<T>(this List<T> list)
        {
            if (list.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random index from null or empty list");
            return Random.Range(0, list.Count);
        }

        /// <summary>
        /// Shuffles the elements of the list.
        /// </summary>
        public static List<T> Shuffle<T>(this List<T> list)
        {
            List<T> result = new(list);
            for (int i = 0; i < result.Count; i++)
            {
                T temp = result[i];
                int randomIndex = Random.Range(i, result.Count);
                result[i] = result[randomIndex];
                result[randomIndex] = temp;
            }
            return result;
        }

        /// <summary>
        /// Swaps two elements in the list.
        /// </summary>
        public static List<T> Swap<T>(this List<T> list, int aIndex, int bIndex)
        {
            if (aIndex < 0 || aIndex >= list.Count || bIndex < 0 || bIndex >= list.Count)
                throw new IndexOutOfRangeException($"Index {aIndex} or {bIndex} is out of bounds of list of length {list.Count}");

            if (aIndex == bIndex)
                return new List<T>(list);

            List<T> result = new(list);
            (result[bIndex], result[aIndex]) = (result[aIndex], result[bIndex]);
            return result;
        }

        /// <summary>
        /// Swaps two elements in the list.
        /// </summary>
        public static List<T> Swap<T>(this List<T> list, T a, T b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException("a or b cannot be null");

            int aIndex = list.IndexOf(a);
            int bIndex = list.IndexOf(b);

            if (aIndex == -1)
                throw new ArgumentException($"Element '{a}' not found in list");
            if (bIndex == -1)
                throw new ArgumentException($"Element '{b}' not found in list");

            return Swap(list, aIndex, bIndex);
        }

        /// <summary>
        /// Checks if the list is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Returns a string representation of a list.
        /// Does not work on nested lists or list that is nested in other structures.
        /// </summary>
        public static string Format<T>(this List<T> list)
        {
            // Handle null.
            if (list == null)
                return "null";

            // Handle empty.
            if (list.Count == 0)
                return "[]";

            // Format the list.
            return "{" + string.Join(", ", list) + "}";
        }

        /// <summary>
        /// Returns a random value from a dictionary.
        /// </summary>
        public static V RandV<K, V>(this Dictionary<K, V> dict)
        {
            if (dict.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random value from null or empty dictionary");
            return dict.Values.ToArray().Rand();
        }

        /// <summary>
        /// Returns a random key from a dictionary.
        /// </summary>
        public static K RandK<K, V>(this Dictionary<K, V> dict)
        {
            if (dict.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random key from null or empty dictionary");
            return dict.Keys.ToArray().Rand();
        }

        public static bool IsNullOrEmpty<K, V>(this Dictionary<K, V> dict)
        {
            return dict == null || dict.Count == 0;
        }

        /// <summary>
        /// Returns a string representation of a dictionary.
        /// Does not work on nested dictionaries or dictionary that is nested in other structures.
        /// </summary>
        public static string Format<K, V>(this Dictionary<K, V> dict)
        {
            // Handle null.
            if (dict == null)
                return "null";

            // Handle empty.
            if (dict.Count == 0)
                return "{}";

            return "{" + string.Join(", ", dict
                .Select(kvp => $"{kvp.Key}: {kvp.Value}")) + "}";
        }

        /// <summary>
        /// Returns a string representation of a queue.
        /// Does not work on nested queues or queue that is nested in other structures.
        /// </summary>
        public static string Format<T>(this Queue<T> queue)
        {
            return queue.ToList().Format();
        }

        /// <summary>
        /// Returns a string representation of a stack.
        /// Does not work on nested stacks or stack that is nested in other structures.
        /// </summary>
        public static string Format<T>(this Stack<T> stack)
        {
            return stack.ToList().Format();
        }

        /// <summary>
        /// Returns a string representation of a hashset.
        /// Does not work on nested hashsets or hashset that is nested in other structures.
        /// </summary>
        public static string Format<T>(this HashSet<T> set)
        {
            return set.ToList().Format();
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace NekoLib.Extensions
{
    public static class CollectionExtensions
    {
        #region Array
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
        /// Checks if the array contains any null elements.
        /// </summary>
        public static bool HasNullElements<T>(this T[] arr) where T : class
        {
            if (arr.IsNullOrEmpty()) return false;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == null) return true;
            }
            return false;
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
        /// Gets the last element of the array.
        /// </summary>
        public static T Last<T>(this T[] arr)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get last element from null or empty array");
            return arr[^1];
        }

        /// <summary>
        /// Gets the first element of the array.
        /// </summary>
        public static T First<T>(this T[] arr)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get first element from null or empty array");
            return arr[0];
        }

        /// <summary>
        /// Gets a sub-array from the specified start index to the end.
        /// </summary>
        public static T[] Slice<T>(this T[] arr, int startIndex)
        {
            return arr.Slice(startIndex, arr.Length - startIndex);
        }

        /// <summary>
        /// Gets a sub-array from the specified start index with the specified length.
        /// </summary>
        public static T[] Slice<T>(this T[] arr, int startIndex, int length)
        {
            if (arr.IsNullOrEmpty())
                return new T[0];
            if (startIndex < 0 || startIndex >= arr.Length)
                throw new IndexOutOfRangeException($"Start index {startIndex} is out of bounds of array of length {arr.Length}");
            if (length < 0 || startIndex + length > arr.Length)
                throw new ArgumentException($"Length {length} would exceed array bounds from start index {startIndex}");

            T[] result = new T[length];
            Array.Copy(arr, startIndex, result, 0, length);
            return result;
        }

        /// <summary>
        /// Checks if the array contains the specified element.
        /// </summary>
        public static bool Contains<T>(this T[] arr, T item)
        {
            return Array.IndexOf(arr, item) != -1;
        }

        /// <summary>
        /// Reverses the array and returns a new array.
        /// </summary>
        public static T[] Reverse<T>(this T[] arr)
        {
            if (arr.IsNullOrEmpty())
                return arr?.Clone() as T[] ?? new T[0];

            T[] result = arr.Clone() as T[];
            Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns multiple random elements from the array without replacement.
        /// </summary>
        public static T[] RandMultiple<T>(this T[] arr, int count)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random elements from null or empty array");
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");
            if (count > arr.Length)
                throw new ArgumentException($"Cannot get {count} elements from array of length {arr.Length}");

            if (count == 0)
                return new T[0];

            var shuffled = arr.Shuffle();
            return shuffled.Slice(0, count);
        }

        /// <summary>
        /// Returns a random element from the collection using weighted selection.
        /// </summary>
        public static T RandWeighted<T>(this T[] arr, Func<T, float> weightSelector)
        {
            if (arr.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random element from null or empty array");

            float totalWeight = arr.Sum(weightSelector);
            if (totalWeight <= 0)
                throw new InvalidOperationException("Total weight must be greater than 0");

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (T item in arr)
            {
                currentWeight += weightSelector(item);
                if (randomValue <= currentWeight)
                    return item;
            }

            return arr.Last(); // Fallback, should rarely happen due to floating point precision
        }
        #endregion

        #region List
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
        /// Checks if the list contains any null elements.
        /// </summary>
        public static bool HasNullElements<T>(this List<T> list) where T : class
        {
            if (list.IsNullOrEmpty()) return false;

            foreach (var item in list)
            {
                if (item == null) return true;
            }
            return false;
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
        /// Returns the last element of the list.
        /// </summary>
        public static T Last<T>(this List<T> list)
        {
            if (list.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get last element from null or empty list");
            return list[^1];
        }

        /// <summary>
        /// Returns the first element of the list.
        /// </summary>
        public static T First<T>(this List<T> list)
        {
            if (list.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get first element from null or empty list");
            return list[0];
        }

        /// <summary>
        /// Returns multiple random elements from the list without replacement.
        /// </summary>
        public static List<T> RandMultiple<T>(this List<T> list, int count)
        {
            if (list.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random elements from null or empty list");
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");
            if (count > list.Count)
                throw new ArgumentException($"Cannot get {count} elements from list of length {list.Count}");

            if (count == 0)
                return new List<T>();

            var shuffled = list.Shuffle();
            return shuffled.Take(count).ToList();
        }

        /// <summary>
        /// Returns a random element from the list using weighted selection.
        /// </summary>
        public static T RandWeighted<T>(this List<T> list, Func<T, float> weightSelector)
        {
            return list.ToArray().RandWeighted(weightSelector);
        }
        #endregion

        #region Dictionary
        /// <summary>
        /// Returns a random value from a dictionary.
        /// </summary>
        public static V RandV<K, V>(this Dictionary<K, V> dict) where K : notnull
        {
            if (dict.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random value from null or empty dictionary");
            return dict.Values.ToArray().Rand();
        }

        /// <summary>
        /// Returns a random key from a dictionary.
        /// </summary>
        public static K RandK<K, V>(this Dictionary<K, V> dict) where K : notnull
        {
            if (dict.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get random key from null or empty dictionary");
            return dict.Keys.ToArray().Rand();
        }

        /// <summary>
        /// Checks if the dictionary contains any null values.
        /// </summary>
        public static bool HasNullValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            where TValue : class
        {
            if (dictionary.IsNullOrEmpty()) return false;

            foreach (var kvp in dictionary)
            {
                if (kvp.Value == null) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the dictionary is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<K, V>(this Dictionary<K, V> dict) where K : notnull
        {
            return dict == null || dict.Count == 0;
        }

        /// <summary>
        /// Returns a new copy of the dictionary.
        /// </summary>
        public static Dictionary<K, V> AsNewCopy<K, V>(this Dictionary<K, V> dict) where K : notnull
        {
            return dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Returns a string representation of a dictionary.
        /// Does not work on nested dictionaries or dictionary that is nested in other structures.
        /// </summary>
        public static string Format<K, V>(this Dictionary<K, V> dict) where K : notnull
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
        #endregion

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


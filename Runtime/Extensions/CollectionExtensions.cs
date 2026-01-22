using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NekoLib.Collections;
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
        public static bool ContainsNull<T>(this T[] arr) where T : class
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
        public static string ToLiteral<T>(this T[] arr)
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

        public static bool IsNullOrEmpty<T>(this T[,] grid)
        {
            return grid == null || grid.Length == 0;
        }

        public static string ToLiteral<T>(this T[,] grid)
        {
            if (grid == null)
                return "null";

            int height = grid.GetLength(0);
            int width = grid.GetLength(1);
            var sb = new StringBuilder();
            sb.Append("{");
            for (int y = 0; y < height; y++)
            {
                if (y > 0) sb.Append(", ");
                sb.Append(y);
                sb.Append(": [");
                for (int x = 0; x < width; x++)
                {
                    if (x > 0) sb.Append(", ");
                    var v = grid[y, x];
                    sb.Append(v?.ToString() ?? "null");
                }
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
        }
        #endregion

        #region Grid<T>
        /// <summary>
        /// Checks if the grid contains the specified element.
        /// </summary>
        public static bool Contains<T>(this Grid<T> grid, T item)
        {
            if (grid.IsNullOrEmpty())
                return false;

            // Linear scan over the contiguous buffer is faster than nested x/y loops
            // and matches the grid's row-major layout.
            var cmp = EqualityComparer<T>.Default;
            var span = grid.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                if (cmp.Equals(span[i], item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the (x, y) index of the specified element in the grid; throws if not found.
        /// </summary>
        public static (int x, int y) IndexOf<T>(this Grid<T> grid, T item)
        {
            if (grid.TryIndexOf(item, out var pos))
                return (pos.x, pos.y);
            throw new InvalidOperationException("Element not found in grid");
        }

        public static bool TryIndexOf<T>(this Grid<T> grid, T item, out (int x, int y) pos)
        {
            if (grid.IsNullOrEmpty())
            {
                pos = default;
                return false;
            }

            var cmp = EqualityComparer<T>.Default;
            int w = grid.Width;
            var span = grid.AsSpan();
            int len = span.Length;

            for (int i = 0; i < len; i++)
            {
                if (cmp.Equals(span[i], item))
                {
                    pos = (i % w, i / w);
                    return true;
                }
            }

            pos = default;
            return false;
        }

        /// <summary>
        /// Returns the last (x, y) index of the specified element in the grid; throws if not found.
        /// </summary>
        public static (int x, int y) LastIndexOf<T>(this Grid<T> grid, T item)
        {
            if (grid.TryLastIndexOf(item, out var pos))
                return (pos.x, pos.y);
            throw new InvalidOperationException("Element not found in grid");
        }

        /// <summary>
        /// Tries to find the last occurrence of the specified element scanning from the end.
        /// </summary>
        public static bool TryLastIndexOf<T>(this Grid<T> grid, T item, out (int x, int y) pos)
        {
            if (grid.IsNullOrEmpty())
            {
                pos = default;
                return false;
            }

            var cmp = EqualityComparer<T>.Default;
            int w = grid.Width;
            var span = grid.AsSpan();

            for (int i = span.Length - 1; i >= 0; i--)
            {
                if (cmp.Equals(span[i], item))
                {
                    pos = (i % w, i / w);
                    return true;
                }
            }

            pos = default;
            return false;
        }

        /// <summary>
        /// Returns the first element of the grid.
        /// </summary>
        public static T First<T>(this Grid<T> grid)
        {
            if (grid.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get first element from null or empty grid");
            return grid[0, 0];
        }

        /// <summary>
        /// Returns the last element of the grid.
        /// </summary>
        public static T Last<T>(this Grid<T> grid)
        {
            if (grid.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot get last element from null or empty grid");
            return grid[grid.Width - 1, grid.Height - 1];
        }

        /// <summary>
        /// Checks if the grid is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this Grid<T> grid)
        {
            return grid == null || grid.Width == 0 || grid.Height == 0;
        }

        /// <summary>
        /// Checks if the grid contains any null elements.
        /// </summary>
        public static bool ContainsNull<T>(this Grid<T> grid) where T : class
        {
            if (grid.IsNullOrEmpty()) return false;

            // Faster: linear scan over the contiguous buffer.
            var span = grid.AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == null) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a string representation of a grid.
        /// Does not work on nested grids or grid that is nested in other structures.
        /// </summary>
        public static string ToLiteral<T>(this Grid<T> grid)
        {
            if (grid == null)
                return "null";

            var sb = new StringBuilder();
            sb.Append($"Grid<{typeof(T).Name}>({grid.Width}x{grid.Height}): ");
            sb.Append("{");
            for (int y = 0; y < grid.Height; y++)
            {
                if (y > 0) sb.Append(", ");
                sb.Append(y);
                sb.Append(": [");
                for (int x = 0; x < grid.Width; x++)
                {
                    if (x > 0) sb.Append(", ");
                    var v = grid[x, y];
                    sb.Append(v?.ToString() ?? "null");
                }
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
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
        public static bool ContainsNull<T>(this List<T> list) where T : class
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
        /// Checks if the read-only list is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IReadOnlyList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Checks if the collection is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }


        /// <summary>
        /// Returns a string representation of a list.
        /// Does not work on nested lists or list that is nested in other structures.
        /// </summary>
        public static string ToLiteral<T>(this List<T> list)
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
        public static bool ContainsNullValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
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
        public static string ToLiteral<K, V>(this Dictionary<K, V> dict) where K : notnull
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
        public static string ToLiteral<T>(this Queue<T> queue)
        {
            return queue.ToList().ToLiteral();
        }

        /// <summary>
        /// Returns a string representation of a stack.
        /// Does not work on nested stacks or stack that is nested in other structures.
        /// </summary>
        public static string ToLiteral<T>(this Stack<T> stack)
        {
            return stack.ToList().ToLiteral();
        }

        /// <summary>
        /// Returns a string representation of a hashset.
        /// Does not work on nested hashsets or hashset that is nested in other structures.
        /// </summary>
        public static string ToLiteral<T>(this HashSet<T> set)
        {
            return set.ToList().ToLiteral();
        }
    }
}


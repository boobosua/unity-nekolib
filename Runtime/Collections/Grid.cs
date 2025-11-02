#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NekoLib.Collections
{
    /// <summary>
    /// Lightweight, cache-friendly 2D grid backed by a flat array.
    /// </summary>
    [DebuggerDisplay("Grid<{typeof(T).Name}>({Width}x{Height})")]
    public sealed class Grid<T>
    {
        // Unity persistence: allow SerializeReference to persist dimensions and data.
        // Fields must be non-readonly and marked [SerializeField].
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private T[] _data;

        public int Width => _width;
        public int Height => _height;
        public int Length => _data?.Length ?? 0;

        /// <summary>
        /// Fast, unchecked indexer. Ensure x/y are valid before using.
        /// </summary>
        public ref T this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _data[x + (y * _width)];
        }

        public Grid(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            _width = width;
            _height = height;
            _data = new T[width * height];
        }

        public Grid(int width, int height, in T fillValue) : this(width, height)
        {
            Fill(fillValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InBounds(int x, int y) =>
            (uint)x < (uint)_width && (uint)y < (uint)_height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(int x, int y, out T value)
        {
            if (!InBounds(x, y))
            {
                value = default!;
                return false;
            }
            value = _data[x + (y * _width)];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySet(int x, int y, in T value)
        {
            if (!InBounds(x, y))
                return false;

            _data[x + (y * _width)] = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetRefUnchecked(int x, int y) => ref _data[x + (y * _width)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => _data.AsSpan();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetRowSpan(int y)
        {
            var start = y * _width;
            return _data.AsSpan(start, _width);
        }

        public void Fill(in T value) => Array.Fill(_data, value);
        public void Clear() => Array.Clear(_data, 0, _data.Length);

        public void CopyTo(Grid<T> destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (destination._width != _width || destination._height != _height)
                throw new ArgumentException("Destination grid size mismatch.", nameof(destination));

            Array.Copy(_data, destination._data, _data.Length);
        }

        public static Grid<T> From2D(T[,] source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            int h = source.GetLength(0);
            int w = source.GetLength(1);
            var grid = new Grid<T>(w, h);

            for (int y = 0; y < h; y++)
            {
                int start = y * w;
                for (int x = 0; x < w; x++)
                    grid._data[start + x] = source[y, x];
            }
            return grid;
        }

        /// <summary>
        /// Iterate over every cell by ref, allowing in-place modification.
        /// </summary>
        public delegate void RefAction(int x, int y, ref T value);

        public void ForEach(RefAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            int w = _width;
            int h = _height;
            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++, i++)
                    action(x, y, ref _data[i]);
            }
        }

        public override string ToString()
        {
            return $"Grid<{typeof(T).Name}>({Width}x{Height})";
        }
    }
}

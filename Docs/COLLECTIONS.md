````markdown
# NekoLib Collections

Lightweight, cache-friendly collection helpers. Primary focus: `Grid<T>` — a flat-array backed 2D grid with ref access and Span-friendly APIs.

### Grid<T>

```csharp
using NekoLib.Collections;

// Create a 10x5 grid of ints
var grid = new Grid<int>(10, 5);

// Fast unchecked access (use only when you know indices are valid)
grid[3, 2] = 42;
int v = grid[3, 2];

// Safe access
if (grid.TryGet(3, 2, out int outValue))
    Debug.Log(outValue);

// Safe write
grid.TrySet(0, 0, 7);

// Iterate by reference for in-place updates
grid.ForEach((x, y, ref int cell) =>
{
    cell += 1; // modify in-place without extra indexing overhead
});

// Get a contiguous row as a Span<T> for fast processing
Span<int> row2 = grid.GetRowSpan(2);
row2[0] = 99; // writes back into the grid

// Treat the whole grid as a flat span (row-major)
Span<int> all = grid.AsSpan();
// Use extension helpers (see Extensions docs) for searches, etc.
// e.g. all contains/IndexOf helpers operate efficiently on spans
```

### Interop with 2D arrays

```csharp
// Convert from a conventional 2D array
int[,] source = new int[5,10]; // [rows, cols]
var converted = Grid<int>.From2D(source);

// Copy between equal-sized grids
var dst = new Grid<int>(10,5);
converted.CopyTo(dst);
```

### When to use Grid<T>

- Use when you want a compact, cache-friendly 2D buffer with ref access.
- Prefer `Grid<T>` over `T[,]` for performance-critical loops, contiguous memory access, and APIs that need Span<T>.
- `Grid<T>` exposes both safe (TryGet/TrySet) and very-fast unchecked ref access via the indexer.
````

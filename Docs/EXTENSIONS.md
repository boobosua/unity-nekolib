# NekoLib Extensions

Extension methods for Unity and C# types.

## Unity Extensions

### GameObjectExtensions

```csharp
// Get or add component
AudioSource audio = gameObject.GetOrAdd<AudioSource>();

// Layer management
bool inLayer = gameObject.IsInLayer(LayerMask.GetMask("Enemy"));
gameObject.SetLayer("Player");

// Child management
gameObject.ClearChildTransforms();
```

### TransformExtensions

```csharp
// Child management
transform.Clear(); // Destroy all children

// Orbital positioning
transform.SetOrbitRotation(target, horizontalAngle: 45f, verticalAngle: 30f, distance: 5f);

// 2D look-at
transform.LookAt2D(targetPosition);

// Distance utilities
float distance = transform.DistanceTo(otherTransform);
bool inRange = transform.InRangeOf(otherTransform, 5f);
```

### CameraExtensions

```csharp
// Culling mask management
bool isVisible = camera.IsLayerInCullingMask(LayerMask.GetMask("Enemy"));
camera.AddToCullingMask(LayerMask.GetMask("UI"));
camera.SetCullingMask(LayerMask.GetMask("Player", "Enemy"));

// FOV control
camera.ZoomIn(15f);
camera.SetFOV(60f);
```

### ColorExtensions

```csharp
// Component modification
Color newColor = originalColor.WithAlpha(0.5f);

// Color operations
Color brighter = color.MultiplyRGB(1.5f);
Color inverted = color.Invert();

// Hex conversion
string hex = color.ColorToHex(); // "#RRGGBBAA"
Color color = "#FF0000FF".HexToColor();
```

### Vector2Extensions

```csharp
// Component modification
Vector2 modified = vector.With(x: 5f, y: 10f);
Vector2 added = vector.Add(x: 2f);
Vector2 multiplied = vector.Multiply(x: 2f, y: 0.5f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector2 direction = fromPos.DirectionTo(toPos);
float distance = fromPos.DistanceTo(toPos);
Vector2 perpendicular = vector.Perpendicular();
Vector2 rotated = vector.Rotate(45f);

// Boundary checks
bool insideCircle = point.IsInsideCircle(center, radius);
bool insideRect = point.IsInsideRect(center, size);

// Random point in annulus
Vector2 randomPoint = origin.RandomPointInAnnulus(minRadius: 2f, maxRadius: 8f);
```

### Vector3Extensions

```csharp
// Component modification
Vector3 modified = vector.With(x: 5f, y: 10f, z: 15f);
Vector3 rotatedX = vector.RotateX(45f);
Vector3 rotatedY = vector.RotateY(90f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector3 direction = fromPos.DirectionTo(toPos);
Vector3 reflected = vector.Reflect(normal);
Vector3 projected = vector.ProjectOnto(onto);

// Boundary checks
bool insideSphere = point.IsInsideSphere(center, radius);
bool insideBox = point.IsInsideBox(center, size);
bool insideBounds = point.IsInsideBounds(bounds);

// Random point in 3D annulus
Vector3 randomPoint = origin.RandomPointInAnnulus(2f, 8f, Plane2D.XZ);
```

## C# Extensions

### StringExtensions

```csharp
// Number parsing
float value = "3,14".ParseFloatWithComma(); // 3.14f

// Percentage formatting
string percent = 0.25f.AsPercent(); // "25%"

// Large number formatting
string short = 1500000.ToShortFormat(1); // "1.5M"

// Enum conversion
MyEnum value = "EnumValue".ToEnum<MyEnum>();
```

### NumberExtensions

```csharp
// Number checks
bool isEven = 42.IsEven();
bool isOdd = 13.IsOdd();

// Percentage calculations
float percentage = current.PercentageOf(total);

// Range clamping
int clamped = value.AtLeast(10).AtMost(100);
float clampedFloat = value.AtLeast(0f).AtMost(1f);

// Probability/chance
bool success = 0.75f.RollChance(); // 75% chance
bool luckyRoll = 25.RollChance(0, 100); // 25% chance out of 100

// Enum conversion
MyEnum enumValue = 1.ToEnum<MyEnum>();
MyEnum safeEnum = 999.ToEnumOrDefault(MyEnum.Default);
```

### CollectionExtensions

```csharp
// Array operations
T randomItem = array.Rand();
int randomIndex = array.RandIndex();
T[] shuffled = array.Shuffle();
T[] swapped = array.Swap(0, 1);
bool isEmpty = array.IsNullOrEmpty();
string formatted = array.Format(); // "[item1, item2, item3]"
T first = array.First();
T last = array.Last();
T[] sliced = array.Slice(2, 5);
T[] multiple = array.RandMultiple(3);
T weighted = array.RandWeighted(item => item.weight);

// List operations
T randomItem = list.Rand();
List<T> shuffled = list.Shuffle();
string formatted = list.Format(); // "{item1, item2, item3}"

// Dictionary operations
V randomValue = dict.RandV();
K randomKey = dict.RandK();
Dictionary<K, V> copy = dict.AsNewCopy();
string formatted = dict.Format(); // "{key1: value1, key2: value2}"
```

### TimeExtensions

```csharp
// Clock formatting
string clock = 3661f.ToClock(); // "01:01:01"
string short = 125f.ToShortClock(); // "02:05"

// DateTime calculations
DateTime pastTime = DateTime.Now.AddHours(-2);
double hours = pastTime.HoursUntilNow(); // 2.0

// DateTime manipulation
DateTime newDate = original.WithDate(year: 2024);
DateTime newTime = original.WithTime(hour: 9, minute: 0);
```

## NekoLib Extensions

### TimerExtensions

```csharp
// Create timers directly from components
Countdown countdown = this.CreateCountdown(10f);
Stopwatch stopwatch = this.CreateStopwatch();

// Cleanup timers
this.CleanupTimers();
```

### SerializeExtensions

```csharp
// JSON serialization
string json = myObject.Serialize(prettyPrint: true);
MyObject restored = json.Deserialize<MyObject>();
```

### TaskExtensions

```csharp
// Fire-and-forget tasks
myAsyncTask.Forget();
myAsyncTask.Forget(ex => Debug.LogError($"Task failed: {ex}"));
```

### TextColorizeExtensions

```csharp
// Basic colorization
string colored = "Hello".Colorize(Color.red);
string hexColored = "World".Colorize("#FF0000");

// Selective colorization
string selective = "Hello World".Colorize(Color.red, "Hello");
string chars = "Hello!".Colorize(Color.blue, '!');
string multiple = "Red and Blue".Colorize(Color.red, "Red", "Blue");

// Conditional colorization
string conditional = "Error".Colorize(Color.red, () => hasError);
string predicate = "Some words".Colorize(Color.green, word => word.Length > 4);
```

### TextFormatExtensions

```csharp
// Bold formatting
string bold = "Important".Bold(); // "<b>Important</b>"
string selective = "This is important".Bold("important");

// Italic formatting
string italic = "Emphasis".Italic(); // "<i>Emphasis</i>"

// Size formatting
string sized = "Big Text".Size(24f); // "<size=24>Big Text</size>"

// Chaining
string formatted = "Important Warning"
    .Bold("Important")
    .Italic("Warning")
    .Size(18f, "Warning");
```

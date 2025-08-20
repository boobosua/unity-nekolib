# NekoLib

A comprehensive utility package for Unity game development inspired by various sources and my own experiences.

## Acknowledgments

Special thanks to [Adam Myhre](https://github.com/adammyhre) and [Code Monkey](https://www.youtube.com/@CodeMonkeyUnity) for inspiring the development of this library through his excellent Unity tutorials and architectural patterns.

Also grateful to GitHub Copilot for assistance with smaller tasks and code refinements throughout the development process.

## Table of Contents

- [Installation](#installation)
- [Features](#features)
- [Dependencies](#dependencies)
- [Usage Examples](#usage-examples)
  - [NetworkManager](#networkmanager)
  - [DateTimeManager](#datetimemanager)
  - [Time Extensions](#time-extensions)
  - [Singleton Patterns](#singleton-patterns)
  - [Timer System](#timer-system)
  - [Collection Extensions](#collection-extensions)
  - [GameObject Extensions](#gameobject-extensions)
  - [Number Extensions](#number-extensions)
  - [Text Extensions](#text-extensions)
  - [Vector Extensions](#vector-extensions)
  - [Transform Extensions](#transform-extensions)
  - [Enum Utils](#enum-utils)
  - [Color Palette](#color-palette)
  - [Utilities](#utilities)
- [Requirements](#requirements)
- [License](#license)
- [Changelog](#changelog)

## Installation

### Via Git URL

Add this package via Unity Package Manager:

```
https://github.com/boobosua/unity-nekolib.git
```

### Manual Installation

1. Download the package
2. Import into your Unity project
3. No external dependencies required

## Features

- **NetworkManager**: Internet connection monitoring with async/await support and cancellation tokens
- **DateTimeManager**: Server time synchronization from TimeAPI.io and Google
- **Time Extensions**: DateTime formatting, calculations, and manipulation with intuitive naming
- **Singleton Patterns**: LazySingleton, SceneSingleton, and PersistentSingleton implementations
- **Timer System**: Modern timer pool with fluent builder pattern and unscaled time support
- **Collection Extensions**: Array/List/Dictionary operations with shuffling, random selection, and formatting
- **GameObject Extensions**: Component management, layer checking, and child transform operations
- **Number Extensions**: Mathematical utilities, percentage calculations, and chance systems
- **Text Extensions**: Rich text formatting (bold, italic, underline) and colorization
- **Vector Extensions**: Enhanced Vector2/Vector3 operations with mathematical functions and annulus generation
- **Transform Extensions**: Child management, 2D rotation, orbital movement, and transform utilities
- **Enum Utils**: Random enum selection, filtering, counting, and iteration utilities
- **Color Palette**: Predefined color palette for consistent UI and debugging
- **Utilities**: Mouse/pointer detection, cached WaitForSeconds, and rotation utilities

## Usage Examples

### NetworkManager

```csharp
// Check internet connection
bool isConnected = await NetworkManager.Instance.CheckInternetConnectionAsync();

// Start monitoring with events
NetworkManager.Instance.OnInternetRefresh += (connected) =>
    Debug.Log($"Internet: {connected}");
NetworkManager.Instance.StartMonitoring();
```

### DateTimeManager

```csharp
// Sync from TimeAPI.io and Google
await DateTimeManager.Instance.FetchTimeFromServerAsync();

// Get synced time
DateTime utcNow = DateTimeManager.Instance.UtcNow();
DateTime localTime = DateTimeManager.Instance.Now();
```

### Time Extensions

```csharp
// Time formatting
float gameTime = 3665.5f;
gameTime.ToClock(); // "01:01:05"
TimeSpan.FromSeconds(7890).ToReadableFormat(); // "2h 11m 30s"

// Large number formatting
1500000.ToShortFormat(); // "1.5M"
2500.ToShortABCFormat(); // "2.5a"

// Date calculations (requires DateTimeManager sync)
DateTime pastTime = DateTime.Now.AddHours(-3);
pastTime.SecondsUntilNow(); // Seconds elapsed
pastTime.MinutesUntilNow(); // Minutes elapsed

DateTime futureTime = DateTime.Now.AddHours(2);
futureTime.SecondsFromNow(); // Seconds remaining

// Date manipulation
DateTime.Now.WithDate(month: 12, day: 25); // Change date components
DateTime.Now.WithTime(hour: 14, minute: 30); // Change time components
someDate.IsToday(); // Period checks
```

### Singleton Patterns

```csharp
public class GameManager : LazySingleton<GameManager> { }      // Auto-creates
public class UIManager : SceneSingleton<UIManager> { }        // Scene-bound
public class AudioManager : PersistentSingleton<AudioManager> { } // Survives scenes
```

### Timer System

```csharp
// Extension methods
var countdown = this.CreateCountdown(5f);
countdown.OnStop += () => Debug.Log("Done!");
countdown.Start();

// Fluent builder
TimerFactory.CreateCountdown(this)
    .SetDuration(10f)
    .SetUnscaledTime()
    .SetLoop(3)
    .Build();
```

### Timer Component

```csharp
// Unity component with UnityEvents
Timer timer = GetComponent<Timer>();
timer.SetWaitTime(5f);
timer.OnTimeOut.AddListener(() => Debug.Log("Timeout!"));
timer.StartTimer();
```

### Collection Extensions

```csharp
// Safe random operations (throws on null/empty)
int randomItem = myArray.Rand();
int randomIndex = myList.RandIndex();
var randomKey = myDict.RandK();
var randomValue = myDict.RandV();

// Array/List utilities
int[] shuffled = myArray.Shuffle();
int[] swapped = myArray.Swap(0, 4); // By index
string[] swappedByValue = myArray.Swap("a", "b"); // By element

// Debug formatting
Debug.Log(myList.Format()); // "{1, 2, 3}"
Debug.Log(myDict.Format()); // "{key1: 1, key2: 2}"
```

### GameObject Extensions

```csharp
// Component utilities
var rigidbody = gameObject.GetOrAdd<Rigidbody>();
myComponent.SetActive(); // gameObject.SetActive(true)
gameObject.IsInLayer(targetLayer);
gameObject.ClearChildTransforms();
```

### Number Extensions

```csharp
// Math utilities
42.IsEven(); // true
75.PercentageOf(100); // 0.75f
150.AtMost(100); // 100 (clamped)
20.AtLeast(50); // 50 (clamped)

// Probability (0.0-1.0 for float, 0-100 for int)
0.7f.RollChance(); // 70% success
25.RollChance(); // 25% success (out of 100)

// Enum conversion
5.ToEnum<GameState>(); // Convert int to enum
5.ToEnumOrDefault<GameState>(defaultValue); // Safe conversion
```

### Text Extensions

```csharp
// Rich text formatting
"Hello World".Bold().Italic().Colorize(Palette.VibrantRed);
"Make this bold".Bold("this"); // Target specific words
"Text".Colorize("#FF0000"); // Hex colors
"Status".Colorize(Color.green, () => isActive); // Conditional

// String parsing & conversion
"3,14".ParseFloatWithComma(); // European decimal format
"50".ToEnum<GameState>(); // String to enum
"invalid".ToEnumOrDefault<GameState>(GameState.Default); // Safe conversion

// Text processing
"HelloWorldTest".SplitCamelCase(); // "Hello World Test"
"remove spaces".WithoutSpaces(); // "removespaces"

// Number formatting extensions (on strings)
"25".AsPercent(); // "25%" (direct formatting)
"0.25".AsExactPercent(); // Treats as 25%
```

### Vector Extensions

```csharp
// Component modification
transform.position.With(y: 5f, z: 10f);
playerPos.Add(x: 2f).Multiply(y: 0.5f).Divide(z: 2f);

// Spatial utilities
playerPos.InRangeOf(targetPos, 5f); // Distance check
playerPos.DistanceTo(targetPos); // Get distance
playerPos.DirectionTo(targetPos); // Get direction

// Vector3 specific
someVector3.ToVector2(); // Drop Z component
someVector3.RotateY(45f); // Rotate around axis

// Geometric operations
vector2.Rotate(90f); // 2D rotation
vector2.Perpendicular(); // 90° counterclockwise
vector3.ProjectOnto(normal); // Vector projection

// Random points in annulus (ring shape)
origin.RandomPointInAnnulus(minRadius: 2f, maxRadius: 5f); // Vector2
origin.RandomPointInAnnulus(2f, 5f, Plane2D.XZ); // Vector3 on plane
```

### Transform Extensions

```csharp
// Child management
transform.Clear(); // Destroy all children
Transform[] children = transform.GetChildren(includeInactive: true);

// 2D rotation
transform.LookAt2D(targetPosition);
transform.LookAt2D(targetPosition, angleOffset: 45f);

// Orbital positioning (static)
transform.SetOrbitRotation(target, horizontal: 45f, vertical: 30f, distance: 10f);
transform.SetOrbitRotationClamped(target, 45f, 30f, 10f, minVertical: -60f, maxVertical: 80f);

// Orbital movement (continuous - call in Update)
float currentAngle = 0f;
transform.OrbitAround(target, Orientation.Horizontal, speed: 30f, staticVertical: 20f, distance: 8f, ref currentAngle);

// Distance & direction utilities
float distance = transform.DistanceTo(otherTransform);
Vector3 direction = transform.DirectionTo(otherTransform);
bool inRange = transform.InRangeOf(otherTransform, 5f);
```

### Enum Utils

```csharp
// Random enum selection
GameState randomState = EnumUtils.GetRandom<GameState>();
Direction direction = EnumUtils.GetRandomExcept(Direction.Up, Direction.Down);

// Enum utilities
int stateCount = EnumUtils.Count<GameState>();
EnumUtils.ForEach<PowerUpType>(powerUp => Debug.Log(powerUp));
```

### Color Palette

```csharp
// Predefined colors
Debug.Log("Success!".Colorize(Palette.MintEmerald));
Debug.LogError("Error!".Colorize(Palette.VibrantRed));
Color uiColor = Palette.AzureTeal;
```

### Utilities

```csharp
// Cached coroutines
yield return Utils.GetWaitForSeconds(1.5f);

// Object detection
bool over3D = Utils.IsPointerOverAny3DObject();
Utils.IsPointerOverAny3DObject(out RaycastHit hit);
bool over2D = Utils.IsPointerOverAny2DObject();

// Mouse utilities
Vector2 mouse2D = Utils.GetMousePosition2D();
Ray mouseRay = Utils.GetMouseRay();
float angle = Utils.GetAngleFromVector(direction);
Quaternion randomRot = Utils.GetRandomRotation(Axis.Y);
```

## Advanced Examples

### Orbital Camera System

```csharp
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 8f;
    [SerializeField] private float sensitivity = 2f;

    private float horizontalAngle = 0f;
    private float verticalAngle = 20f;

    void Update()
    {
        if (target == null) return;

        // Mouse input for manual control
        if (Input.GetMouseButton(1)) // Right click
        {
            horizontalAngle += Input.GetAxis("Mouse X") * sensitivity;
            verticalAngle -= Input.GetAxis("Mouse Y") * sensitivity;

            // Manual positioning with clamped vertical
            transform.SetOrbitRotationClamped(target, horizontalAngle, verticalAngle, distance);
        }
        else
        {
            // Auto orbit horizontally when not controlling
            transform.OrbitAround(target, Orientation.Horizontal, speed: 15f, verticalAngle, distance, ref horizontalAngle);
        }
    }
}
```

### Smart Collection Manager

```csharp
public class InventoryManager : MonoBehaviour
{
    private List<Item> items = new();
    private Dictionary<ItemType, List<Item>> itemsByType = new();

    public void AddRandomLoot()
    {
        // Safe random selection from weighted drops
        if (!lootTable.IsNullOrEmpty())
        {
            var randomLoot = lootTable.Rand();
            items.Add(randomLoot);
            Debug.Log($"Found: {items.Format()}");
        }
    }

    public void ShuffleInventory()
    {
        items = items.Shuffle(); // Non-destructive shuffle
        UpdateUI();
    }

    public Item GetRandomItemOfType(ItemType type)
    {
        return itemsByType.TryGetValue(type, out var typeItems) && !typeItems.IsNullOrEmpty()
            ? typeItems.Rand()
            : null;
    }
}
```

## Requirements

- Unity 2020.3 or later
- No external dependencies required

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

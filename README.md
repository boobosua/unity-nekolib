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
- **Color Palette**: Predefined color palette for consistent UI and debugging
- **Utilities**: Mouse/pointer detection, cached WaitForSeconds, and rotation utilities

## Usage Examples

### NetworkManager

```csharp
// Check internet connection with timeout and cancellation support
bool isConnected = await NetworkManager.Instance.CheckInternetConnectionAsync();

// Check with custom cancellation token
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
bool result = await NetworkManager.Instance.CheckInternetConnectionAsync(cts.Token);

// Start automatic monitoring (checks every 5 seconds)
NetworkManager.Instance.StartMonitoring(); // Fire and forget

// Start monitoring with cancellation token
var monitoringCts = new CancellationTokenSource();
NetworkManager.Instance.StartMonitoring(monitoringCts.Token);

// Stop monitoring manually
NetworkManager.Instance.StopMonitoring();

// Subscribe to connection status changes
NetworkManager.Instance.OnInternetRefresh += (isConnected) =>
{
    if (isConnected)
        Debug.Log("Internet connection restored!".Colorize(Palette.MintEmerald));
    else
        Debug.LogWarning("Internet connection lost!".Colorize(Palette.VibrantRed));
};

// Check current connection status (cached from last check)
bool currentStatus = NetworkManager.Instance.HasInternet;
```

### DateTimeManager

```csharp
// Sync server time from TimeAPI.io or Google
await DateTimeManager.Instance.FetchTimeFromServerAsync();

// Get current UTC time (synced)
DateTime utcNow = DateTimeManager.Instance.UtcNow();

// Get local time
DateTime localTime = DateTimeManager.Instance.Now();

// Get today's date
DateTime today = DateTimeManager.Instance.Today();
```

### Time Extensions

```csharp
// Time formatting
float gameTime = 3665.5f; // 1 hour, 1 minute, 5.5 seconds
Debug.Log($"Game time: {gameTime.ToClock()}"); // "01:01:05"

// Readable duration format
TimeSpan duration = TimeSpan.FromSeconds(7890);
Debug.Log($"Duration: {duration.ToReadableFormat()}"); // "2h 11m 30s"
Debug.Log($"Compact: {7890.5f.ToReadableFormat(false)}"); // "2h11m30s"

// Past time calculations (intuitive naming)
DateTime pastTime = DateTime.Now.AddMinutes(-45);
Debug.Log($"Minutes ago: {pastTime.MinutesAgo():F0}");
Debug.Log($"Hours ago: {pastTime.HoursAgo():F1}");

// Future time calculations
DateTime futureTime = DateTime.Now.AddHours(3);
Debug.Log($"Seconds from now: {futureTime.SecondsFromNow():F0}");
Debug.Log($"Hours from now: {futureTime.HoursFromNow():F1}");

// Date manipulation
DateTime christmas = DateTime.Now.WithDate(month: 12, day: 25);
DateTime meeting = DateTime.Now.WithTime(hour: 15, minute: 30);

// Period checks
bool isToday = someDate.IsToday();
bool isStartOfWeek = someDate.IsStartOfWeek(); // Monday 00:00:00
```

### Singleton Patterns

```csharp
// LazySingleton - auto-creates when accessed
public class GameManager : LazySingleton<GameManager>
{
    public void Initialize() { }
}

// SceneSingleton - exists only in current scene
public class UIManager : SceneSingleton<UIManager>
{
    public void ShowMenu() { }
}

// PersistentSingleton - survives scene changes
public class AudioManager : PersistentSingleton<AudioManager>
{
    public void PlayMusic() { }
}
```

### Timer System

```csharp
// Simple extension method timers
var countdown = this.CreateCountdown(5f);
countdown.OnStop += () => Debug.Log("Timer finished!");
countdown.Start();

var stopwatch = this.CreateStopwatch();
stopwatch.Start();

// Fluent builder pattern (advanced)
var advancedTimer = TimerFactory.CreateCountdown(this)
    .SetDuration(10f)
    .SetUnscaledTime()
    .SetLoop(3)
    .Build();

// Unscaled time (ignores Time.timeScale)
var uiTimer = this.CreateCountdown(2f).SetUnscaledTime();

// Method chaining
var loopTimer = this.CreateCountdown(1f)
    .SetLoop(-1)  // infinite loops
    .Start();
```

### Timer Component

```csharp
// Unity component-based timer with UnityEvents
Timer timer = GetComponent<Timer>();

// Configure timer properties
timer.SetWaitTime(5f);
timer.SetOneShot(true);
timer.SetIgnoreTimeScale(false);

// Add event listeners (Editor-compatible)
timer.AddTimeOutListener(() => Debug.Log("Timer finished!"));
timer.AddBeginListener(() => Debug.Log("Timer started!"));
timer.AddUpdateListener(() => Debug.Log($"Progress: {timer.Progress:P}"));

// Control timer
timer.StartTimer();
timer.Pause();
timer.Resume();
timer.Stop();
```

### Collection Extensions

```csharp
// Array/List operations
int randomItem = myArray.Rand();
int[] shuffled = myArray.Shuffle();
int[] swapped = myArray.Swap(0, 4);
string formatted = myArray.Format(); // "[1, 2, 3, 4, 5]"

// Dictionary operations
var randomValue = myDict.RandV();
var randomKey = myDict.RandK();
```

### GameObject Extensions

```csharp
// Component management
var rigidbody = gameObject.GetOrAdd<Rigidbody>();
myComponent.SetActive();    // gameObject.SetActive(true)

// Layer checking and child management
bool isInTargetLayer = gameObject.IsInLayer(targetLayer);
gameObject.ClearChildTransforms();
```

### Number Extensions

```csharp
// Math utilities
bool isEven = 42.IsEven();
float percent = 75.PercentageOf(100);  // 0.75f

// Range clamping and chance
int clamped = 150.AtMost(100);  // 100
bool success = 0.7f.HasChance();  // 70% chance
```

### Text Extensions

```csharp
// Basic formatting
string text = "Hello World".Bold().Italic().Colorize(Palette.VibrantRed);

// Target specific words/characters
string formatted = "Make this bold".Bold("this");
string colored = "Error message".Colorize(Palette.VibrantRed, "Error");

// Hex colors and predicates
string hexColored = "Text".Colorize("#FF0000");
string conditional = "Error".Bold(() => hasError);
```

### Vector Extensions

```csharp
// Component modification
Vector3 newPos = transform.position.With(y: 5f, z: 10f);
Vector2 adjusted = playerPos.Add(x: 2f).Multiply(y: 0.5f);

// Range checking and random points
bool inRange = playerPos.InRangeOf(targetPos, 5f);
Vector3 randomPoint = origin.RandomPointInAnnulus(2f, 10f, AnnulusPlane.XZ);
Vector2 vec2 = someVector3.ToVector2();
```

### Color Palette

```csharp
// Use predefined colors
Debug.Log("Success!".Colorize(Palette.MintEmerald));
Debug.LogError("Error!".Colorize(Palette.VibrantRed));

// Available color categories: Gray, Yellow, Red, Green, Blue, Purple, Orange
Color uiColor = Palette.AzureTeal;
```

### Utilities

```csharp
// Cached WaitForSeconds (no memory allocation)
yield return Utils.GetWaitForSeconds(1.5f);
yield return Utils.GetWaitForSecondsRealtime(2f);  // Unscaled time

// 3D Object Detection
bool over3D = Utils.IsPointerOverAny3DObject();
bool over3DWithDistance = Utils.IsPointerOverAny3DObject(10f, myLayerMask);
bool overSpecific = Utils.IsPointerOver3DObject(targetGameObject);
bool overComponent = Utils.IsPointerOver3DObject<Collider>();

// With raycast hit information
if (Utils.IsPointerOverAny3DObject(out RaycastHit hit))
{
    Debug.Log($"Hit: {hit.transform.name} at {hit.point}");
}

// 2D Object Detection (orthographic cameras)
bool over2D = Utils.IsPointerOverAny2DObject();
bool over2DSpecific = Utils.IsPointerOver2DObject(target2DObject);
bool over2DComponent = Utils.IsPointerOver2DObject<Collider2D>();

// With collision information
if (Utils.IsPointerOverAny2DObject(out Collider2D hit2D))
{
    Debug.Log($"Hit 2D: {hit2D.name}");
}

// UI Detection
bool overUI = Utils.IsPointerOverUI(uiLayerMask);

// Mouse/Pointer Position
bool inGameWindow = Utils.IsMouseInGameWindow();
Vector2 mouse2D = Utils.GetMousePosition2D();              // For 2D/orthographic
Vector3 mouse3D = Utils.GetMousePosition3D(distance: 10f); // At specific distance
Vector3 mouseFromRay = Utils.GetMousePosition3DFromRaycast(); // From collision

// Mouse Ray
Ray mouseRay = Utils.GetMouseRay();
Ray customCameraRay = Utils.GetMouseRay(myCamera);

// Rotation Utilities
float angle = Utils.GetAngleFromVector(direction);
Quaternion randomRot = Utils.GetRandomRotation(Axis.Y); // Y-axis only
Quaternion multiAxisRot = Utils.GetRandomRotation(
    Axis.XYZ,
    xRange: new Vector2(0, 90),
    yRange: new Vector2(0, 360),
    zRange: new Vector2(-45, 45)
);

#if UNITY_EDITOR
// Editor-only asset finding
ScriptableObject[] assets = Utils.FindAllAssets<ScriptableObject>("Assets/Data/");
AudioClip[] sounds = Utils.FindAllAssets<AudioClip>("Assets/Audio/");
#endif
```

## Requirements

- Unity 2020.3 or later
- No external dependencies required

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

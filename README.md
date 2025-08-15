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
// Formatting
float gameTime = 3665.5f;
gameTime.ToClock(); // "01:01:05"
TimeSpan.FromSeconds(7890).ToReadableFormat(); // "2h 11m 30s"

// Calculations
pastTime.MinutesAgo(); // Time difference
DateTime.Now.WithDate(month: 12, day: 25); // Date manipulation
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
// Random operations
int randomItem = myArray.Rand();
var randomKey = myDict.RandK();

// Utilities
int[] shuffled = myArray.Shuffle();
string formatted = myList.Format(); // "{1, 2, 3}"
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
150.AtMost(100); // 100
0.7f.HasChance(); // 70% chance
```

### Text Extensions

```csharp
// Rich text formatting
"Hello World".Bold().Italic().Colorize(Palette.VibrantRed);
"Make this bold".Bold("this"); // Target specific words
"Text".Colorize("#FF0000"); // Hex colors
```

### Vector Extensions

```csharp
// Component modification
transform.position.With(y: 5f, z: 10f);
playerPos.Add(x: 2f).Multiply(y: 0.5f);

// Utilities
playerPos.InRangeOf(targetPos, 5f);
someVector3.ToVector2();
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
Quaternion randomRot = Utils.GetRandomRotation(Axis.Y);
```

## Requirements

- Unity 2020.3 or later
- No external dependencies required

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

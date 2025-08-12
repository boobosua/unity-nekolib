# NekoLib

A comprehensive utility package for Unity game development inspired by various sources and my own experiences.

## Acknowledgments

Special thanks to [Adam Myhre](https://github.com/adammyhre) for inspiring the development of this library through his excellent Unity tutorials and architectural patterns.

Also grateful to GitHub Copilot for assistance with smaller tasks and code refinements throughout the development process.

## Table of Contents

- [Installation](#installation)
- [Features](#features)
- [Dependencies](#dependencies)
- [Usage Examples](#usage-examples)
  - [NetworkManager](#networkmanager)
  - [DateTimeManager](#datetimemanager)
  - [Singleton Patterns](#singleton-patterns)
  - [Timer Components](#timer-components)
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

- **NetworkManager**: Internet connection monitoring with async/await support, cancellation tokens, and automatic duplicate monitoring prevention
- **DateTimeManager**: Server time synchronization from TimeAPI.io and Google
- **Singleton Patterns**: LazySingleton, SceneSingleton, and PersistentSingleton implementations
- **Timer System**: Modern timer pool with automatic lifecycle management, fluent builder pattern, and unscaled time support
- **Text Extensions**: Rich text formatting (bold, italic, underline) and colorization
- **Vector Extensions**: Enhanced Vector2/Vector3 operations and coordinate conversions
- **Color Palette**: Predefined color palette for consistent UI and debugging
- **Utilities**: Mouse/pointer detection, cached WaitForSeconds, and common helper functions

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

### Text Extensions

```csharp
// Rich text formatting
string text = "Hello World".Bold().Italic().Colorize(Palette.VibrantRed);

// Selective formatting
string formatted = "Make this bold".Bold("this");

// Colorize specific words
string colored = "Error message".Colorize(Palette.VibrantRed, "Error");
```

### Vector Extensions

```csharp
// Modify specific components
Vector3 newPos = transform.position.With(y: 5f);

// Coordinate conversions
Vector3 screenPos = worldPosition.WorldToScreen();
Vector3 worldPos = screenPosition.ScreenToWorld();

// Utility operations
bool inRange = playerPos.InRangeOf(targetPos, 5f);
Vector3 randomPoint = origin.RandomPointInAnnulus(2f, 10f);
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

// Mouse/pointer detection
bool overUI = Utils.IsPointerOverUIElement(uiLayer);
bool over3D = Utils.IsPointerOverAny3DObject();
bool over2D = Utils.IsPointerOverAny2DObject();

// Mouse world position (2D)
Vector2 mouseWorld = Utils.MouseWorldPosition;
```

## Requirements

- Unity 2020.3 or later
- No external dependencies required

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

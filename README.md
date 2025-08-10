# NekoLib

A comprehensive utility package for Unity game development with built-in UniTask support for modern async/await patterns.

## Table of Contents

- [Installation](#installation)
- [Features](#features)
- [Dependencies](#dependencies)
- [Usage Examples](#usage-examples)
  - [NetworkManager](#networkmanager)
  - [DateTime Utilities](#datetime-utilities)
  - [Singleton Pattern](#singleton-pattern)
  - [Extensions](#extensions)
- [Requirements](#requirements)
- [License](#license)
- [Changelog](#changelog)

## Installation

### Via Git URL

Add this package via Unity Package Manager:

```
https://github.com/boobosua/unity-nekolib.git?path=Assets/NekoLib
```

### Manual Installation

1. Download the package
2. Import into your Unity project
3. UniTask will be automatically installed as a dependency

## Features

- **Singleton Pattern**: Easy-to-use singleton implementations
- **NetworkManager**: Internet connection monitoring with async/await support
- **Extensions**: Useful extension methods for Unity types
- **Color Palette**: Predefined color palettes for debugging and UI
- **DateTime Utilities**: Date and time manipulation tools
- **Utilities**: Various utility functions for common tasks
- **UniTask Integration**: Built-in async/await support for better performance

## Dependencies

This package automatically installs and includes:

- **UniTask**: For async/await patterns and better performance than coroutines

## Usage Examples

### NetworkManager

```csharp
// Check internet connection
bool isConnected = await NetworkManager.Instance.CheckInternetConnectionAsync();

// Start monitoring
NetworkManager.Instance.StartMonitoringAsync().Forget();

// Subscribe to status changes
NetworkManager.Instance.OnInternetRefresh += (connected) =>
    Debug.Log($"Internet: {connected}");
```

### DateTime Utilities

```csharp
// Get current timestamp
long timestamp = DateTimeUtility.GetCurrentTimestamp();

// Convert to DateTime
DateTime dateTime = DateTimeUtility.TimestampToDateTime(timestamp);

// Format time
string formatted = DateTimeUtility.FormatTime(DateTime.Now, "HH:mm:ss");

// Time difference
TimeSpan diff = DateTimeUtility.GetTimeDifference(startTime, endTime);
```

### Singleton Pattern

```csharp
public class GameManager : Singleton<GameManager>
{
    public void Initialize()
    {
        // Your initialization code
    }
}

// Usage
GameManager.Instance.Initialize();
```

### Extensions

```csharp
// GameObject extensions
gameObject.SetActiveOptimized(true);

// Vector3 extensions
Vector3 position = transform.position.WithY(5f);

// Color extensions
Color randomColor = ColorPalette.GetRandomColor();
```

## Requirements

- Unity 2020.3 or later
- UniTask (automatically installed)

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

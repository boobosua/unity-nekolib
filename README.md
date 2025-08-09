# NekoLib

A comprehensive utility package for Unity game development with built-in UniTask support for modern async/await patterns.

## Features

- **Singleton Pattern**: Easy-to-use singleton implementations
- **NetworkManager**: Internet connection monitoring with async/await support
  - 10-second timeout to prevent hanging requests
  - Automatic cancellation token handling
  - Event-driven status updates
  - Built-in monitoring with customizable intervals
- **Extensions**: Useful extension methods for Unity types
- **Color Palette**: Predefined color palettes for debugging and UI
- **Utilities**: Various utility functions for common tasks
- **UniTask Integration**: Built-in async/await support for better performance

## Dependencies

This package automatically installs and includes:

- **UniTask**: For async/await patterns and better performance than coroutines

## Usage Examples

### NetworkManager with Async/Await

```csharp
// Simple internet connection check (uses automatic cancellation token)
bool isConnected = await NetworkManager.Instance.CheckInternetConnectionAsync();

// Check current internet status
bool currentStatus = NetworkManager.Instance.HasInternet;

// Start automatic monitoring (checks every 5 seconds)
NetworkManager.Instance.StartMonitoringAsync().Forget();

// Start monitoring with custom cancellation token
using var cts = new CancellationTokenSource();
NetworkManager.Instance.StartMonitoringAsync(cts.Token).Forget();

// Stop monitoring
NetworkManager.Instance.StopMonitoring();
```

### Advanced Usage with Cancellation

```csharp
// Manual cancellation after 30 seconds
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
bool connected = await NetworkManager.Instance.CheckInternetConnectionAsync(cts.Token);

// Use GameObject's lifetime for automatic cancellation
bool connected = await NetworkManager.Instance.CheckInternetConnectionAsync(this.destroyCancellationToken);
```

### Event Handling

```csharp
// Subscribe to internet status changes
NetworkManager.Instance.OnInternetRefresh += (isConnected) => {
    if (isConnected)
    {
        Debug.Log("Internet connection restored!");
    }
    else
    {
        Debug.Log("Internet connection lost!");
    }
};

// Unsubscribe (important for preventing memory leaks)
NetworkManager.Instance.OnInternetRefresh -= YourCallbackMethod;
```

### Best Practices

```csharp
public class GameManager : MonoBehaviour
{
    private async void Start()
    {
        // Check internet on startup
        bool hasInternet = await NetworkManager.Instance.CheckInternetConnectionAsync();

        if (hasInternet)
        {
            // Start monitoring - automatically cancels when GameManager is destroyed
            NetworkManager.Instance.StartMonitoringAsync(this.destroyCancellationToken).Forget();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Stop monitoring when app is paused to save battery
            NetworkManager.Instance.StopMonitoring();
        }
    }
}
```

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

## Requirements

- Unity 2020.3 or later
- UniTask (automatically installed)

## License

See [LICENSE.md](LICENSE.md) for licensing information.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

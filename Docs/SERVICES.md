# SERVICES

Singleton services for networking and time management.

## DateTime Services

### DateTimeManager

Server-synchronized time service with fallback to system time.

```csharp
// Sync with server
await DateTimeManager.Instance.FetchTimeFromServerAsync();

// Get current time (server-synced)
DateTime utcNow = DateTimeManager.Instance.UtcNow();
DateTime localNow = DateTimeManager.Instance.Now();

// Time period checks
bool isStartOfWeek = DateTimeManager.Instance.IsTodayStartOfWeek();
bool isStartOfMonth = DateTimeManager.Instance.IsTodayStartOfMonth();
```

### TimeSystem

Static wrapper for DateTimeManager.

```csharp
// Static access to time methods
DateTime utcNow = TimeSystem.UtcNow();
DateTime localNow = TimeSystem.Now();
bool isStartOfWeek = TimeSystem.IsTodayStartOfWeek();
```

## Networking Services

### NetworkManager

Internet connection monitoring and detection.

```csharp
// Check internet connection
bool hasInternet = await NetworkManager.Instance.CheckInternetConnectionAsync();

// Start/stop monitoring
NetworkManager.Instance.StartMonitoring();
NetworkManager.Instance.StopMonitoring();

// Listen for connection changes
NetworkManager.AddInternetRefreshListener(isConnected => {
    Debug.Log($"Internet: {isConnected}");
});
```

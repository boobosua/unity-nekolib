# NekoLib Services

Singleton services for networking and time management.

## DateTime Services

### DateTimeService

Server-synchronized time service with fallback to system time.

```csharp
// Sync with server
await DateTimeService.FetchTimeFromServerAsync();

// Get current time (server-synced)
DateTime utcNow = DateTimeService.UtcNow;
DateTime localNow = DateTimeService.Now;

// Time period checks
bool isStartOfWeek = DateTimeService.IsTodayStartOfWeek;
bool isStartOfMonth = DateTimeService.IsTodayStartOfMonth;
```

#### Usage Example

```csharp
async void Start()
{
    try
    {
        await DateTimeService.FetchTimeFromServerAsync();
        Debug.Log($"Server time synced: {DateTimeService.Now}");
    }
    catch (OperationCanceledException)
    {
        Debug.Log("Time sync cancelled");
    }
}

void CheckDailyRewards()
{
    var lastLogin = DateTime.Parse(PlayerPrefs.GetString("LastLogin"));
    var now = DateTimeService.Now;

    if (now.Date > lastLogin.Date)
    {
        GiveDailyReward();
        PlayerPrefs.SetString("LastLogin", now.ToString());
    }
}
```

## Networking Services

### NetworkService

Centralized network management service with static API for easy access.

```csharp
// Check internet connection once
bool hasInternet = await NetworkService.FetchInternetConnectionAsync();

// Start monitoring internet connection
NetworkService.StartMonitoring();

// Listen for internet connection changes
NetworkService.OnConnectionUpdate += OnConnectionChanged;

// Stop monitoring
NetworkService.StopMonitoring();

// Clean up
NetworkService.Dispose();
```

#### Usage Example

```csharp
async void Start()
{
    // Check internet connection once
    bool isOnline = await NetworkService.FetchInternetConnectionAsync();
    Debug.Log($"Internet connection: {isOnline}");

    // Start monitoring internet connection
    NetworkService.StartMonitoring();

    // Listen for internet connection changes
    NetworkService.OnConnectionUpdate += OnConnectionUpdate;
}

private void OnConnectionUpdate(bool isOnline)
{
    if (isOnline)
    {
        Debug.Log("Internet connection restored!");
        // Handle reconnection logic here
    }
    else
    {
        Debug.Log("Internet connection lost!");
        // Handle disconnection logic here
    }
}

void OnDestroy()
{
    // Clean up
    NetworkService.Dispose();
}
```

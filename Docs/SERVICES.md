# NekoLib Services

Singleton services for networking and time management.

## DateTime Services

### TimeService

Server-synchronized time service.

If you define `NEKO_TIME_SERVICE_DEBUG` in Scripting Define Symbols, `TimeService.Now`/`UtcNow` will behave like `DateTime.Now`/`DateTime.UtcNow` for local debugging.

Without `NEKO_TIME_SERVICE_DEBUG`, `TimeService.Now`/`UtcNow` fall back to `DateTime.Now`/`DateTime.UtcNow` until a successful sync.

#### NEKO_TIME_SERVICE_DEBUG

Enable it via Unity:

- **Edit → Project Settings → Player → Other Settings → Scripting Define Symbols**
- Add `NEKO_TIME_SERVICE_DEBUG` for the target platform.

When enabled:

- `FetchTimeFromServerAsync`/`FetchTimeFromServerCoroutine` return success without performing any web requests.
- `TimeService.UtcNow` returns `DateTime.UtcNow` and `TimeService.Now` returns `DateTime.Now`.

Time drift tracking uses `Time.realtimeSinceStartupAsDouble` when available (Unity 2020.2+ / Unity 6+) for improved precision.

Time sync attempts multiple HTTP "Date" header endpoints before falling back to a time API.

```csharp
// Sync with server (async)
bool synced = await TimeService.FetchTimeFromServerAsync();

// Sync with server (coroutine)
StartCoroutine(TimeService.FetchTimeFromServerCoroutine(ok => Debug.Log($"Synced: {ok}")));

// Get current time (server-synced)
DateTime utcNow = TimeService.UtcNow;
DateTime localNow = TimeService.Now;

// Check if TimeService has successfully synced
bool hasSynced = TimeService.HasSynced;

// Time period checks
bool isStartOfWeek = TimeService.IsTodayStartOfWeek;
bool isStartOfMonth = TimeService.IsTodayStartOfMonth;
```

#### Usage Example

```csharp
async void Start()
{
    bool synced = await TimeService.FetchTimeFromServerAsync();
    Debug.Log($"Server time synced: {synced} ({TimeService.Now})");
}

void CheckDailyRewards()
{
    var lastLogin = DateTime.Parse(PlayerPrefs.GetString("LastLogin"));
    var now = TimeService.Now;

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

Internet checks try multiple endpoints to avoid relying on a single domain that may be blocked in some regions.

```csharp
// Check internet connection once
ConnectionStatus status = await NetworkService.FetchInternetConnectionAsync();

// Current state (updated after checks / monitoring)
ConnectionStatus current = NetworkService.Status;
bool isOnline = NetworkService.IsOnline;

// Coroutine version
StartCoroutine(NetworkService.FetchInternetConnectionCoroutine(s => Debug.Log($"Status: {s}")));

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
    ConnectionStatus status = await NetworkService.FetchInternetConnectionAsync();
    Debug.Log($"Internet connection: {status}");

    // Start monitoring internet connection
    NetworkService.StartMonitoring();

    // Listen for internet connection changes
    NetworkService.OnConnectionUpdate += OnConnectionUpdate;
}

private void OnConnectionUpdate(ConnectionStatus status)
{
    if (status == ConnectionStatus.Online)
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

# NekoLib Services

Singleton services for networking and time management.

```csharp
using NekoLib.Services;
```

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

// Date-only helpers
DateTime todayLocal = TimeService.Today;         // local date, time stripped
DateTime todayUtc = TimeService.TodayUtc;        // UTC date, time stripped
DateTime nextDay = TimeService.NextDay;           // local next midnight
DateTime nextDayUtc = TimeService.NextDayUtc;    // UTC next midnight

// Check if TimeService has successfully synced
bool hasSynced = TimeService.HasSynced;

// Time period checks (local)
bool isStartOfWeek = TimeService.IsTodayStartOfWeek;     // Monday
bool isStartOfMonth = TimeService.IsTodayStartOfMonth;   // 1st of month

// Time period checks (UTC)
bool isStartOfWeekUtc = TimeService.IsTodayStartOfWeekUtc;
bool isStartOfMonthUtc = TimeService.IsTodayStartOfMonthUtc;
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

Background internet connectivity monitoring with a static API.

Uses `Application.internetReachability` as a fast offline path, then confirms real internet access via HTTP HEAD against multiple endpoints. `OnConnectionChanged` fires only when status actually changes.

```csharp
// Start monitoring
NetworkService.StartMonitoring();

// Listen for connectivity changes (true = online, false = offline)
NetworkService.OnConnectionChanged += HandleNetworkChange;

// Current state
bool isOnline = NetworkService.IsOnline;

// Stop monitoring
NetworkService.StopMonitoring();

// Clean up all listeners
NetworkService.Dispose();
```

#### Usage Example

```csharp
void OnEnable()
{
    NetworkService.OnConnectionChanged += HandleNetworkChange;
    NetworkService.StartMonitoring();
}

void OnDisable()
{
    NetworkService.OnConnectionChanged -= HandleNetworkChange;
    NetworkService.StopMonitoring();
}

private void HandleNetworkChange(bool isOnline)
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
    NetworkService.Dispose();
}
```

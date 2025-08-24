# Services

## Table of Contents

- [NetworkManager](#networkmanager)
- [DateTimeManager](#datetimemanager)

---

## NetworkManager

Monitors internet connectivity with async/await support.

```csharp
private async void Start()
{
    // Check connection once
    bool isConnected = await NetworkManager.Instance.CheckInternetConnectionAsync();

    // Subscribe to connection changes
    NetworkManager.Instance.OnInternetRefresh += OnConnectionChanged;

    // Start continuous monitoring
    NetworkManager.Instance.StartMonitoring();
}

private void OnConnectionChanged(bool isConnected)
{
    Debug.Log($"Internet connection: {isConnected}");
}
```

---

## DateTimeManager

Synchronizes time with external servers to prevent client-side manipulation.

```csharp
private async void Start()
{
    // Fetch server time
    await DateTimeManager.Instance.FetchTimeFromServerAsync();

    // Get synchronized times
    DateTime utcTime = DateTimeManager.Instance.UtcNow();
    DateTime localTime = DateTimeManager.Instance.Now();

    Debug.Log($"Server UTC: {utcTime}");
}
```

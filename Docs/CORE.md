# NekoLib Core

Core foundational systems: singletons, timers, and color swatches.

### PersistentSingleton

Survives scene changes. Perfect for game managers.

```csharp
public class GameManager : PersistentSingleton<GameManager>
{
    public int Score { get; set; }
}

// Usage
GameManager.Instance.Score += 100;
```

### LazySingleton

Auto-created on first access. Perfect for utilities.

```csharp
public class AudioManager : LazySingleton<AudioManager>
{
    public void PlayMusic(AudioClip clip) { /* implementation */ }
}

// Usage - GameObject created automatically
AudioManager.Instance.PlayMusic(backgroundMusic);
```

### Creating Timers

Use the fluent builders exposed via `Utils` in `NekoLib.Utilities`.

```csharp
using NekoLib.Utilities; // namespace for Utils

// Countdown timer (loops 3 times, only updates when not paused)
var countdown = Utils.CreateCountdown(this)
    .SetDuration(10f)
    .SetLoop(3)
    .SetUpdateWhen(() => !isPaused)
    .Build();

// Stopwatch timer (auto stops when gameIsOver && only updates in active state)
var stopwatch = Utils.CreateStopwatch(this)
    .SetStopCondition(() => gameIsOver)
    .SetUpdateWhen(() => isActiveState)
    .Build();
```

### Timer Control

```csharp
// Start and handle events
countdown.OnStop += () => Debug.Log("Time's up!");
countdown.Start();

// Monitor progress
float progress = countdown.Progress; // 0.0 to 1.0
string timeLeft = countdown.InverseClockFormat; // "MM:SS"

// Control timers
countdown.Pause();
countdown.Resume();
countdown.Stop();

// Conditional updates - timer only ticks when condition is true
timer.SetUpdateWhen(() => player.IsAlive && !game.IsPaused);
```

### Invoke Helpers

Convenience extension methods on `MonoBehaviour` (namespace `NekoLib.Extensions`) that schedule actions via the PlayerLoop timer system — no coroutines required.

```csharp
using NekoLib.Extensions;

// Invoke once after a delay (scaled or unscaled time)
this.InvokeAfterDelay(2f, () => Debug.Log("Fired after 2s"));
this.InvokeAfterDelay(2f, () => Debug.Log("Unscaled after 2s"), useUnscaledTime: true);

// Repeated invoke every interval (returns IDisposable -> Dispose to stop)
IDisposable handle = this.InvokeEvery(1f, () => Debug.Log("Tick each second"));
handle.Dispose(); // stops

// Invoke on whole-second ticks for a total duration
this.InvokeEverySeconds(
    intervalSeconds: 1,
    durationSeconds: 10,
    onTick: sec => Debug.Log($"Tick {sec}s"),
    onStop: () => Debug.Log("Finished 10s")
);

// Stopwatch convenience via extensions
var sw = this.GetStopwatch();          // create
sw.OnUpdate += t => Debug.Log($"Elapsed: {t:F2}s");
sw.Start();
// ... later
float elapsed = sw.StopAndGetTime();   // returns seconds
sw.Dispose();                          // return to pool if pooling enabled
```

Notes:

- All helpers use the PlayerLoop-driven `TimerPlayerLoopDriver` (no MonoBehaviour `Update`).
- Timer pooling is enabled by default.
- Increase the retention cap if needed: `Utils.SetTimerMaxPoolSize(1024);` (namespace `NekoLib.Utilities`).
- `TimerFactory` and `TimerConfig` were removed; migrate to `Utils.CreateCountdown(...)` / `Utils.CreateStopwatch(...)`.
- Dispose timers you no longer need to minimize the Active list and leverage pooling.

### Color Swatch

Pre-defined color constants for consistent theming.

```csharp
// Available colors
Color darkGray = Swatch.DG;
Color vibrantRed = Swatch.VR;

// Usage in debug messages
Debug.Log("Success!".Colorize(Swatch.DE));
Debug.LogError("Error!".Colorize(Swatch.VR));

// UI theming
button.color = Swatch.VC;
errorText.color = Swatch.VR;
```

### Log

Simple conditional logger. Methods are compiled only in the Editor, Development builds, or when `NEKOLIB_LOG` is defined.

```csharp
using NekoLib.Logger;

// Basic usage
Log.Info("Started");
Log.Warn("Potential issue");
Log.Error("Something went wrong");

// With context to ping an object in the Console
Log.Info("Found object", someGameObject);
```

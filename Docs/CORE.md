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

```csharp
// Countdown timer
var countdown = TimerFactory.CreateCountdown(this)
    .SetDuration(10f)
    .SetLoop(3)
    .SetUpdateWhen(() => !isPaused) // Conditional updates
    .Build();

// Stopwatch timer
var stopwatch = TimerFactory.CreateStopwatch(this)
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

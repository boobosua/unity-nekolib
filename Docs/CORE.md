# NekoLib Core

Core foundational systems: singletons, timers, and color swatches.

## Singleton Patterns

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

### SceneSingleton

Recreated each scene. Perfect for UI managers.

```csharp
public class UIManager : SceneSingleton<UIManager>
{
    public void ShowPauseMenu() => pauseMenu.SetActive(true);
}

// Usage
UIManager.Instance.ShowPauseMenu();
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

## Timer System

### Creating Timers

```csharp
// Countdown timer
var countdown = TimerFactory.CreateCountdown(this)
    .SetDuration(10f)
    .SetLoop(3)
    .Build();

// Stopwatch timer
var stopwatch = TimerFactory.CreateStopwatch(this)
    .SetStopCondition(() => gameIsOver)
    .Build();
```

### Using Timers

```csharp
// Start and handle events
countdown.OnStop += () => Debug.Log("Time's up!");
countdown.Start();

// Monitor progress
float progress = countdown.Progress; // 0.0 to 1.0
string timeLeft = countdown.InverseClockFormat; // "MM:SS"
```

## Color Swatch

Pre-defined color constants for consistent theming.

```csharp
// Available colors
Color darkGray = Swatch.DG;
Color vibrantRed = Swatch.VR;

// Usage in debug messages
Debug.Log("Success!".Colorize(Swatch.DE));
button.color = Swatch.VC;
```

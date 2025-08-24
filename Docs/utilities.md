# Utilities

## Table of Contents

- [Singletons](#singletons)
- [Timer System](#timer-system)
- [Enum Utils](#enum-utils)
- [Mouse Utils](#mouse-utils)
- [Time Utils](#time-utils)
- [Swatch](#swatch)
- [YieldTask](#yieldtask)

---

## Singletons

### LazySingleton

Automatically creates a singleton GameObject when first accessed.

```csharp
public class GameManager : LazySingleton<GameManager>
{
    public int score;
}

// Usage anywhere in code
GameManager.Instance.score = 100;
```

### SceneSingleton

Singleton destroyed when scene unloads. Must exist in scene.

```csharp
public class UIManager : SceneSingleton<UIManager>
{
    public void ShowMenu() { }
}

// Usage
UIManager.Instance.ShowMenu();
```

### PersistentSingleton

Singleton that persists across all scenes.

```csharp
public class AudioManager : PersistentSingleton<AudioManager>
{
    public void PlaySound(AudioClip clip) { }
}

// Usage
AudioManager.Instance.PlaySound(soundEffect);
```

---

## Timer System

### TimerFactory.CreateCountdown

Fluent API for creating countdown timers.

```csharp
var countdown = TimerFactory.CreateCountdown(this)
    .SetDuration(10f)
    .SetUnscaledTime()
    .SetLoop(3)
    .Build();

countdown.Start();
```

### TimerFactory.CreateStopwatch

Fluent API for creating stopwatch timers.

```csharp
var stopwatch = TimerFactory.CreateStopwatch(this)
    .SetUnscaledTime()
    .Build();

stopwatch.Start();
```

### CreateCountdown (Extension)

Direct countdown creation from MonoBehaviour.

```csharp
var countdown = this.CreateCountdown(5f);
countdown.Start();
```

### CreateStopwatch (Extension)

Direct stopwatch creation from MonoBehaviour.

```csharp
var stopwatch = this.CreateStopwatch();
stopwatch.Start();
```

---

## Enum Utils

### GetRandomEnum

Get random enum value.

```csharp
public enum GameState { Menu, Playing, Paused }
var randomState = Utils.GetRandomEnum<GameState>();
```

### GetRandomEnum (with exclusions)

Get random enum excluding specific values.

```csharp
var state = Utils.GetRandomEnum<GameState>(GameState.Menu);
```

### CountEnum

Count total enum values.

```csharp
int count = Utils.CountEnum<GameState>();
```

### AllEnum

Get all enum values excluding specific ones.

```csharp
var allStates = Utils.AllEnum<GameState>(GameState.Menu);
```

### ForEnum

Iterate through all enum values.

```csharp
Utils.ForEnum<GameState>(state => Debug.Log(state));
```

---

## Mouse Utils

### IsMouseInGameWindow

Check if mouse cursor is within game window bounds.

```csharp
if (Utils.IsMouseInGameWindow())
{
    // Handle mouse input
}
```

### GetMousePosition2D

Get mouse world position for 2D games (orthographic cameras).

```csharp
Vector2 mousePos = Utils.GetMousePosition2D();
```

### GetMousePosition3D

Get mouse world position in 3D at specified distance from camera.

```csharp
Vector3 mousePos = Utils.GetMousePosition3D(10f);
```

### GetMousePosition3DFromRaycast

Get 3D mouse position from raycast hit.

```csharp
Vector3 hitPoint = Utils.GetMousePosition3DFromRaycast();
```

### GetMouseRay

Get mouse ray for raycasting.

```csharp
Ray mouseRay = Utils.GetMouseRay();
```

---

## Time Utils

### GetWaitForSeconds

Get cached WaitForSeconds to avoid memory allocation.

```csharp
yield return Utils.GetWaitForSeconds(1.5f);
```

### GetWaitForSecondsRealtime

Get cached WaitForSecondsRealtime (unaffected by Time.timeScale).

```csharp
yield return Utils.GetWaitForSecondsRealtime(2f);
```

---

## Swatch

Predefined color palette for consistent styling.

```csharp
// Use predefined colors
Color redColor = Swatch.CR;    // Candy Red
Color greenColor = Swatch.ME;  // Mint Emerald
Color blueColor = Swatch.AT;   // Azure Teal
Color yellowColor = Swatch.GA; // Golden Amber

// Text colorization in logs
Debug.Log("Success!".Colorize(Swatch.ME));
```

---

## YieldTask

Bridge between async/await Tasks and Unity coroutines.

```csharp
public class NetworkHandler : MonoBehaviour
{
    private async Task<string> FetchDataAsync()
    {
        await Task.Delay(2000);
        return "Data received";
    }

    private IEnumerator HandleNetworkRequest()
    {
        var task = FetchDataAsync();
        yield return new YieldTask(task);
        Debug.Log($"Result: {task.Result}");
    }

    private void Start()
    {
        StartCoroutine(HandleNetworkRequest());
    }
}
```

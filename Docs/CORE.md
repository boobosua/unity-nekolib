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

NekoLib timers are PlayerLoop-driven (no coroutines) and come in two flavors:

- `Countdown`: counts down from a duration to 0.
- `Stopwatch`: counts up until you stop it (or a stop condition triggers).

Both are lightweight `readonly struct` handles in `NekoLib.Timer`.

```csharp
using NekoLib.Timer;
using UnityEngine;

// Countdown — loops 3 times, only ticks when not paused
var countdown = Countdown.Create(this, 10f)
    .SetLoop(3)
    .OnUpdateWhen(() => !isPaused)
    .OnUpdate(remaining => Debug.Log($"Remaining: {remaining:F2}s"))
    .OnStop(() => Debug.Log("Countdown finished"));

countdown.Start();

// Stopwatch — stops when a condition becomes true
var stopwatch = Stopwatch.Create(this)
    .SetStopWhen(() => gameIsOver)
    .OnUpdateWhen(() => isActiveState)
    .OnUpdate(elapsed => Debug.Log($"Elapsed: {elapsed:F2}s"))
    .OnStop(() => Debug.Log("Stopwatch stopped"));

stopwatch.Start();
```

### Timer Control

```csharp
// State
bool alive = countdown.IsAlive;
bool running = countdown.IsRunning;
bool paused = countdown.IsPaused;

// Countdown values
float remaining = countdown.RemainingTime;
float total = countdown.TotalTime;
int loopIteration = countdown.CurrentLoopIteration;

// Control
countdown.Pause();
countdown.Resume();
countdown.Stop();   // invokes OnStop callbacks
countdown.Cancel(); // silent (does NOT invoke OnStop callbacks)

// Conditional updates — timer only ticks when condition is true
countdown.OnUpdateWhen(() => player.IsAlive && !game.IsPaused);
```

### Invoke Helpers

Convenience extension methods on `MonoBehaviour` (namespace `NekoLib.Timer`) that schedule actions via the PlayerLoop timer system — no coroutines required.

```csharp
using NekoLib.Timer;

// Invoke once after a delay; returns a token to cancel before it fires
TimerToken token = this.CallAfter(2f, () => Debug.Log("Fired after 2s"));
TimerToken unscaled = this.CallAfter(2f, () => Debug.Log("Unscaled after 2s"), useUnscaledTime: true);
token.Cancel(); // cancels before it fires — silent, no callbacks

// Repeat every interval; returns a token to stop the loop
TimerToken ticker = this.CallEvery(1f, () => Debug.Log("Tick each second"));
TimerToken unscaledTicker = this.CallEvery(1f, () => Debug.Log("Tick"), useUnscaledTime: true);

ticker.Cancel(); // stops the loop without invoking stop callbacks
```

Notes:

- All helpers use the PlayerLoop-driven driver (no MonoBehaviour `Update`).
- Active timers can be inspected via `Window > Neko Framework > Timer Tracker`.

### Pooling

NekoLib provides a small, deterministic prefab pooling helper built on Unity's `UnityEngine.Pool.ObjectPool<T>`.

- `IPoolable` gives you lifecycle hooks: `OnSpawned()` and `OnDespawned()`.
- `PrefabPool<T>` manages instances of a prefab `T : MonoBehaviour, IPoolable`.
- Use `Spawn(...)` / `Despawn(...)` (older `Get()` / `Release()` naming was removed).

Lifecycle ordering (important when writing your hooks):

- `OnSpawned()` is invoked as soon as the instance is fetched from the underlying pool.
- After that, `Spawn(...)` applies any requested parenting/transform changes and activates the GameObject.
- `OnDespawned()` is invoked before the pool reparents the instance under the pool root and deactivates it.

```csharp
using NekoLib.Pooling;
using UnityEngine;

public sealed class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawned()
    {
        // reset state, enable trails, etc.
    }

    public void OnDespawned()
    {
        // stop VFX, clear velocity, etc.
    }
}

public sealed class BulletSpawner : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform poolRoot;

    private PrefabPool<Bullet> _pool;

    private void Awake()
    {
        _pool = new PrefabPool<Bullet>(
            prefab: bulletPrefab,
            poolRoot: poolRoot,
            defaultCapacity: 32,
            maxSize: 256,
            collectionCheck: true
        );

        _pool.Prewarm(32);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        var bullet = _pool.Spawn(position, rotation);
        // ... use bullet
    }

    public void Despawn(Bullet bullet)
    {
        _pool.Despawn(bullet);
    }
}
```

If you want pooled objects to be able to return themselves without holding a pool reference, inherit from `PoolableBehaviour`:

```csharp
using NekoLib.Pooling;
using UnityEngine;

public sealed class EnemyProjectile : PoolableBehaviour
{
    public override void OnSpawned()
    {
        // reset state
    }

    public override void OnDespawned()
    {
        // cleanup state
    }

    private void OnCollisionEnter(Collision _)
    {
        // If created by PrefabPool, returns to pool; otherwise Destroy(gameObject).
        ReleaseSelf();
    }
}
```

Other useful APIs on `PrefabPool<T>`:

- `Spawn()` / `Spawn(Transform parent)` / `Spawn(Vector3, Quaternion)` / `Spawn(Vector3, Quaternion, Transform parent)`
- `Despawn(T instance, float delaySeconds)`
- `Clear()` (destroys inactive pooled instances)
- `CountInactive` (how many instances are currently pooled)

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

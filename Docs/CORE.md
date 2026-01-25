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

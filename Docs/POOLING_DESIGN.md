# NekoLib Pooling System — Design Document

A record of the design process, decisions, trade-offs, and rationale for the pooling system rewrite.

---

## Problem Statement

Unity's `ObjectPool<T>` requires writing the same five callbacks for every type you pool:

```csharp
// Written identically for every new pooled type — all boilerplate
new ObjectPool<Bullet>(
    () => Instantiate(prefab, root),
    obj => obj.gameObject.SetActive(true),
    obj => { obj.gameObject.SetActive(false); obj.transform.SetParent(root); },
    obj => Destroy(obj.gameObject),
    collectionCheck: true,
    defaultCapacity: 16,
    maxSize: 256
);
```

With 50+ pooled types this becomes significant repeated code. The goal was a system where the user writes **zero pool setup boilerplate per type** while retaining the same performance — or better.

A secondary problem: pooled objects (bullets, enemies, projectiles) need to **release themselves** from inside physics callbacks (`OnTriggerEnter`, `OnCollisionEnter`) without holding an explicit pool reference in every component. The previous `PoolableObject` attempted to solve this but bundled timer-based delayed release into the base class, making it complicated.

---

## Design Goals

1. **Zero callbacks per type.** `new Pool<Bullet>(prefab)` is all setup that ever exists.
2. **Self-release.** Any poolable object can call `Despawn()` on itself — from `OnTriggerEnter`, from a timer callback, from anywhere — without knowing its pool type.
3. **Data injection without allocations.** RPG games drive object parameters (lifetime, damage, mask) from spreadsheets at runtime. Data must be injectable at spawn time with zero closure allocation.
4. **Machine-gun performance.** The hot path (Spawn + Despawn) must have zero heap allocations in steady state. No `GetComponent`, no dictionary lookups, no per-call closures.
5. **Simplicity for 20+ types.** Each new pooled type should only contain its unique logic. No repeated infrastructure.
6. **Composition over inheritance for common behaviours.** Lifetime expiry, collision despawn, trigger despawn, and particle-stop despawn should all be drag-on components — zero code required.

---

## Why Not Use Unity's `ObjectPool<T>`

Unity's `ObjectPool<T>` is internally a `Stack<T>` plus an optional `HashSet<T>` for `collectionCheck`, plus delegate fields for the four callbacks.

When `actionOnGet` and `actionOnRelease` are nulled out (which is required to fix the lifecycle ordering problem described below), the remaining cost per cycle is:

- `Get()`: `HashSet.Remove` + `Stack.Pop`
- `Release()`: `HashSet.Contains` + `HashSet.Add` + `Stack.Push`

Two `HashSet` operations per spawn/despawn whose only purpose is detecting double-release — a bug already prevented by the `_despawning` guard on `PoolableObject`. The abstraction pays for something unused.

**Decision: use a raw `Stack<T>` directly.** The full implementation is ~50 lines. We get:
- No `HashSet` overhead on the hot path
- `GetLiveInstance()` null-skip loop that handles externally-destroyed instances cleanly
- `Prewarm` that instantiates directly into the stack (no `Get`/`Release` dance)
- Complete readability — no "why are these callbacks null?" confusion

The only regression is losing `collectionCheck`'s loud exception on double-release in editor. This is covered by the `_despawning` guard (silent no-op instead of exception) and can be augmented with a `#if UNITY_EDITOR` `HashSet` if a project needs the diagnostic.

---

## Lifecycle Ordering Problem

Unity's `ObjectPool<T>` triggers `actionOnGet` synchronously inside `Get()` — before the caller can call `SetActive(true)`. If `OnSpawned` is wired through `actionOnGet`, the object is inactive when the callback fires:

- Coroutines cannot be started
- `OnEnable` has not run
- Physics components don't respond
- Particle systems won't emit

**Fix:** call `OnSpawned` explicitly in `SpawnCore` **after** `SetActive(true)`. Call `OnDespawned` **before** `SetActive(false)`. This requires not using `actionOnGet`/`actionOnRelease` at all — which reinforces the decision to drop `ObjectPool<T>`.

**Correct lifecycle order:**

```
Spawn:   Get from stack → SetParent → [init callback] → SetActive(true) → OnEnable → OnSpawned()
Despawn: OnDespawned() → SetActive(false) → OnDisable → SetParent(root) → Push to stack
```

---

## Architecture

### Types

```
IPoolable         (public interface)    — OnSpawned / OnDespawned contract. Opt-in.
IPool             (internal interface)  — Type-erased back-reference from PoolableObject to Pool<T>.
PoolableObject    (public abstract)     — Base class granting Despawn() self-release. Opt-in.
Pool<T>           (public sealed)       — The pool. One per prefab type. Owns a Stack<T>.
```

Plus four drag-on components that eliminate code for common despawn patterns:

```
DespawnAfterLifetime        — timer-based auto-despawn. Supports runtime lifetime override.
DespawnOnCollision          — despawns on first physics collision.
DespawnOnTrigger            — despawns on trigger enter, filtered by LayerMask.
DespawnWhenParticleStopped  — despawns when ParticleSystem fires OnParticleSystemStopped.
```

### Why two opt-in paths?

**`IPoolable` only** — for objects that are always despawned externally (VFX, particles driven by a spawner manager). No inheritance required.

**`PoolableObject`** — for objects that need self-release (`bullet.Despawn()` from `OnTriggerEnter`). Inherits from `MonoBehaviour` (or `SerializedMonoBehaviour` under Odin Inspector). Implements `IPoolable`.

The pool works with any `Component` type. If T implements `IPoolable`, lifecycle callbacks fire. If T extends `PoolableObject`, the back-reference is wired and self-release works. If T is a plain component (no interface), the pool just manages `SetActive`.

---

## Key Design Decisions

### 1. `_despawning` guard on `PoolableObject`

**Problem:** `DespawnAfterLifetime` (timer) and `DespawnOnCollision` can both fire in the same frame, triggering two `Despawn()` calls on the same instance. Without a guard, the second call pushes an already-inactive instance onto the stack, corrupting the pool.

**Solution:** A `bool _despawning` flag. The first `Despawn()` call sets it and proceeds. Subsequent calls return early. `Pool<T>.PrepareSpawn()` resets it before each activation. This is cheaper than a `HashSet`, covers release builds, and eliminates the need for `collectionCheck`.

```csharp
public void Despawn()
{
    if (_despawning) return;
    _despawning = true;
    if (_pool != null && _pool.IsValid) _pool.Despawn(this);
    else Destroy(gameObject);
}
```

### 2. Type erasure via `IPool`

`PoolableObject.Despawn()` needs to call back to `Pool<T>` without knowing T. `IPool` (internal) provides a `void Despawn(Component)` method. `Pool<T>` implements it with a cast `(T)instance` — safe because `IPool` is internal and `SetPool(this)` is only called from `Pool<T>.CreateInstance()` on instances it created.

### 3. Data injection via `Spawn<TState>`

For spreadsheet-driven data (lifetime, damage, mask, speed), data must be set before `SetActive(true)` fires — because `OnEnable` components (like `DespawnAfterLifetime`) read it on enable.

```csharp
// Zero allocation — static lambda, generic state, no closure
_pool.Spawn(bulletData, static (b, d) => b.Initialize(d), pos, rot);
```

The `Action<T, TState>` overload passes state explicitly, so the lambda captures nothing from the outer scope. The compiler can verify this at compile time with the `static` keyword (C# 9+, Unity 2021.2+).

### 4. Delayed despawn stays out of the framework

The previous `PoolableObject` included `ReleaseSelf(float delay)`, `_releaseToken`, `CancelPendingRelease`, and `IPoolReleaser` to coordinate two despawn entry points. This was the primary source of complexity.

The insight: delayed despawn is three lines in the user's class. The token lives next to the logic it guards. Both cancellation paths (timer fires first, or manual despawn fires first) are safe without any pool coordination.

```csharp
public override void OnSpawned()
    => _lifetimeToken = this.CallAfter(_lifetime, this, static b => b.Despawn());

public override void OnDespawned()
    => _lifetimeToken.Cancel();  // safe no-op if timer already fired
```

`DespawnAfterLifetime` provides this pattern as a drag-on component for objects that don't need a custom script at all.

### 5. Pool root management

The pool root (parent Transform for inactive instances) is optional. If the caller passes `null`, the pool auto-creates a `HideAndDontSave` GameObject with `DontDestroyOnLoad` and owns its lifetime — it is destroyed when the pool is `Dispose()`d.

If a root is provided, the pool does not own it (caller is responsible).

### 6. All `Spawn` overloads unparent from pool root

Every Spawn variant detaches the instance from the pool root. An instance that remains parented to the pool root is surprising: it follows the root around the scene, and world position reads unexpectedly. The pool root is an implementation detail and must not be visible to consumers of the spawned object.

Newly created instances and recycled instances both start with `localPosition = zero`, `localRotation = identity` relative to the pool root, so world position after `SetParent(null)` is always deterministic.

### 7. `Prewarm` bypasses lifecycle

`Prewarm` instantiates directly into the stack. `OnSpawned` and `OnDespawned` do not fire. The previous design used `Get()` + `Release()` in a loop, which required an `_isPrewarming` flag to suppress the callbacks and caused `CountActive` to fluctuate momentarily. Instantiating directly is cleaner.

---

## Performance Characteristics

| Operation | Cost | Dominant factor |
|---|---|---|
| `Spawn()` | ~1–2 µs | `SetActive(true)` (Unity native call) |
| `Despawn()` | ~1–2 µs | `SetActive(false)` (Unity native call) |
| `_despawning` bool check | ~0 ns | Single bool read |
| `s_isPoolable` static bool | ~0 ns | Evaluated once at type-init |
| Interface cast + virtual call | ~5 ns | Standard IL cost |
| Stack push/pop | ~2 ns | Direct array index |
| `Spawn<TState>` init callback | 0 alloc | No closure, generic state |
| Steady-state allocations | **Zero** | After Prewarm |

`SetActive` is the unavoidable bottleneck — it drives `OnEnable`/`OnDisable` across all components on the GameObject. This cost is identical in any pooling system. At 1000 spawns/sec the pool overhead is approximately 2 ms/sec total, dominated entirely by `SetActive`.

---

## Composition Pattern for Common Behaviours

Instead of encoding common despawn patterns in base class methods, they are drag-on components. Each prefab configuration becomes a designer decision, not a code change.

| Component | What it does | Required setup |
|---|---|---|
| `DespawnAfterLifetime` | Timer-based auto-despawn | Serialised `_lifetime` field; for data-driven override, use `Spawn<TState>` + `SetLifetime()` |
| `DespawnOnCollision` | Despawns on first physics collision | Rigidbody required on GameObject |
| `DespawnOnTrigger` | Despawns on trigger enter, filtered by `LayerMask` | Collider set to trigger |
| `DespawnWhenParticleStopped` | Despawns when particle system finishes | `ParticleSystem.stopAction = Callback` |

A VFX prefab with `PoolableVfx` + `DespawnWhenParticleStopped` needs **zero custom code**.

---

## Validated Edge Cases

| Scenario | Behaviour |
|---|---|
| Double `Despawn()` same frame | `_despawning` guard: second call is silent no-op |
| External `Destroy(pooledInstance)` | `GetLiveInstance()` loop skips null Unity objects, creates fresh |
| `Despawn()` called after pool `Dispose()` | `!IsValid` check routes to `Object.Destroy` |
| `Spawn()` called after pool `Dispose()` | `ObjectDisposedException` thrown |
| `PoolableObject` not managed by a pool | `_pool == null` → `Despawn()` falls back to `Destroy(gameObject)` |
| Scene unload destroys pool root | `IsValid` returns false; active instances route to `Destroy` on Despawn |
| Auto-root pool across scene loads | Root is `DontDestroyOnLoad`; survives scene transitions |
| `OnDespawned` re-triggers `Despawn` | `_despawning` flag prevents recursion |
| Two pools, same prefab | Each pool wires `SetPool(this)` independently; no cross-contamination |
| `Prewarm` when already at target | `needed = Max(0, target - stack.Count)` → no-op |
| `Prewarm` beyond `maxSize` | Capped at `Min(count, maxSize)` before loop |
| Data-driven lifetime from spreadsheet | `Spawn<TState>` init fires before `SetActive`; `DespawnAfterLifetime.OnEnable` reads correct value |

---

## Final API Surface

```csharp
// Pool<T> — one per prefab type
public sealed class Pool<T> : IDisposable where T : Component
{
    public Pool(T prefab, int capacity = 16, int maxSize = 256, Transform root = null);

    public int  CountInactive { get; }
    public int  CountActive   { get; }
    public bool IsValid       { get; }

    // Spawn overloads
    public T Spawn();
    public T Spawn(Transform parent);
    public T Spawn(Vector3 position, Quaternion rotation);
    public T Spawn(Vector3 position, Quaternion rotation, Transform parent);

    // Data-injection overloads (zero allocation — static lambda + generic state)
    public T Spawn<TState>(TState state, Action<T, TState> init, Vector3 position, Quaternion rotation);
    public T Spawn<TState>(TState state, Action<T, TState> init, Vector3 position, Quaternion rotation, Transform parent);

    public void Despawn(T instance);
    public void Prewarm(int count);
    public void Clear();
    public void Dispose();
}

// PoolableObject — base class for self-releasing objects
public abstract class PoolableObject : MonoBehaviour, IPoolable
{
    public void Despawn();          // self-release; safe to call multiple times
    public abstract void OnSpawned();
    public abstract void OnDespawned();
}

// IPoolable — opt-in lifecycle hooks (no self-release)
public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}
```

---

## Usage Patterns

### Minimum viable — no interface, plain component

```csharp
// Pool handles SetActive only. OnEnable/OnDisable drive everything.
public class PooledParticle : MonoBehaviour
{
    private ParticleSystem _ps;
    private void Awake()     => _ps = GetComponent<ParticleSystem>();
    private void OnEnable()  => _ps.Play();
    private void OnDisable() => _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
}

var pool = new Pool<PooledParticle>(prefab);
var fx   = pool.Spawn(pos, Quaternion.identity);
pool.Despawn(fx);
```

### Self-releasing bullet with lifetime

```csharp
public class Bullet : PoolableObject
{
    [SerializeField] private float _speed    = 40f;
    [SerializeField] private float _lifetime = 3f;
    private Rigidbody  _rb;
    private TimerToken _lifetimeToken;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Fire(Vector3 direction) => _rb.linearVelocity = direction * _speed;

    public override void OnSpawned()
        => _lifetimeToken = this.CallAfter(_lifetime, this, static b => b.Despawn());

    public override void OnDespawned()
    {
        _lifetimeToken.Cancel();
        _rb.linearVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider _) => Despawn();
}

// Spawner
private Pool<Bullet> _pool;
private void Awake() => _pool = new Pool<Bullet>(_prefab, capacity: 128);
private void Start() => _pool.Prewarm(64);
private void OnDestroy() => _pool.Dispose();

public void Fire(Vector3 pos, Vector3 dir)
{
    var b = _pool.Spawn(pos, Quaternion.LookRotation(dir));
    b.Fire(dir);
}
```

### RPG — data-driven from spreadsheet, zero allocation

```csharp
[Serializable]
public struct BulletData
{
    public float     Lifetime;
    public float     Speed;
    public float     Damage;
    public LayerMask HitMask;
}

public class RpgBullet : PoolableObject
{
    private BulletData _data;
    private Rigidbody  _rb;
    private TimerToken _lifetimeToken;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    // Called before SetActive — data ready when OnSpawned fires
    public void Initialize(BulletData data)
    {
        _data = data;
        _rb.linearVelocity = transform.forward * data.Speed;
    }

    public override void OnSpawned()
        => _lifetimeToken = this.CallAfter(_data.Lifetime, this, static b => b.Despawn());

    public override void OnDespawned()
    {
        _lifetimeToken.Cancel();
        _rb.linearVelocity = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_data.HitMask.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.TryGetComponent<IDamageable>(out var t)) t.TakeDamage(_data.Damage);
        Despawn();
    }
}

// Fire — zero allocation regardless of rate
_pool.Spawn(_bulletData, static (b, d) => b.Initialize(d), pos, Quaternion.LookRotation(dir));
```

### Zero-code VFX prefab

Prefab components (inspector only, no custom script):
- `PoolableVfx` (empty PoolableObject subclass)
- `DespawnWhenParticleStopped`
- `ParticleSystem` with Stop Action = Callback

```csharp
// The only code needed anywhere
public sealed class PoolableVfx : PoolableObject
{
    public override void OnSpawned()   { }
    public override void OnDespawned() { }
}
```

---

## Files

### Modified / replaced
| File | Action |
|---|---|
| `Runtime/Core/Pooling/IPoolable.cs` | Updated docs |
| `Runtime/Core/Pooling/Pool.cs` | Full rewrite — raw `Stack<T>`, no `ObjectPool<T>` |
| `Runtime/Core/Pooling/PoolableObject.cs` | Rewrite — minimal self-release only |

### Added
| File | Purpose |
|---|---|
| `Runtime/Core/Pooling/IPool.cs` | Internal type-erased back-reference interface |
| `Runtime/Core/Pooling/Components/DespawnAfterLifetime.cs` | Timer-based auto-despawn component |
| `Runtime/Core/Pooling/Components/DespawnOnCollision.cs` | Collision-based despawn component |
| `Runtime/Core/Pooling/Components/DespawnOnTrigger.cs` | Trigger-based despawn with layer filter |
| `Runtime/Core/Pooling/Components/DespawnWhenParticleStopped.cs` | VFX auto-despawn component |

### Deleted
| File | Reason |
|---|---|
| `Runtime/Core/Pooling/IPoolReleaser.cs` | Replaced by `IPool.cs` |

---

## What Was Deliberately Left Out

| Feature | Reason excluded |
|---|---|
| `Despawn(instance, float delay)` on `Pool<T>` | Delays belong in the object via `CallAfter` + stored `TimerToken`. Putting it in the pool forced complex coordination between two cancel paths (`ReleaseSelf(delay)` and `Pool.Despawn(delay)`). |
| Static pool registry (`NekoPool.Spawn(prefab)`) | Global state with hidden lifetime. Fine for low-frequency spawning; bad for hot paths. Users who want it can build a thin facade over `Pool<T>`. |
| `Despawn()` returning a `TimerToken` | Would require `Pool<T>` to own and track per-instance timers. User's component already tracks it in one field. |
| Thread safety | Unity's game loop is single-threaded. Paying for locks on a hot path with no threading benefit is waste. |
| Pool manager / registry singleton | Imposes architecture on the user. `Pool<T>` as a plain field gives the same convenience with explicit ownership. |
| `ObjectPool<T>` wrapper | Using it with null callbacks pays HashSet overhead for double-release detection we already prevent with `_despawning`. Rolling `Stack<T>` directly is 50 lines with full control. |

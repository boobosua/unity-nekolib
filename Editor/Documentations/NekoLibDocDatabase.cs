#if UNITY_EDITOR
using System.Collections.Generic;

namespace NekoLib
{
    public static class NekoLibDocDatabase
    {
        public static List<NekoLibDocEntry> GetEntries()
        {
            return new List<NekoLibDocEntry>
            {
                new NekoLibDocEntry
                {
                    Title = "PersistentSingleton<T>",
                    Namespace = "NekoLib.Singleton",
                    Summary = "A MonoBehaviour singleton that survives scene changes via DontDestroyOnLoad.",
                    Description = "Inherit from PersistentSingleton<T> to create a manager that lives for the entire game session. Only one instance is ever allowed; duplicates are automatically destroyed.",
                    Code =
@"public class GameManager : PersistentSingleton<GameManager>
{
    public int Score { get; set; }
}

// Usage
GameManager.Instance.Score += 100;",
                    Tags = new[] { "Singleton", "Lifecycle", "Scene" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "Instance",
                            Summary = "Returns the single active instance, or null if none exists yet.",
                            Code =
@"if (GameManager.Instance != null)
    GameManager.Instance.Score += 100;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "HasInstance",
                            Summary = "True when a live instance exists. Safe to poll without creating one.",
                            Code =
@"if (PersistentSingleton<GameManager>.HasInstance)
    GameManager.Instance.Score += 10;"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "LazySingleton<T>",
                    Namespace = "NekoLib.Singleton",
                    Summary = "Auto-created singleton. A new GameObject is spawned on first access if none exists.",
                    Description = "Ideal for utility managers (audio, pooling, etc.) that do not need to be pre-placed in any scene. The instance is created lazily and destroyed when the application quits. Safe to use with Enter Play Mode / Disable Domain Reload in the Editor.",
                    Code =
@"public class AudioManager : LazySingleton<AudioManager>
{
    public void PlayMusic(AudioClip clip) { /* ... */ }
}

// No scene setup needed — created on first access
AudioManager.Instance.PlayMusic(backgroundMusic);",
                    Tags = new[] { "Singleton", "Lazy", "Auto-create" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "Instance",
                            Summary = "Returns (or lazily creates) the singleton instance. Creates a new GameObject if none exists.",
                            Code =
@"// Accessing Instance for the first time creates the GameObject automatically
AudioManager.Instance.PlayMusic(clip);"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Countdown",
                    Namespace = "NekoLib.Timer",
                    Summary = "PlayerLoop-driven countdown timer handle. No coroutines. Supports loops, callbacks, and conditional ticking.",
                    Description = "Countdown is a lightweight readonly struct handle. Create with Countdown.Create(owner, duration), chain builder methods, then call .Start(). Ticks via Unity's PlayerLoop — no MonoBehaviour Update overhead.\n\nKey state: IsAlive, IsRunning, IsPaused, RemainingTime, TotalTime, CurrentLoopIteration.\nKey controls: Start(), Stop(), Cancel() (silent — no OnStop), Pause(), Resume(), AddTime(), ReduceTime().",
                    Code =
@"var countdown = Countdown.Create(this, 10f)
    .SetLoop(3)
    .OnStart(() => Debug.Log(""Started!""))
    .OnUpdateWhen(() => !isPaused)
    .OnUpdate(t => Debug.Log($""Remaining: {t:F2}s""))
    .OnLoop(() => Debug.Log(""Loop!""))
    .OnStop(() => Debug.Log(""Finished!""));

countdown.Start();

// Control
countdown.Pause();
countdown.Resume();
countdown.Cancel();      // silent teardown — no OnStop fired
countdown.AddTime(5f);
countdown.ReduceTime(2f);

bool  alive = countdown.IsAlive;
float left  = countdown.RemainingTime;",
                    Tags = new[] { "Timer", "PlayerLoop", "Countdown" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsAlive",
                            Summary = "True while the timer is registered and ticking (not yet stopped or cancelled).",
                            Code =
@"var cd = Countdown.Create(this, 5f).Start();
if (cd.IsAlive)
    Debug.Log(""Still running"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsRunning",
                            Summary = "True when alive and not paused.",
                            Code =
@"if (cd.IsRunning)
    UpdateUI(cd.RemainingTime);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsPaused",
                            Summary = "True when the timer is alive but paused.",
                            Code =
@"pauseIcon.SetActive(cd.IsPaused);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "RemainingTime",
                            Summary = "Seconds left in the current loop iteration.",
                            Code =
@"timerLabel.text = cd.RemainingTime.ToClock();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "TotalTime",
                            Summary = "The total duration this countdown was created with.",
                            Code =
@"float pct = 1f - (cd.RemainingTime / cd.TotalTime);
progressBar.fillAmount = pct;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "CurrentLoopIteration",
                            Summary = "Zero-based count of completed loop iterations.",
                            Code =
@"Debug.Log($""Loop {cd.CurrentLoopIteration + 1} of {loopCount}"");"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Create(MonoBehaviour owner, float duration)",
                            Summary = "Creates a new countdown attached to a MonoBehaviour owner.",
                            Code =
@"var cd = Countdown.Create(this, 10f)
    .OnStop(() => OnTimerDone())
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Start()",
                            Summary = "Starts (or restarts) the countdown from TotalTime.",
                            Code =
@"var cd = Countdown.Create(this, 5f).OnStop(OnDone);
cd.Start();
// restart later
cd.Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Stop()",
                            Summary = "Stops the timer and fires the OnStop callback.",
                            Code =
@"cd.Stop(); // OnStop fires"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Cancel()",
                            Summary = "Stops the timer silently — no OnStop callback is fired.",
                            Code =
@"cd.Cancel(); // silent teardown — no callbacks"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Pause()",
                            Summary = "Freezes ticking. RemainingTime holds its value.",
                            Code =
@"cd.Pause();
pausePanel.SetActive(true);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Resume()",
                            Summary = "Resumes ticking from where it was paused.",
                            Code =
@"cd.Resume();
pausePanel.SetActive(false);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "AddTime(float seconds)",
                            Summary = "Adds seconds to the remaining time (e.g. bonus time power-up).",
                            Code =
@"// Power-up: +5s bonus
cd.AddTime(5f);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "ReduceTime(float seconds)",
                            Summary = "Subtracts seconds from the remaining time. Clamps to zero.",
                            Code =
@"// Penalty: -3s
cd.ReduceTime(3f);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "SetLoop(int count)",
                            Summary = "Sets how many times the countdown repeats. Use -1 for infinite.",
                            Code =
@"// Repeat 3 times, then fire OnStop
var cd = Countdown.Create(this, 2f)
    .SetLoop(3)
    .OnLoop(() => SpawnWave())
    .OnStop(() => GameOver())
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "SetUseUnscaledTime(bool value)",
                            Summary = "When true the timer ticks using unscaled time (ignores Time.timeScale).",
                            Code =
@"// Pause menu timer — unaffected by slow-motion
var cd = Countdown.Create(this, 30f)
    .SetUseUnscaledTime(true)
    .OnStop(OnTimeout)
    .Start();"
                        },
                        // Callbacks
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnStart(Action)",
                            Summary = "Fires once when Start() is called.",
                            Code =
@"Countdown.Create(this, 5f)
    .OnStart(() => sfx.PlayOneShot(countdownBeep))
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnUpdate(Action<float>)",
                            Summary = "Fires every tick with the current remaining time in seconds.",
                            Code =
@"Countdown.Create(this, 10f)
    .OnUpdate(t => timerLabel.text = t.ToClock())
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnUpdateWhen(Func<bool>)",
                            Summary = "Conditionally gate ticking. The timer only ticks while the predicate returns true.",
                            Code =
@"Countdown.Create(this, 30f)
    .OnUpdateWhen(() => !isPaused && isInRound)
    .OnStop(OnRoundEnd)
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnLoop(Action)",
                            Summary = "Fires at the end of each loop iteration (before restarting).",
                            Code =
@"Countdown.Create(this, 5f)
    .SetLoop(-1)
    .OnLoop(() => SpawnEnemy())
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnStop(Action)",
                            Summary = "Fires when the last loop ends or Stop() is called. Not fired on Cancel().",
                            Code =
@"Countdown.Create(this, 10f)
    .OnStop(() => ShowTimeUpScreen())
    .Start();"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Stopwatch",
                    Namespace = "NekoLib.Timer",
                    Summary = "Counts elapsed time upward. Stops when a predicate becomes true or you call Stop()/Cancel().",
                    Description = "Mirrors Countdown's builder API but measures elapsed time rather than remaining time. Use .SetStopWhen(predicate) to auto-stop on a condition.",
                    Code =
@"var stopwatch = Stopwatch.Create(this)
    .SetStopWhen(() => gameIsOver)
    .OnUpdateWhen(() => isActiveState)
    .OnUpdate(elapsed => Debug.Log($""Elapsed: {elapsed:F2}s""))
    .OnStop(() => Debug.Log(""Stopped!""));

stopwatch.Start();

float elapsed = stopwatch.ElapsedTime;",
                    Tags = new[] { "Timer", "Stopwatch", "Elapsed" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "ElapsedTime",
                            Summary = "Seconds elapsed since Start() was called.",
                            Code =
@"timerLabel.text = stopwatch.ElapsedTime.ToClock();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsAlive",
                            Summary = "True while the stopwatch is registered and has not been stopped or cancelled.",
                            Code =
@"if (stopwatch.IsAlive)
    UpdateElapsedDisplay();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsRunning",
                            Summary = "True when alive and not paused.",
                            Code =
@"stopButton.interactable = stopwatch.IsRunning;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsPaused",
                            Summary = "True when the stopwatch is alive but paused.",
                            Code =
@"pauseIcon.SetActive(stopwatch.IsPaused);"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Create(MonoBehaviour owner)",
                            Summary = "Creates a new stopwatch attached to a MonoBehaviour owner.",
                            Code =
@"var sw = Stopwatch.Create(this)
    .OnUpdate(t => label.text = t.ToClock())
    .OnStop(OnRoundEnd)
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Start()",
                            Summary = "Starts (or restarts) the stopwatch from zero.",
                            Code =
@"stopwatch.Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Stop()",
                            Summary = "Stops the stopwatch and fires OnStop.",
                            Code =
@"stopwatch.Stop();
Debug.Log($""Final time: {stopwatch.ElapsedTime:F2}s"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Cancel()",
                            Summary = "Stops silently — no OnStop callback is fired.",
                            Code =
@"stopwatch.Cancel();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Pause() / Resume()",
                            Summary = "Freezes or resumes elapsed time accumulation.",
                            Code =
@"stopwatch.Pause();
// ... show pause menu ...
stopwatch.Resume();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "SetStopWhen(Func<bool>)",
                            Summary = "Auto-stops the stopwatch when the predicate returns true.",
                            Code =
@"Stopwatch.Create(this)
    .SetStopWhen(() => playerDead)
    .OnStop(ShowDeathScreen)
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "SetUseUnscaledTime(bool value)",
                            Summary = "When true the stopwatch ticks using unscaled time (ignores Time.timeScale).",
                            Code =
@"Stopwatch.Create(this)
    .SetUseUnscaledTime(true)
    .OnUpdate(t => realTimeLabel.text = t.ToClock())
    .Start();"
                        },
                        // Callbacks
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnStart(Action)",
                            Summary = "Fires once when Start() is called.",
                            Code =
@"Stopwatch.Create(this)
    .OnStart(() => startPanel.SetActive(false))
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnUpdate(Action<float>)",
                            Summary = "Fires every tick with the current elapsed time in seconds.",
                            Code =
@"Stopwatch.Create(this)
    .OnUpdate(t => timerLabel.text = t.ToClock())
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnUpdateWhen(Func<bool>)",
                            Summary = "Conditionally gate ticking. Stopwatch only ticks while the predicate returns true.",
                            Code =
@"Stopwatch.Create(this)
    .OnUpdateWhen(() => roundActive && !isPaused)
    .OnStop(OnRoundEnd)
    .Start();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnStop(Action)",
                            Summary = "Fires when Stop() is called or SetStopWhen predicate becomes true.",
                            Code =
@"Stopwatch.Create(this)
    .SetStopWhen(() => reachedGoal)
    .OnStop(() => SaveBestTime(stopwatch.ElapsedTime))
    .Start();"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "CallAfter / CallEvery",
                    Namespace = "NekoLib.Timer",
                    Summary = "MonoBehaviour extension helpers for fire-once or repeating timer calls without coroutines.",
                    Description = "CallAfter and CallEvery are extension methods on MonoBehaviour. They return a TimerToken which can be cancelled silently before firing. Both support an optional useUnscaledTime parameter.",
                    Code =
@"// One-shot after delay
TimerToken token    = this.CallAfter(2f, () => Debug.Log(""Fired!""));
TimerToken unscaled = this.CallAfter(2f, () => Debug.Log(""Unscaled""),
    useUnscaledTime: true);
token.Cancel();   // cancel before it fires — no callbacks

// Repeating tick
TimerToken ticker = this.CallEvery(1f, () => Debug.Log(""Tick""));
ticker.Cancel();  // stop the loop",
                    Tags = new[] { "Timer", "Invoke", "Repeat" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "CallAfter(float delay, Action action)",
                            Summary = "Fires action once after delay seconds. Returns a token to cancel before firing.",
                            Code =
@"TimerToken token = this.CallAfter(3f, () => SpawnBoss());
// Cancel if the round ends early
void OnRoundEnd() => token.Cancel();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "CallAfter(float delay, Action action, bool useUnscaledTime)",
                            Summary = "Same as CallAfter but ticks with unscaled time when useUnscaledTime is true.",
                            Code =
@"// Fires after 2 real-world seconds regardless of Time.timeScale
TimerToken t = this.CallAfter(2f, ShowTip, useUnscaledTime: true);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "CallEvery(float interval, Action action)",
                            Summary = "Fires action repeatedly every interval seconds. Returns a token to stop the loop.",
                            Code =
@"TimerToken ticker = this.CallEvery(1f, () => IncrementScore(1));
// Stop when player dies
void OnPlayerDied() => ticker.Cancel();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "CallEvery(float interval, Action action, bool useUnscaledTime)",
                            Summary = "Same as CallEvery but ticks with unscaled time when useUnscaledTime is true.",
                            Code =
@"// UI heartbeat — unaffected by slow-motion
TimerToken hb = this.CallEvery(0.5f, PulseHeartIcon, useUnscaledTime: true);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "TimerToken.IsAlive",
                            Summary = "True while the associated timer is still active (not yet fired or cancelled).",
                            Code =
@"if (spawnToken.IsAlive)
    cancelButton.interactable = true;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "TimerToken.Cancel()",
                            Summary = "Cancels the timer silently. No callback is fired.",
                            Code =
@"spawnToken.Cancel(); // stops fire-once or repeating loop"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "PrefabPool<T>",
                    Namespace = "NekoLib.Pooling",
                    Summary = "Deterministic prefab object pool wrapping Unity's ObjectPool<T>. Use Spawn/Despawn instead of Instantiate/Destroy.",
                    Description = "Implement IPoolable on your MonoBehaviour to receive OnSpawned() and OnDespawned() lifecycle callbacks. PrefabPool<T> handles parenting, transforms, and activation automatically.\n\nLifecycle: OnSpawned → parenting/activation (Spawn) → use → OnDespawned → pool reparent/deactivate (Despawn).",
                    Code =
@"public sealed class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawned()   { /* reset state */ }
    public void OnDespawned() { /* cleanup */     }
}

// Setup
_pool = new PrefabPool<Bullet>(
    bulletPrefab, poolRoot,
    defaultCapacity: 32, maxSize: 256);
_pool.Prewarm(32);

// Runtime
var b = _pool.Spawn(position, rotation);
_pool.Despawn(b);
_pool.Despawn(b, delaySeconds: 2f);  // delayed
_pool.Clear();
int free = _pool.CountInactive;",
                    Tags = new[] { "Pool", "Spawn", "Despawn", "Performance" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "CountInactive",
                            Summary = "Number of instances currently sitting idle in the pool.",
                            Code =
@"Debug.Log($""{_pool.CountInactive} bullets ready in pool"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsValid",
                            Summary = "False if the prefab reference is null or the pool is in a broken state.",
                            Code =
@"if (!_pool.IsValid)
    Debug.LogError(""Pool prefab is missing!"");"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Spawn(Vector3 position, Quaternion rotation)",
                            Summary = "Gets an instance from the pool, places it, and calls OnSpawned().",
                            Code =
@"Bullet b = _pool.Spawn(firePoint.position, firePoint.rotation);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Spawn(Vector3 position, Quaternion rotation, Transform parent)",
                            Summary = "Same as Spawn but reparents the instance under the given Transform.",
                            Code =
@"var effect = _pool.Spawn(hit.point, Quaternion.identity, hitParent);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Spawn()",
                            Summary = "Spawns at Vector3.zero with no rotation. Useful when you set position yourself.",
                            Code =
@"var enemy = _pool.Spawn();
enemy.transform.position = spawnPoint;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Despawn(T instance)",
                            Summary = "Returns the instance to the pool immediately and calls OnDespawned().",
                            Code =
@"_pool.Despawn(bullet); // immediate return"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Despawn(T instance, float delaySeconds)",
                            Summary = "Returns the instance after a delay in seconds.",
                            Code =
@"// Auto-despawn after 3 seconds (e.g. muzzle flash)
_pool.Despawn(muzzleFlash, 3f);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Prewarm(int count)",
                            Summary = "Pre-instantiates count instances into the pool to avoid runtime allocation spikes.",
                            Code =
@"// Called once at level load
_pool.Prewarm(50);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Clear()",
                            Summary = "Destroys all pooled instances and resets the pool.",
                            Code =
@"void OnLevelUnload() => _pool.Clear();"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "PoolableBehaviour",
                    Namespace = "NekoLib.Pooling",
                    Summary = "Base class for poolable objects that need to return themselves to their pool without holding a reference.",
                    Description = "Inherit from PoolableBehaviour instead of MonoBehaviour + IPoolable when your object needs to self-release. Falls back to Destroy() if not managed by a PrefabPool.",
                    Code =
@"public sealed class EnemyProjectile : PoolableBehaviour
{
    public override void OnSpawned()   { /* reset */ }
    public override void OnDespawned() { /* cleanup */ }

    private void OnCollisionEnter(Collision _)
    {
        ReleaseSelf();             // returns to pool or Destroy()
        // ReleaseSelf(delay: 2f); // delayed variant
    }
}",
                    Tags = new[] { "Pool", "Self-release" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "ReleaseSelf()",
                            Summary = "Returns this object to its pool immediately. Falls back to Destroy() if not pooled.",
                            Code =
@"private void OnTriggerEnter2D(Collider2D _)
{
    PlayHitEffect();
    ReleaseSelf();
}"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "ReleaseSelf(float delay)",
                            Summary = "Returns this object to the pool after delay seconds.",
                            Code =
@"private void OnHit()
{
    PlayExplosion();
    ReleaseSelf(delay: 1.5f); // wait for particle to finish
}"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnSpawned()",
                            Summary = "Called by the pool immediately after this instance is retrieved. Reset state here.",
                            Code =
@"public override void OnSpawned()
{
    _health = _maxHealth;
    _rb.velocity = Vector2.zero;
    gameObject.SetActive(true);
}"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnDespawned()",
                            Summary = "Called by the pool just before this instance is returned. Clean up here.",
                            Code =
@"public override void OnDespawned()
{
    _trailRenderer.Clear();
    StopAllCoroutines();
}"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Swatch (Color Palette)",
                    Namespace = "NekoLib.ColorPalette",
                    Summary = "Pre-defined color constants for consistent in-editor and in-game theming.",
                    Description = "Use Swatch constants for consistent Debug log coloring and runtime UI theming. Common swatches: VR (Vibrant Red), VC (Vibrant Cyan), DE (Debug Emphasis), DG (Dark Gray).",
                    Code =
@"Debug.Log(""Success!"".Colorize(Swatch.DE));
Debug.LogError(""Error!"".Colorize(Swatch.VR));

button.color    = Swatch.VC;
errorText.color = Swatch.VR;
Color dark      = Swatch.DG;",
                    Tags = new[] { "Color", "Debug", "UI" },
                    Category = DocCategory.Core
                },
                new NekoLibDocEntry
                {
                    Title = "Log",
                    Namespace = "NekoLib.Logger",
                    Summary = "Conditional logger — Info, Warn, and Assert are stripped in release builds. Error and Exception always fire so crash reporters receive them.",
                    Description = "Log.Info and Log.Warn are compiled only in the Editor, Development builds, or when NEKOLIB_LOG is defined. Log.Error and Log.Exception always fire in all builds so crash reporters (e.g. Firebase Crashlytics, Sentry) capture them via Application.logMessageReceived. Supports an optional context object to ping it in the Console.",
                    Code =
@"using NekoLib.Logger;

Log.Info(""System started"");
Log.Warn(""Low memory"");
Log.Error(""Something broke"");

// With context — click the log entry to ping the object
Log.Info(""Found target"", enemyGameObject);",
                    Tags = new[] { "Logging", "Debug", "Conditional" },
                    Category = DocCategory.Core,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Info(string message)",
                            Summary = "Logs an informational message (white). Stripped in non-development builds.",
                            Code =
@"Log.Info($""Player spawned at {transform.position}"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Info(string message, Object context)",
                            Summary = "Logs with a context object — click the Console entry to ping it in the Hierarchy.",
                            Code =
@"Log.Info(""Target acquired"", enemy.gameObject);"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Warn(string message)",
                            Summary = "Logs a warning (yellow). Stripped in non-development builds.",
                            Code =
@"Log.Warn(""Pool capacity exceeded — consider increasing maxSize"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Error(string message)",
                            Summary = "Logs an error (red). Always fires in all builds so crash reporters can capture it.",
                            Code =
@"Log.Error($""Failed to load asset: {assetName}"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Assert(bool condition, string message)",
                            Summary = "Logs an error and breaks if condition is false. No-op in release builds.",
                            Code =
@"Log.Assert(_pool != null, ""Bullet pool must be assigned"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Exception(Exception exception)",
                            Summary = "Logs an exception with full stack trace. Always fires in all builds so crash reporters can capture it.",
                            Code =
@"try { RiskyOperation(); }
catch (Exception e) { Log.Exception(e); }"
                        },
                    }
                },

                // ── Components ──────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "SpriteAnimator",
                    Namespace = "NekoLib.Components",
                    Summary = "Frame-based sprite animation for SpriteRenderer. Auto-pauses when renderer is disabled.",
                    Description = "Loop modes: Once (stops on last frame), Loop (wraps back), PingPong (reverses). Add per-frame UnityEvent callbacks via the Frame Events inspector tab. OnCycleComplete fires at each cycle boundary.",
                    Code =
@"var anim = GetComponent<SpriteAnimator>();
anim.Play();                                       // Once mode (default)
anim.Play(SpriteAnimatorLoopMode.Loop);            // explicit loop
anim.Restart();
anim.Stop();
anim.SetFrameRate(24f);
anim.GoToFrame(5);
anim.SetFrameRate(0f);    // freeze on current frame

bool playing = anim.IsPlaying;
int  frame   = anim.CurrentFrame;
int  total   = anim.FrameCount;

anim.OnCycleComplete.AddListener(() => Debug.Log(""Cycle done!""));",
                    Tags = new[] { "Animation", "Sprite", "SpriteRenderer" },
                    Category = DocCategory.Components,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsPlaying",
                            Summary = "True while the animation is actively playing (not stopped or paused).",
                            Code =
@"if (anim.IsPlaying)
    anim.Stop();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "CurrentFrame",
                            Summary = "Zero-based index of the currently displayed frame.",
                            Code =
@"Debug.Log($""Frame {anim.CurrentFrame} / {anim.FrameCount - 1}"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "FrameCount",
                            Summary = "Total number of frames in the assigned sprite array.",
                            Code =
@"progressBar.fillAmount = (float)anim.CurrentFrame / anim.FrameCount;"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Play(SpriteAnimatorLoopMode loopMode = Once)",
                            Summary = "Starts playback. Sets and applies the given loop mode immediately (like Rigidbody.isKinematic). Defaults to Once.",
                            Code =
@"anim.Play();                                    // Once — stops at last frame
anim.Play(SpriteAnimatorLoopMode.Loop);         // continuous loop
anim.Play(SpriteAnimatorLoopMode.PingPong);     // bounces back and forth"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Stop()",
                            Summary = "Stops playback and resets to the first frame.",
                            Code =
@"anim.Stop();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Restart()",
                            Summary = "Stops and immediately starts from frame 0.",
                            Code =
@"anim.Restart();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "SetFrameRate(float fps)",
                            Summary = "Changes the playback speed in frames per second. Pass 0 to freeze.",
                            Code =
@"anim.SetFrameRate(12f);   // slow cinematic
anim.SetFrameRate(0f);    // freeze on current frame"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "GoToFrame(int index)",
                            Summary = "Jumps to a specific frame index without changing play state.",
                            Code =
@"anim.GoToFrame(0); // reset to first frame"
                        },
                        // Callbacks
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnCycleComplete",
                            Summary = "UnityEvent fired at the end of each animation cycle (loop wrap or ping-pong reversal).",
                            Code =
@"anim.OnCycleComplete.AddListener(() =>
{
    Debug.Log(""Cycle finished!"");
    SpawnTrailEffect();
});"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "UISpriteAnimator",
                    Namespace = "NekoLib.Components",
                    Summary = "Same as SpriteAnimator but targets a UI Image. Auto-pauses when Image or CanvasGroups have alpha ≤ 0.",
                    Description = "Assign CanvasGroups in the inspector — animation auto-pauses when any group's alpha is zero. Shares the same play/stop API as SpriteAnimator.",
                    Code =
@"// Requires Image component on the same GameObject
var anim = GetComponent<UISpriteAnimator>();
anim.Play();
anim.Stop();
// Assign CanvasGroups in inspector for auto-pause on invisible UI",
                    Tags = new[] { "Animation", "UI", "Image" },
                    Category = DocCategory.Components,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Property, Signature = "IsPlaying",
                            Summary = "True while the animation is actively playing.",
                            Code = @"if (anim.IsPlaying) anim.Stop();" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "CurrentFrame / FrameCount",
                            Summary = "Zero-based current frame index and total number of frames.",
                            Code = @"progressBar.fillAmount = (float)anim.CurrentFrame / anim.FrameCount;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Play() / Stop() / Restart()",
                            Summary = "Same playback control API as SpriteAnimator.",
                            Code =
@"anim.Play();
anim.Play(SpriteAnimatorLoopMode.Loop);
anim.Stop();
anim.Restart();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetFrameRate(float fps) / GoToFrame(int index)",
                            Summary = "Change playback speed or jump to a specific frame.",
                            Code =
@"anim.SetFrameRate(12f);
anim.GoToFrame(0);" },
                        new DocMember { Kind = DocMemberKind.Callback, Signature = "OnCycleComplete",
                            Summary = "UnityEvent fired at the end of each animation cycle.",
                            Code =
@"anim.OnCycleComplete.AddListener(() => PlaySoundEffect(cycleClip));" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "ScrollingBackground variants",
                    Namespace = "NekoLib.Components",
                    Summary = "Continuously scrolls a texture offset for parallax/looping background effects. Four renderer variants.",
                    Description = "Variants: ScrollingSpriteRenderer, ScrollingImage, ScrollingRawImage (scrolls uvRect, no material copy), ScrollingMeshRenderer. All share the same API. Configure Speed, Auto Play, and Use Unscaled Time in the inspector.",
                    Code =
@"var scroller = GetComponent<ScrollingSpriteRenderer>();
scroller.Play();     // reset offset and start
scroller.Pause();
scroller.Resume();
scroller.Stop();     // reset offset

scroller.SetSpeed(new Vector2(0.2f, 0f));
scroller.SetSpeedX(0.2f);
scroller.SetSpeedY(0f);

bool    playing = scroller.IsPlaying;
Vector2 speed   = scroller.Speed;",
                    Tags = new[] { "Background", "Scroll", "Parallax" },
                    Category = DocCategory.Components,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Property, Signature = "IsPlaying",
                            Summary = "True while the scroller is actively scrolling.",
                            Code = @"pauseButton.interactable = scroller.IsPlaying;" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "Speed",
                            Summary = "Current scroll speed as a Vector2 (x = horizontal, y = vertical).",
                            Code = @"scroller.Speed = new Vector2(0.3f, 0f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Play()",
                            Summary = "Resets the offset to zero and starts scrolling.",
                            Code = @"scroller.Play();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Pause() / Resume()",
                            Summary = "Pauses or resumes scrolling without resetting the offset.",
                            Code =
@"scroller.Pause();
// ... player pauses game ...
scroller.Resume();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Stop()",
                            Summary = "Stops scrolling and resets the texture offset to zero.",
                            Code = @"scroller.Stop();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetSpeed(Vector2 speed) / SetSpeedX(float x) / SetSpeedY(float y)",
                            Summary = "Changes the scroll speed at runtime.",
                            Code =
@"scroller.SetSpeed(new Vector2(0.2f, 0f));
scroller.SetSpeedX(0.5f);  // only change X
scroller.SetSpeedY(0f);    // stop vertical" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "AutoDestroy",
                    Namespace = "NekoLib.Components",
                    Summary = "Destroys the GameObject after a configurable delay. Fires OnBeforeDestroy just before destruction.",
                    Description = "Set _destroyAfter (seconds, default 5) in the inspector. Subscribe to OnBeforeDestroy for cleanup. No code required for basic use.",
                    Code =
@"var ad = GetComponent<AutoDestroy>();
ad.OnBeforeDestroy.AddListener(() => Debug.Log(""Goodbye!""));",
                    Tags = new[] { "Lifecycle", "Destroy" },
                    Category = DocCategory.Components,
                    Members = new[]
                    {
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnBeforeDestroy",
                            Summary = "UnityEvent fired one frame before the GameObject is destroyed. Use for cleanup or visual effects.",
                            Code =
@"var ad = GetComponent<AutoDestroy>();
ad.OnBeforeDestroy.AddListener(() =>
{
    particleSystem.Stop();
    AudioManager.Instance.PlayOneShot(destroyClip);
});"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "LookAtCamera",
                    Namespace = "NekoLib.Components",
                    Summary = "Makes a GameObject face the camera every frame. Four facing modes selectable in the inspector.",
                    Description = "Modes: LookAt, LookAtInverted, CameraForward, CameraForwardInverted. Uses Camera.main by default. Override via inspector.",
                    Code =
@"// Pure inspector setup — no code required.
// Use Custom Camera + Camera To Look At to override Camera.main.",
                    Tags = new[] { "Billboard", "Camera" },
                    Category = DocCategory.Components
                },
                new NekoLibDocEntry
                {
                    Title = "AutoOrbitAround",
                    Namespace = "NekoLib.Components",
                    Summary = "Continuously orbits around a target transform. Configurable speed, elevation, bearing, and facing mode.",
                    Description = "Inspector fields: Target, Distance, Speed (deg/s; negative = reverse), StartAngle (stagger multiple orbiters), ElevationAngle (0 = flat ring, 90 = vertical loop), BearingAngle, Facing mode. Draws a gizmo arc in the Scene view.",
                    Code =
@"// Pure inspector setup. To evenly space three orbiters:
// orbiter1._startAngle = 0f;
// orbiter2._startAngle = 120f;
// orbiter3._startAngle = 240f;",
                    Tags = new[] { "Orbit", "Transform", "Gizmo" },
                    Category = DocCategory.Components
                },

                // ── Extensions ──────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "GameObjectExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "GetOrAdd<T>, active-state helpers, layer management, child queries.",
                    Description = "GetOrAdd<T> avoids repeated GetComponent + AddComponent boilerplate. SetActive()/SetInactive() improve intent clarity. ClearChildTransforms() destroys all children. GetChildrenInLayer/Recursive filter by LayerMask.",
                    Code =
@"AudioSource audio = gameObject.GetOrAdd<AudioSource>();

mono.SetActive();
mono.SetInactive();

bool inLayer = gameObject.IsInLayer(LayerMask.GetMask(""Enemy""));
gameObject.SetLayer(""Player"");
gameObject.SetLayer(8);

gameObject.ClearChildTransforms();

GameObject[] enemies =
    gameObject.GetChildrenInLayerRecursive(LayerMask.GetMask(""Enemy""));",
                    Tags = new[] { "GameObject", "Component", "Layer" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetOrAdd<T>()",
                            Summary = "Returns the component if it exists, otherwise adds and returns a new one.",
                            Code = @"var rb = gameObject.GetOrAdd<Rigidbody2D>();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetActive() / SetInactive()",
                            Summary = "Readable shorthands for SetActive(true) and SetActive(false).",
                            Code = @"enemy.SetInactive();   // same as enemy.SetActive(false)
player.SetActive();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsInLayer(LayerMask mask)",
                            Summary = "Returns true if the GameObject's layer is included in the LayerMask.",
                            Code =
@"if (gameObject.IsInLayer(LayerMask.GetMask(""Enemy"")))
    TakeDamage();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetLayer(string name) / SetLayer(int index)",
                            Summary = "Sets the layer by name string or integer index.",
                            Code =
@"gameObject.SetLayer(""Ignore Raycast"");
gameObject.SetLayer(2);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ClearChildTransforms()",
                            Summary = "Destroys all immediate child GameObjects.",
                            Code = @"contentContainer.ClearChildTransforms(); // clear spawned list items" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetChildrenInLayer(LayerMask mask)",
                            Summary = "Returns immediate children whose layer matches the given LayerMask.",
                            Code = @"GameObject[] walls = room.GetChildrenInLayer(LayerMask.GetMask(""Wall""));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetChildrenInLayerRecursive(LayerMask mask)",
                            Summary = "Returns all descendants (any depth) whose layer matches the LayerMask.",
                            Code = @"GameObject[] all = root.GetChildrenInLayerRecursive(LayerMask.GetMask(""Enemy""));" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "TransformExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Clear children, LookAt2D, distance/direction helpers, transform resets.",
                    Description = "Clear() destroys all children. GetChildren() collects them. LookAt2D supports an optional angle offset. DistanceTo/DirectionTo/InRangeOf are readable alternatives to Vector3.Distance. ResetTransform / ResetLocalTransform zero world or local TRS.",
                    Code =
@"transform.Clear();
Transform[] kids = transform.GetChildren(includeInactive: false);

transform.LookAt2D(targetPos);
transform.LookAt2D(targetTr, angleOffset: 90f);

float   dist    = transform.DistanceTo(other);
Vector3 dir     = transform.DirectionTo(other);
bool    inRange = transform.InRangeOf(other, 5f);

transform.ResetTransform();       // world TRS -> identity
transform.ResetLocalTransform();  // local TRS -> identity",
                    Tags = new[] { "Transform", "Distance", "Children" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Clear()",
                            Summary = "Destroys all immediate child GameObjects.",
                            Code = @"poolContainer.Clear(); // destroy all spawned children" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetChildren(bool includeInactive)",
                            Summary = "Returns all immediate children as a Transform array. Pass true to include inactive ones.",
                            Code =
@"Transform[] active = transform.GetChildren();
Transform[] all    = transform.GetChildren(includeInactive: true);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "LookAt2D(Transform target)",
                            Summary = "Rotates this transform to face a 2D position or transform. Optional angle offset.",
                            Code =
@"transform.LookAt2D(enemy.position);
transform.LookAt2D(enemy, angleOffset: 90f); // offset for sprites facing right" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "DistanceTo(Transform other)",
                            Summary = "Returns the world-space distance to another Transform or Vector3.",
                            Code =
@"float dist = transform.DistanceTo(target);
if (dist < attackRange) Attack();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "DirectionTo(Transform other)",
                            Summary = "Returns the normalized direction vector toward another Transform or Vector3.",
                            Code = @"Vector3 dir = transform.DirectionTo(target);
rb.velocity = dir * speed;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "InRangeOf(Transform other, float range)",
                            Summary = "Returns true when the distance to other is less than or equal to range.",
                            Code =
@"if (transform.InRangeOf(player.transform, detectionRadius))
    ChasePlayer();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ResetTransform()",
                            Summary = "Resets world position to zero, rotation to identity, and scale to one.",
                            Code = @"spawnPoint.ResetTransform();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ResetLocalTransform()",
                            Summary = "Resets local position, rotation, and scale to their identity values.",
                            Code = @"uiPanel.transform.ResetLocalTransform();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "ColorExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Fluent Color modifications: alpha, channels, multiply, add, invert, grayscale, luminance, hex.",
                    Description = "All channel methods return a new Color — the original is unchanged. ToHex produces #RRGGBBAA. ToColor parses hex strings.",
                    Code =
@"Color faded   = color.WithAlpha(0.5f);
Color redded  = color.WithRed(1f);
Color bright  = color.MultiplyRGB(1.5f);
Color lighter = color.AddRGB(0.1f);
Color inv     = color.Invert();
Color grey    = color.ToGrayscale();
float lum     = color.GetLuminance();

string hex   = color.ToHex();           // ""#RRGGBBAA""
Color parsed = ""#FF0000FF"".ToColor();",
                    Tags = new[] { "Color", "Fluent", "Hex" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WithAlpha(float a)",
                            Summary = "Returns a copy of the color with the alpha channel set to a.",
                            Code = @"image.color = baseColor.WithAlpha(0.5f); // 50% transparent" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WithRed(float r) / WithGreen(float g) / WithBlue(float b)",
                            Summary = "Returns a copy with a single channel replaced. Original unchanged.",
                            Code = @"Color warning = color.WithRed(1f).WithBlue(0f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "MultiplyRGB(float factor)",
                            Summary = "Multiplies all RGB channels by factor. Useful for brightening or darkening.",
                            Code = @"Color highlight = baseColor.MultiplyRGB(1.4f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AddRGB(float value)",
                            Summary = "Adds value to all RGB channels. Clamps to [0, 1].",
                            Code = @"Color lighter = color.AddRGB(0.15f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Invert()",
                            Summary = "Returns a color with each RGB channel inverted (1 - channel). Alpha unchanged.",
                            Code = @"Color inv = accentColor.Invert();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToGrayscale()",
                            Summary = "Converts to grayscale using luminance weights.",
                            Code = @"Color grey = spriteColor.ToGrayscale();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetLuminance()",
                            Summary = "Returns the perceptual luminance (0–1) of the color.",
                            Code = @"bool isDark = color.GetLuminance() < 0.5f;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToHex()",
                            Summary = "Returns the color as a #RRGGBBAA hex string.",
                            Code = @"string hex = selectedColor.ToHex(); // ""#FF6600FF""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToColor()  (string ext)",
                            Summary = "Parses a #RRGGBB or #RRGGBBAA hex string into a Color.",
                            Code = @"Color c = ""#FF6600"".ToColor();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Vector2 / Vector3 Extensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Fluent component modification, distance/direction, range checks, perpendicular, rotate, and random annulus.",
                    Description = "With/Add/Subtract/Multiply/Divide allow per-component modification without allocating. InRangeOf/DirectionTo/DistanceTo simplify spatial queries. Perpendicular/PerpendicularClockwise, Rotate(). IsInsideCircle/Rect/Sphere/Box for hit checks. RandomPointInAnnulus for spawn scatter.",
                    Code =
@"// Vector2
Vector2 v   = vector.With(x: 5f).Add(y: 2f);
bool near   = pos.InRangeOf(target, 5f);
Vector2 dir = from.DirectionTo(to);
Vector2 per = vector.Perpendicular();
Vector2 rot = vector.Rotate(45f);
Vector2 rnd = origin.RandomPointInAnnulus(2f, 8f);

// Vector3
Vector3 w     = vector.With(y: 10f).RotateY(90f);
bool inSphere = point.IsInsideSphere(center, 5f);
bool inBox    = point.IsInsideBox(center, size);",
                    Tags = new[] { "Vector", "Math", "Spatial" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "With(float? x, float? y) / With(float? x, float? y, float? z)",
                            Summary = "Returns a copy with specified components replaced. Unspecified components are preserved.",
                            Code =
@"Vector2 up    = vel.With(y: 0f);      // zero out Y
Vector3 flat  = pos.With(y: 0f);      // flatten to XZ plane" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Add / Subtract / Multiply / Divide",
                            Summary = "Fluent per-component arithmetic. All return a new vector.",
                            Code =
@"Vector2 shifted = pos.Add(x: 2f).Subtract(y: 1f);
Vector3 scaled  = size.Multiply(z: 2f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "InRangeOf(Vector2 target, float range)",
                            Summary = "Returns true when this vector is within range distance of target.",
                            Code =
@"if (transform.position.InRangeOf(target.position, 5f))
    Attack();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "DirectionTo(Vector2 target)",
                            Summary = "Returns a normalized direction vector from this point to target.",
                            Code = @"Vector2 dir = origin.DirectionTo(target);
rb.velocity = dir * speed;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "DistanceTo(Vector2 target)",
                            Summary = "Returns the distance from this vector to target.",
                            Code = @"float d = pos.DistanceTo(enemy.position);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Perpendicular() / PerpendicularClockwise()",
                            Summary = "Returns a vector perpendicular to this one (counter-clockwise or clockwise).",
                            Code = @"Vector2 normal = moveDir.Perpendicular();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Rotate(float degrees)  (Vector2 ext)",
                            Summary = "Rotates the Vector2 by the given degrees counter-clockwise.",
                            Code = @"Vector2 rotated = forward.Rotate(45f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RandomPointInAnnulus(float min, float max)",
                            Summary = "Returns a random point within a ring defined by min and max radius.",
                            Code = @"Vector2 spawnPos = origin.RandomPointInAnnulus(3f, 8f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RotateX(float deg) / RotateY(float deg) / RotateZ(float deg)  (Vector3 ext)",
                            Summary = "Rotates the Vector3 around the specified axis by degrees.",
                            Code = @"Vector3 orbiting = basePos.RotateY(angle);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsInsideSphere(Vector3 center, float radius)",
                            Summary = "Returns true when this point is inside the sphere.",
                            Code = @"if (point.IsInsideSphere(explosionCenter, blastRadius)) ApplyDamage();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsInsideBox(Vector3 center, Vector3 size)",
                            Summary = "Returns true when this point is inside the axis-aligned box.",
                            Code = @"if (pos.IsInsideBox(roomCenter, roomSize)) TriggerRoomEvent();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "StringExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Comma-float parsing, SplitCamelCase, percent formatting, short number formats (K/M/B), enum conversion.",
                    Description = "ToShortFormat handles K/M/B/T and beyond for idle-game style large numbers. ToShortABCFormat uses alphabetic suffixes (a/b/c...). AsPercent multiplies by 100; AsExactPercent uses the value directly.",
                    Code =
@"float  val   = ""3,14"".ParseFloatWithComma();    // 3.14f
string split = ""MyVarName"".SplitCamelCase();    // ""My Var Name""

string pct   = 0.25f.AsPercent();               // ""25%""
string exact = 25f.AsExactPercent();             // ""25%""

string short1 = 1_500_000m.ToShortFormat(1);    // ""1.5M""
string abc    = 1_500m.ToShortABCFormat();       // ""1.5a""

MyEnum e = ""EnumValue"".ToEnum<MyEnum>();",
                    Tags = new[] { "String", "Format", "Number" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ParseFloatWithComma()",
                            Summary = "Parses a float string that uses a comma as the decimal separator (e.g. European locale).",
                            Code = @"float f = ""3,14"".ParseFloatWithComma(); // 3.14f" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SplitCamelCase()",
                            Summary = "Inserts spaces before capital letters. Useful for displaying field names as labels.",
                            Code = @"string label = nameof(playerHealth).SplitCamelCase(); // ""Player Health""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AsPercent()  (float ext)",
                            Summary = "Multiplies the float by 100 and appends %. 0.25f → \"25%\".",
                            Code = @"winRateLabel.text = winRate.AsPercent(); // ""72%""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AsExactPercent()  (float ext)",
                            Summary = "Appends % to the value as-is, without multiplying. 25f → \"25%\".",
                            Code = @"label.text = healthPercent.AsExactPercent();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToShortFormat(int decimals)  (decimal ext)",
                            Summary = "Formats large numbers with K/M/B/T suffixes for idle-game style display.",
                            Code =
@"string s = 1_500_000m.ToShortFormat(1); // ""1.5M""
string t = 2_300m.ToShortFormat(0);     // ""2K""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToShortABCFormat()  (decimal ext)",
                            Summary = "Formats large numbers with alphabetic suffixes (a, b, c…) for idle games.",
                            Code = @"string s = 1_500m.ToShortABCFormat(); // ""1.5a""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToEnum<T>()",
                            Summary = "Parses the string as an enum value of type T.",
                            Code = @"WeaponType wt = ""Sword"".ToEnum<WeaponType>();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "CollectionExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Rand, Shuffle, Swap, RandWeighted, RandMultiple, Slice, ToLiteral, and more.",
                    Description = "All helpers are generic. RandWeighted accepts a weight selector func. ToLiteral gives human-readable string representations useful for debug logging. Null-safety: IsNullOrEmpty, ContainsNull.",
                    Code =
@"T    item  = array.Rand();
int  idx   = array.RandIndex();
T[]  sh    = array.Shuffle();
T[]  sl    = array.Slice(2, 5);
T[]  multi = array.RandMultiple(3);
T    wgt   = array.RandWeighted(x => x.weight);
string lit = array.ToLiteral();   // ""[a, b, c]""

bool empty = list.IsNullOrEmpty();

V      rv   = dict.RandV();
string dlit = dict.ToLiteral();  // ""{k: v, ...}""",
                    Tags = new[] { "Array", "List", "Dictionary", "Random" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Rand()",
                            Summary = "Returns a random element. Works on T[], List<T>. Throws if null or empty.",
                            Code = @"string name = namePool.Rand();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RandIndex()",
                            Summary = "Returns a random valid index into the array or list.",
                            Code = @"int i = slots.RandIndex();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Shuffle()",
                            Summary = "Returns a new shuffled copy without modifying the original.",
                            Code =
@"T[] deck = cards.Shuffle();
foreach (var card in deck) Deal(card);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RandMultiple(int count)",
                            Summary = "Returns count unique random elements without replacement.",
                            Code = @"Item[] loot = dropTable.RandMultiple(3);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RandWeighted(Func<T, float> weightSelector)",
                            Summary = "Picks one element using weighted probability. Higher weight = more likely.",
                            Code =
@"Enemy e = spawnTable.RandWeighted(x => x.spawnWeight);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Swap(T a, T b)",
                            Summary = "Returns a new array/list with two elements swapped by index or value.",
                            Code =
@"int[] sorted = order.Swap(0, 2);     // by index
string[] s  = names.Swap(""Alice"", ""Bob""); // by value" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Slice(int startIndex, int length)",
                            Summary = "Returns a sub-array from startIndex with the given length.",
                            Code = @"T[] page = allItems.Slice(pageIndex * 10, 10);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsNullOrEmpty()",
                            Summary = "Returns true if the collection is null or contains no elements. Works on T[], List<T>, ICollection<T>.",
                            Code =
@"if (inventory.IsNullOrEmpty())
    ShowEmptyState();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ContainsNull()",
                            Summary = "Returns true if any element in the array or list is null.",
                            Code = @"Log.Assert(!slots.ContainsNull(), ""Null slot detected"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToLiteral()",
                            Summary = "Returns a human-readable string of the collection contents for debug logging.",
                            Code =
@"Debug.Log(inventory.ToLiteral());   // ""[Sword, Shield, Potion]""
Debug.Log(stats.ToLiteral());       // ""{hp: 100, mp: 50}""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "RandV() / RandK()  (Dictionary)",
                            Summary = "Returns a random value or key from a Dictionary.",
                            Code =
@"string key = dialogueMap.RandK();
string val = dialogueMap.RandV();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "NumberExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "IsEven, IsOdd, PercentageOf, AtLeast/AtMost clamping, IsSuccessfulRoll probability, ToEnum.",
                    Description = "IsSuccessfulRoll(float) treats the value as a 0–1 probability. The int overload takes a max parameter. AtLeast/AtMost chain for expressive clamping.",
                    Code =
@"bool even  = 42.IsEven();
bool odd   = 13.IsOdd();
float pct  = current.PercentageOf(total);

int clamped = value.AtLeast(10).AtMost(100);

bool hit  = 0.75f.IsSuccessfulRoll();      // 75% chance
bool hit2 = 25.IsSuccessfulRoll(0, 100);   // 25 out of 100

MyEnum e = 1.ToEnum<MyEnum>();",
                    Tags = new[] { "Number", "Math", "Random" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsEven() / IsOdd()",
                            Summary = "Returns true if the integer is even or odd.",
                            Code = @"if (waveNumber.IsEven()) SpawnEliteEnemy();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "PercentageOf(float total)",
                            Summary = "Returns this value as a 0–1 fraction of total.",
                            Code =
@"float fill = currentHealth.PercentageOf(maxHealth);
healthBar.fillAmount = fill;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AtLeast(T min) / AtMost(T max)",
                            Summary = "Clamps the value to a minimum or maximum. Both can be chained.",
                            Code =
@"int damage = (baseDamage - armor).AtLeast(0);
float vol = rawVolume.AtLeast(0f).AtMost(1f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsSuccessfulRoll()  (float)",
                            Summary = "Treats the value as a 0–1 probability and returns true with that chance.",
                            Code =
@"// 30% chance to drop loot
if (0.30f.IsSuccessfulRoll())
    SpawnLoot();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsSuccessfulRoll(int min, int max)  (int ext)",
                            Summary = "Returns true if a random int in [min, max) is less than the value.",
                            Code = @"if (25.IsSuccessfulRoll(0, 100)) TriggerCriticalHit(); // 25% chance" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToEnum<T>()",
                            Summary = "Converts the integer to the enum of type T.",
                            Code = @"GameState state = savedValue.ToEnum<GameState>();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "TimeExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "ToClock, ToShortClock, ToReadableFormat, and DateTime elapsed/remaining helpers.",
                    Description = "ToClock formats seconds as HH:MM:SS. ToShortClock gives MM:SS. ToReadableFormat produces \"1d 2h 3m\" style strings. DateTime helpers use TimeService.Now for server-synced results.",
                    Code =
@"string clock = 3661f.ToClock();            // ""01:01:01""
string sh    = 125f.ToShortClock();        // ""02:05""
string rdbl  = 93784f.ToReadableFormat(); // ""1d 2h 3m""

DateTime past    = TimeService.Now.AddHours(-2);
TimeSpan elapsed = past.TimeSince();
double   secs    = past.SecondsSince();

DateTime future = TimeService.Now.AddHours(3);
double   left   = future.SecondsFromNow();",
                    Tags = new[] { "Time", "Format", "Clock" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToClock()  (float ext)",
                            Summary = "Formats a float of seconds as HH:MM:SS.",
                            Code = @"timerLabel.text = remainingSeconds.ToClock(); // ""01:30:00""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToShortClock()  (float ext)",
                            Summary = "Formats a float of seconds as MM:SS.",
                            Code = @"timerLabel.text = remainingSeconds.ToShortClock(); // ""02:45""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ToReadableFormat()  (float ext)",
                            Summary = "Formats seconds into a human-readable \"1d 2h 3m 4s\" string, omitting zero units.",
                            Code = @"cooldownLabel.text = totalSeconds.ToReadableFormat(); // ""3h 20m""" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "TimeSince()  (DateTime ext)",
                            Summary = "Returns the TimeSpan elapsed since the given DateTime.",
                            Code =
@"var lastLogin = DateTime.Parse(PlayerPrefs.GetString(""LastLogin""));
TimeSpan offline = lastLogin.TimeSince();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SecondsSince()  (DateTime ext)",
                            Summary = "Returns the total seconds elapsed since the given DateTime as a double.",
                            Code = @"double secs = lastEventTime.SecondsSince();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SecondsFromNow()  (DateTime ext)",
                            Summary = "Returns the total seconds from now until the given future DateTime.",
                            Code =
@"DateTime resetTime = TimeService.NextDay;
double secs = resetTime.SecondsFromNow();
countdownLabel.text = ((float)secs).ToClock();" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "TextColorize / TextFormat Extensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Rich-text helpers: Colorize (whole, selective, conditional), Bold, Italic, Size — all chainable.",
                    Description = "Colorize injects Unity rich-text color tags. Selective overloads target specific substrings or chars. Conditional overloads take a predicate. Bold/Italic/Size support selective targeting and can be chained.",
                    Code =
@"string colored   = ""Hello"".Colorize(Color.red);
string selective = ""Hello World"".Colorize(Color.red, ""Hello"");
string cond      = ""Error"".Colorize(Color.red, () => hasError);

string bold   = ""Important"".Bold();
string italic = ""Emphasis"".Italic();
string sized  = ""Big"".Size(24f);

string result = ""Important Warning""
    .Bold(""Important"")
    .Italic(""Warning"")
    .Size(18f, ""Warning"");",
                    Tags = new[] { "RichText", "Color", "Format" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Colorize(Color color)",
                            Summary = "Wraps the entire string in a Unity rich-text color tag.",
                            Code = @"debugLabel.text = message.Colorize(Color.yellow);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Colorize(Color color, string substring)",
                            Summary = "Colors only the first occurrence of substring within the string.",
                            Code = @"string s = ""Error: file not found"".Colorize(Color.red, ""Error"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Colorize(Color color, Func<char, bool> predicate)",
                            Summary = "Colorizes the whole string only when the predicate returns true.",
                            Code = @"string s = value.ToString().Colorize(Color.red, () => value < 0);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Bold()",
                            Summary = "Wraps the string in a <b> tag. Pass a substring to bold only that part.",
                            Code =
@"label.text = $""Score: {""{score}"".Bold()}"";
label.text = ""Tip: press Space to jump"".Bold(""Space"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Italic()",
                            Summary = "Wraps the string in an <i> tag. Pass a substring to italicize only that part.",
                            Code = @"label.text = caption.Italic();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Size(int fontSize)",
                            Summary = "Wraps the string in a <size> tag. Pass a substring to size only that part.",
                            Code = @"label.text = title.Size(24f);" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "CoroutineExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Sequential, parallel, delayed, conditional coroutine starters. Coroutine↔Task bridge.",
                    Description = "StartCoroutineSequence runs coroutines one after another. StartCoroutineParallel runs them concurrently. StartCoroutineDelayed adds a wait. StartCoroutineWhen waits for a condition. AsTask, WhenAll, WhenAny bridge into async/await.",
                    Code =
@"this.StartCoroutineSequence(corA, corB, corC);
this.StartCoroutineParallel(corA, corB);
this.StartCoroutineDelayed(myCor, 2f);
this.StartCoroutineWhen(myCor, () => isReady);

// Async bridge
Task t = StartCoroutine(myCor).AsTask(this);
await t;
await this.WhenAll(corA, corB, corC);",
                    Tags = new[] { "Coroutine", "Async", "Task" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartCoroutineSequence(params IEnumerator[])",
                            Summary = "Runs coroutines one after another, waiting for each to finish before starting the next.",
                            Code =
@"this.StartCoroutineSequence(
    FadeOut(),
    LoadScene(),
    FadeIn());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartCoroutineParallel(params IEnumerator[])",
                            Summary = "Starts all coroutines simultaneously on the same MonoBehaviour.",
                            Code = @"this.StartCoroutineParallel(PlayMusic(), AnimateUI(), SpawnEnemies());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartCoroutineDelayed(IEnumerator coroutine, float delay)",
                            Summary = "Waits delay seconds before starting the coroutine.",
                            Code = @"this.StartCoroutineDelayed(SpawnBoss(), 3f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartCoroutineWhen(IEnumerator coroutine, Func<bool> predicate)",
                            Summary = "Polls the predicate each frame and starts the coroutine once it returns true.",
                            Code = @"this.StartCoroutineWhen(BeginCutscene(), () => assetsLoaded);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AsTask(MonoBehaviour owner)",
                            Summary = "Converts a running Coroutine handle to a Task that completes when the coroutine ends.",
                            Code =
@"Task t = StartCoroutine(MyCoroutine()).AsTask(this);
await t;
Debug.Log(""Coroutine done"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WhenAll(params IEnumerator[])",
                            Summary = "Runs all coroutines in parallel and returns a Task that completes when all finish.",
                            Code = @"await this.WhenAll(LoadAudio(), LoadTextures(), LoadLevel());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WhenAny(params IEnumerator[])",
                            Summary = "Runs all coroutines and returns a Task that completes when the first one finishes.",
                            Code = @"await this.WhenAny(WaitForInput(), TimeoutCoroutine(5f));" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "TaskExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Forget() for fire-and-forget tasks, YieldTask for awaiting Tasks inside coroutines.",
                    Description = "Forget() swallows exceptions unless you supply an error handler. YieldTask wraps a Task in a CustomYieldInstruction. AsCoroutine() converts an async Task to a yield instruction.",
                    Code =
@"myTask.Forget();
myTask.Forget(ex => Log.Error($""Failed: {ex}""));

IEnumerator Example()
{
    Task<string> webTask = FetchAsync();
    yield return new YieldTask(webTask);
    Log.Info(webTask.Result);

    yield return AnotherAsync().AsCoroutine();
}",
                    Tags = new[] { "Task", "Async", "Coroutine" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Forget()",
                            Summary = "Fire-and-forget a Task. Silently swallows any exception.",
                            Code = @"SaveDataAsync().Forget();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Forget(Action<Exception> errorHandler)",
                            Summary = "Fire-and-forget with an error handler invoked if the Task faults.",
                            Code = @"FetchLeaderboardAsync().Forget(ex => Log.Error($""Fetch failed: {ex.Message}""));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AsCoroutine()",
                            Summary = "Wraps a Task as a yield instruction so it can be awaited inside a coroutine.",
                            Code =
@"IEnumerator LoadSequence()
{
    yield return FetchServerTimeAsync().AsCoroutine();
    Debug.Log(""Time fetched, continuing..."");
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "new YieldTask(task)",
                            Summary = "Wraps a Task<T> so you can yield return it and read the result afterward.",
                            Code =
@"IEnumerator Fetch()
{
    var task = new YieldTask(DownloadAsync());
    yield return task;
    Debug.Log(task.Result);
}" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "AnimatorExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Clip length queries, progress tracking, state checks, PlayAndWait / CrossFadeAndWait.",
                    Description = "GetAnimationLength works with both string names and hash ints. GetCurrentAnimationProgress/RemainingTime give frame-accurate values. PlayAndWait and CrossFadeAndWait are coroutine yield helpers.",
                    Code =
@"float len  = animator.GetAnimationLength(""Jump"");
float prog = animator.GetCurrentAnimationProgress(layerIndex: 0);
float left = animator.GetCurrentAnimationRemainingTime(0);
bool  ok   = animator.IsPlayingAnimation(""Jump"");

yield return animator.PlayAndWait(""Attack"");
yield return animator.CrossFadeAndWait(""Idle"", 0.2f);
yield return animator.WaitForAnimation(""Death"");",
                    Tags = new[] { "Animator", "Animation", "Coroutine" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetAnimationLength(string name)",
                            Summary = "Returns the clip length in seconds for the named animation state.",
                            Code =
@"float attackDur = animator.GetAnimationLength(""Attack"");
this.CallAfter(attackDur, OnAttackEnd);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetCurrentAnimationProgress(int layer)",
                            Summary = "Returns the normalized playback position (0–1) of the current state on the given layer.",
                            Code =
@"float prog = animator.GetCurrentAnimationProgress(0);
progressBar.fillAmount = prog;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetCurrentAnimationRemainingTime(int layer)",
                            Summary = "Returns the remaining clip time in seconds for the current animation state.",
                            Code =
@"float left = animator.GetCurrentAnimationRemainingTime(0);
this.CallAfter(left, SpawnHitEffect);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPlayingAnimation(string name)",
                            Summary = "Returns true if the animator is currently in the named state.",
                            Code = @"if (!animator.IsPlayingAnimation(""Idle"")) return;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "PlayAndWait(string name)",
                            Summary = "Plays the animation and yields until it finishes. Use inside a coroutine.",
                            Code =
@"IEnumerator AttackSequence()
{
    yield return animator.PlayAndWait(""Attack"");
    yield return animator.PlayAndWait(""Recover"");
    isAttacking = false;
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "CrossFadeAndWait(string name, float transitionDuration)",
                            Summary = "Cross-fades to the animation and yields until the target clip finishes.",
                            Code =
@"IEnumerator Die()
{
    yield return animator.CrossFadeAndWait(""Death"", 0.2f);
    gameObject.SetInactive();
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WaitForAnimation(string name)",
                            Summary = "Yields until the named animation starts playing on layer 0.",
                            Code =
@"animator.SetTrigger(""Jump"");
yield return animator.WaitForAnimation(""Jump"");" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "CameraExtensions",
                    Namespace = "NekoLib.Extensions",
                    Summary = "Culling mask management, FOV control, orthographic size, FitBoundsInView, screen size query.",
                    Description = "Culling helpers: IsLayerInCullingMask, AddToCullingMask, RemoveFromCullingMask, ShowAllLayers, HideAllLayers. FOV: ZoomIn, ZoomOut, SetFOV. Orthographic: SetOrthographicSize, FitBoundsInView.",
                    Code =
@"camera.AddToCullingMask(LayerMask.GetMask(""UI""));
camera.RemoveFromCullingMask(LayerMask.GetMask(""UI""));
camera.ShowAllLayers();
camera.HideAllLayers();

camera.ZoomIn(15f);
camera.SetFOV(60f);
camera.SetOrthographicSize(5f);
camera.FitBoundsInView(bounds);

Vector2 screen = camera.GetScreenSize();",
                    Tags = new[] { "Camera", "Culling", "FOV" },
                    Category = DocCategory.Extensions,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AddToCullingMask(LayerMask mask) / RemoveFromCullingMask(LayerMask mask)",
                            Summary = "Adds or removes layers from the camera's culling mask without touching other layers.",
                            Code =
@"cam.AddToCullingMask(LayerMask.GetMask(""UI""));
cam.RemoveFromCullingMask(LayerMask.GetMask(""HUD""));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsLayerInCullingMask(int layer)",
                            Summary = "Returns true if the given layer index is visible to this camera.",
                            Code = @"if (!cam.IsLayerInCullingMask(uiLayer)) cam.AddToCullingMask(1 << uiLayer);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ShowAllLayers() / HideAllLayers()",
                            Summary = "Sets the culling mask to everything or nothing.",
                            Code =
@"screenshotCam.HideAllLayers();
screenshotCam.AddToCullingMask(LayerMask.GetMask(""Game""));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ZoomIn(float fov) / ZoomOut(float fov) / SetFOV(float fov)",
                            Summary = "Adjusts the perspective camera field of view.",
                            Code =
@"cam.ZoomIn(15f);   // fov -= 15
cam.ZoomOut(15f);  // fov += 15
cam.SetFOV(60f);   // absolute" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetOrthographicSize(float size)",
                            Summary = "Sets the orthographic camera's half-height size.",
                            Code = @"cam.SetOrthographicSize(5f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "FitBoundsInView(Bounds bounds)",
                            Summary = "Adjusts orthographic size or FOV so the given Bounds fits entirely in view.",
                            Code = @"cam.FitBoundsInView(levelBounds);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetScreenSize()",
                            Summary = "Returns the screen dimensions as a Vector2 (width, height) in pixels.",
                            Code = @"Vector2 screen = cam.GetScreenSize();" },
                    }
                },

                // ── Services ────────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "TimeService",
                    Namespace = "NekoLib.Services",
                    Summary = "Server-synchronized DateTime service. Falls back to DateTime.Now until a successful sync.",
                    Description = "Call FetchTimeFromServerAsync() at startup to sync. Access via TimeService.Now/UtcNow. Define NEKO_TIME_SERVICE_DEBUG to skip real web requests locally.\n\nPeriod helpers: IsTodayStartOfWeek, IsTodayStartOfMonth. Date helpers: Today, TodayUtc, NextDay, NextDayUtc.",
                    Code =
@"bool synced = await TimeService.FetchTimeFromServerAsync();

// Or coroutine variant
StartCoroutine(TimeService.FetchTimeFromServerCoroutine(
    ok => Debug.Log($""Synced: {ok}"")));

DateTime now  = TimeService.Now;
DateTime utc  = TimeService.UtcNow;
bool mon      = TimeService.IsTodayStartOfWeek;

// Daily reward check
var last = DateTime.Parse(PlayerPrefs.GetString(""LastLogin""));
if (TimeService.Now.Date > last.Date)
    GiveDailyReward();",
                    Tags = new[] { "Time", "Server", "DateTime" },
                    Category = DocCategory.Services,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "Now",
                            Summary = "Current local DateTime. Uses server time after a successful sync, otherwise device time.",
                            Code =
@"DateTime now = TimeService.Now;
Debug.Log($""Local time: {now:HH:mm:ss}"");"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "UtcNow",
                            Summary = "Current UTC DateTime. Uses server time after sync, otherwise device UTC.",
                            Code =
@"DateTime utc = TimeService.UtcNow;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "Today",
                            Summary = "Today's date at midnight (local). Equivalent to TimeService.Now.Date.",
                            Code =
@"DateTime today = TimeService.Today;"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "HasSynced",
                            Summary = "True after at least one successful server fetch.",
                            Code =
@"if (!TimeService.HasSynced)
    await TimeService.FetchTimeFromServerAsync();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsTodayStartOfWeek",
                            Summary = "True if today is Monday (start of ISO week).",
                            Code =
@"if (TimeService.IsTodayStartOfWeek)
    ResetWeeklyRewards();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsTodayStartOfMonth",
                            Summary = "True if today is the 1st of the month.",
                            Code =
@"if (TimeService.IsTodayStartOfMonth)
    ResetMonthlyLeaderboard();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "NextDay",
                            Summary = "Tomorrow's date at midnight (local).",
                            Code =
@"TimeSpan untilReset = TimeService.NextDay - TimeService.Now;
label.text = untilReset.ToReadableFormat();"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "FetchTimeFromServerAsync()",
                            Summary = "Async: syncs with a public time API. Returns true on success. Call once at startup.",
                            Code =
@"async void Start()
{
    bool ok = await TimeService.FetchTimeFromServerAsync();
    if (!ok) Debug.LogWarning(""Time sync failed — using device time"");
}"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "FetchTimeFromServerCoroutine(Action<DateTime> callback)",
                            Summary = "Coroutine variant of FetchTimeFromServerAsync. Invokes callback with success bool.",
                            Code =
@"StartCoroutine(TimeService.FetchTimeFromServerCoroutine(
    ok =>
    {
        if (ok) ShowDailyReward();
    }));"
                        },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "NetworkService",
                    Namespace = "NekoLib.Services",
                    Summary = "Centralized internet connectivity check and background monitoring with a static API.",
                    Description = "FetchInternetConnectionAsync() does a one-shot check against multiple endpoints. StartMonitoring() runs periodic checks and fires OnConnectionUpdate on state changes. Always call Dispose() in OnDestroy.",
                    Code =
@"ConnectionStatus status =
    await NetworkService.FetchInternetConnectionAsync();
Debug.Log($""Online: {NetworkService.IsOnline}"");

NetworkService.StartMonitoring();
NetworkService.OnConnectionUpdate += OnConnectionChanged;

void OnConnectionChanged(ConnectionStatus s)
{
    if (s == ConnectionStatus.Online) ReconnectServices();
}

void OnDestroy() { NetworkService.Dispose(); }",
                    Tags = new[] { "Network", "Internet", "Monitor" },
                    Category = DocCategory.Services,
                    Members = new[]
                    {
                        // Properties
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "Status",
                            Summary = "Current ConnectionStatus enum value: Unknown, Online, or Offline.",
                            Code =
@"switch (NetworkService.Status)
{
    case ConnectionStatus.Online:  EnableOnlineFeatures(); break;
    case ConnectionStatus.Offline: ShowOfflineBanner();    break;
}"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Property,
                            Signature = "IsOnline",
                            Summary = "Shorthand for Status == ConnectionStatus.Online.",
                            Code =
@"submitButton.interactable = NetworkService.IsOnline;"
                        },
                        // Methods
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "FetchInternetConnectionAsync()",
                            Summary = "One-shot async check. Updates Status and returns the result.",
                            Code =
@"var status = await NetworkService.FetchInternetConnectionAsync();
if (status == ConnectionStatus.Online)
    await SyncCloudData();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "FetchInternetConnectionCoroutine(Action<bool> callback)",
                            Summary = "Coroutine variant of the one-shot check.",
                            Code =
@"StartCoroutine(NetworkService.FetchInternetConnectionCoroutine(
    s => offlineBanner.SetActive(s == ConnectionStatus.Offline)));"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "StartMonitoring()",
                            Summary = "Begins periodic background checks. Fires OnConnectionUpdate on state changes.",
                            Code =
@"NetworkService.OnConnectionUpdate += OnNetworkChanged;
NetworkService.StartMonitoring();"
                        },
                        new DocMember
                        {
                            Kind = DocMemberKind.Method,
                            Signature = "Dispose()",
                            Summary = "Stops monitoring and releases resources. Call in OnDestroy.",
                            Code =
@"void OnDestroy()
{
    NetworkService.OnConnectionUpdate -= OnNetworkChanged;
    NetworkService.Dispose();
}"
                        },
                        // Callbacks
                        new DocMember
                        {
                            Kind = DocMemberKind.Callback,
                            Signature = "OnConnectionUpdate",
                            Summary = "Fired when connectivity state changes during monitoring. Passes the new ConnectionStatus.",
                            Code =
@"void OnEnable()
{
    NetworkService.OnConnectionUpdate += HandleNetworkChange;
    NetworkService.StartMonitoring();
}

void OnDisable()
{
    NetworkService.OnConnectionUpdate -= HandleNetworkChange;
    NetworkService.StopMonitoring();
}

void HandleNetworkChange(ConnectionStatus status)
{
    offlineBanner.SetActive(status == ConnectionStatus.Offline);
}"
                        },
                    }
                },

                // ── Utilities ───────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "MouseUtils",
                    Namespace = "NekoLib.Utilities",
                    Summary = "Mouse world-position helpers for 2D and 3D, raycast shortcuts, and game-window boundary check.",
                    Description = "GetMousePosition2D works for orthographic cameras. GetMousePosition3D takes a distance parameter. GetMousePosition3DFromRaycast hits actual geometry with an optional layer mask. GetMouseRay returns the screen-to-world ray.",
                    Code =
@"bool inWindow = Utils.IsMouseInGameWindow();
Vector2 pos2D = Utils.GetMousePosition2D();
Vector3 pos3D = Utils.GetMousePosition3D(10f);
Vector3 hit3D = Utils.GetMousePosition3DFromRaycast(
    LayerMask.GetMask(""Ground""));
Ray ray = Utils.GetMouseRay();",
                    Tags = new[] { "Mouse", "Input", "Raycast" },
                    Category = DocCategory.Utilities,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsMouseInGameWindow()",
                            Summary = "Returns true when the mouse cursor is within the game window bounds.",
                            Code = @"if (!Utils.IsMouseInGameWindow()) return;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetMousePosition2D()",
                            Summary = "Returns the mouse position in 2D world space using Camera.main.",
                            Code =
@"Vector2 mouseWorld = Utils.GetMousePosition2D();
aimTarget.position = mouseWorld;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetMousePosition3D(float distance)",
                            Summary = "Returns the mouse position on a plane at the given distance from the camera.",
                            Code =
@"Vector3 pos = Utils.GetMousePosition3D(10f);
targetMarker.position = pos;" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetMousePosition3DFromRaycast(LayerMask layerMask)",
                            Summary = "Raycasts from the mouse and returns the hit point on geometry in the given layer mask.",
                            Code =
@"Vector3 ground = Utils.GetMousePosition3DFromRaycast(
    LayerMask.GetMask(""Ground""));
character.MoveTo(ground);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetMouseRay()",
                            Summary = "Returns the Ray from the camera through the current mouse position.",
                            Code =
@"Ray ray = Utils.GetMouseRay();
if (Physics.Raycast(ray, out RaycastHit hit))
    Debug.Log(hit.collider.name);" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Raycast2D / Raycast3D Utils",
                    Namespace = "NekoLib.Utilities",
                    Summary = "IsPointerOver* helpers for 2D/3D objects with optional layer mask, out-collider, and component retrieval.",
                    Description = "Overloads include: out Collider/RaycastHit; generic IsPointerOver2DObject<T>/IsPointerOver3DObject<T> for direct component access. UIElementUtils.IsPointerOverUI checks UI layers.",
                    Code =
@"bool over2D = Utils.IsPointerOverAny2DObject();
bool over   = Utils.IsPointerOverAny2DObject(
    LayerMask.GetMask(""Interactive""));
bool hit    = Utils.IsPointerOverAny2DObject(out Collider2D col);
bool hasBtn = Utils.IsPointerOver2DObject<Button>(out Button btn);

bool over3D = Utils.IsPointerOverAny3DObject();
bool hit3D  = Utils.IsPointerOverAny3DObject(out RaycastHit rh);
bool hasI   = Utils.IsPointerOver3DObject<Interactable>(
    out Interactable i);

bool overUI = Utils.IsPointerOverUI(LayerMask.GetMask(""UI""));",
                    Tags = new[] { "Raycast", "Input", "2D", "3D" },
                    Category = DocCategory.Utilities,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOverAny2DObject()",
                            Summary = "Returns true if the mouse is over any 2D collider.",
                            Code = @"if (Utils.IsPointerOverAny2DObject()) HandleClick();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOverAny2DObject(LayerMask layerMask)",
                            Summary = "Returns true if the mouse is over a 2D collider on the specified layers.",
                            Code = @"bool hit = Utils.IsPointerOverAny2DObject(LayerMask.GetMask(""Enemy""));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOverAny2DObject(out Collider2D)",
                            Summary = "Returns true and outputs the first hit 2D collider.",
                            Code =
@"if (Utils.IsPointerOverAny2DObject(out Collider2D col))
    col.GetComponent<Enemy>()?.TakeDamage();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOver2DObject<T>(out T)",
                            Summary = "Returns true if the mouse hits a 2D collider with component T, and outputs it.",
                            Code =
@"if (Utils.IsPointerOver2DObject<Enemy>(out Enemy e))
    e.ShowTooltip();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOverAny3DObject()",
                            Summary = "Returns true if the mouse ray hits any 3D collider.",
                            Code = @"if (Utils.IsPointerOverAny3DObject()) ShowCursor(CursorType.Hand);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOver3DObject<T>(out T)",
                            Summary = "Returns true if the mouse ray hits a 3D collider with component T, and outputs it.",
                            Code =
@"if (Utils.IsPointerOver3DObject<Interactable>(out var obj))
    obj.Highlight();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsPointerOverUI(LayerMask layerMask)",
                            Summary = "Returns true if the pointer is currently over a UI element on the given layer mask.",
                            Code = @"if (Utils.IsPointerOverUI(LayerMask.GetMask(""UI""))) return;" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "EnumUtils",
                    Namespace = "NekoLib.Utilities",
                    Summary = "GetRandomEnum, AllEnum, CountEnum, ForEnum — generic enum helpers without boilerplate.",
                    Description = "GetRandomEnum can exclude specific values. AllEnum returns all values as an array with optional exclusions. ForEnum is a foreach substitute that takes an action.",
                    Code =
@"MyEnum rand  = Utils.GetRandomEnum<MyEnum>();
MyEnum randX = Utils.GetRandomEnum(MyEnum.None, MyEnum.Default);

int      count = Utils.CountEnum<MyEnum>();
MyEnum[] all   = Utils.AllEnum<MyEnum>();
MyEnum[] some  = Utils.AllEnum(MyEnum.None);

Utils.ForEnum<MyEnum>(v => Debug.Log(v));",
                    Tags = new[] { "Enum", "Random", "Iteration" },
                    Category = DocCategory.Utilities,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetRandomEnum<T>()",
                            Summary = "Returns a random value of the enum type T.",
                            Code = @"WeaponType wt = Utils.GetRandomEnum<WeaponType>();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetRandomEnum<T>(params T[] exclude)",
                            Summary = "Returns a random enum value, excluding the specified values.",
                            Code = @"Element elem = Utils.GetRandomEnum(Element.None, Element.Null);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "AllEnum<T>(params T[] exclude)",
                            Summary = "Returns all enum values as a T[] array, optionally excluding specified values.",
                            Code =
@"GameState[] active = Utils.AllEnum(GameState.None, GameState.Error);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "CountEnum<T>()",
                            Summary = "Returns the total number of values defined in the enum.",
                            Code = @"int count = Utils.CountEnum<Direction>(); // 4" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "ForEnum<T>(Action<T>)",
                            Summary = "Iterates over all enum values and invokes the action for each.",
                            Code =
@"Utils.ForEnum<Direction>(dir =>
{
    var btn = Instantiate(directionButton);
    btn.Setup(dir);
});" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "TransformUtils / TimeUtils",
                    Namespace = "NekoLib.Utilities",
                    Summary = "GetAngleFromVector, GetRandomRotation, and cached WaitForSeconds allocation helpers.",
                    Description = "GetRandomRotation accepts an Axis enum and optional per-axis Vector2 range. TimeUtils.GetWaitForSeconds caches WaitForSeconds instances to avoid per-frame GC allocation in coroutines.",
                    Code =
@"float angle    = Utils.GetAngleFromVector(direction);
Quaternion rY  = Utils.GetRandomRotation(Axis.Y);
Quaternion rXY = Utils.GetRandomRotation(
    Axis.XY,
    new Vector2(0f, 90f),
    new Vector2(-45f, 45f));

// Cached waits — avoids GC alloc
yield return Utils.GetWaitForSeconds(1.5f);
yield return Utils.GetWaitForSecondsRealtime(2f);",
                    Tags = new[] { "Transform", "Rotation", "Coroutine" },
                    Category = DocCategory.Utilities,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetAngleFromVector(Vector2 direction)",
                            Summary = "Returns the angle in degrees of a direction vector relative to Vector2.right.",
                            Code =
@"float angle = Utils.GetAngleFromVector(moveDir);
transform.rotation = Quaternion.Euler(0, 0, angle);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetRandomRotation(Vector3 axis)",
                            Summary = "Returns a random rotation around the specified axis (or axes) using default 0–360 range.",
                            Code =
@"transform.rotation = Utils.GetRandomRotation(Axis.Y);   // random Y spin
transform.rotation = Utils.GetRandomRotation(Axis.XYZ); // fully random" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetRandomRotation(Vector3 axis, Vector2 xRange, Vector2 yRange)",
                            Summary = "Returns a random rotation with per-axis angle ranges specified as Vector2(min, max).",
                            Code =
@"Quaternion r = Utils.GetRandomRotation(
    Axis.XY,
    new Vector2(-30f, 30f),
    new Vector2(-30f, 30f));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetWaitForSeconds(float duration)",
                            Summary = "Returns a cached WaitForSeconds to avoid GC allocation inside coroutines.",
                            Code =
@"IEnumerator Reload()
{
    yield return Utils.GetWaitForSeconds(reloadTime);
    isReloaded = true;
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetWaitForSecondsRealtime(float duration)",
                            Summary = "Returns a cached WaitForSecondsRealtime (unscaled time) to avoid GC allocation.",
                            Code = @"yield return Utils.GetWaitForSecondsRealtime(2f);" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "EventUtils (UnityEvent aliases)",
                    Namespace = "NekoLib.Utilities",
                    Summary = "Serializable UnityEvent subclasses for common types: FloatEvent, IntEvent, StringEvent, BoolEvent.",
                    Description = "These typed event aliases save repetitive UnityEvent<float> etc. declarations and are serializable (visible in the inspector). Drop-in replacements for UnityEvent<T>.",
                    Code =
@"[SerializeField] private FloatEvent  onValueChanged;
[SerializeField] private IntEvent    onCountChanged;
[SerializeField] private StringEvent onTextChanged;
[SerializeField] private BoolEvent   onToggled;

onValueChanged.Invoke(0.5f);
onCountChanged.Invoke(3);
onTextChanged.Invoke(""Hello"");
onToggled.Invoke(true);",
                    Tags = new[] { "Event", "UnityEvent", "Serializable" },
                    Category = DocCategory.Utilities,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "FloatEvent",
                            Summary = "Serializable UnityEvent<float>. Visible in the inspector like a standard UnityEvent.",
                            Code =
@"[SerializeField] private FloatEvent onHealthChanged;

// Assign in inspector or in code:
onHealthChanged.AddListener(v => healthBar.fillAmount = v);
onHealthChanged.Invoke(currentHealth / maxHealth);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IntEvent",
                            Summary = "Serializable UnityEvent<int>.",
                            Code =
@"[SerializeField] private IntEvent onScoreChanged;
onScoreChanged.Invoke(score);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StringEvent",
                            Summary = "Serializable UnityEvent<string>.",
                            Code =
@"[SerializeField] private StringEvent onDialogueLine;
onDialogueLine.Invoke(""Hello, adventurer!"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "BoolEvent",
                            Summary = "Serializable UnityEvent<bool>.",
                            Code =
@"[SerializeField] private BoolEvent onPauseToggled;
onPauseToggled.Invoke(isPaused);" },
                    }
                },

                // ── Editor Tools ─────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "Timer Tracker Window",
                    Namespace = "Window > Neko Framework > Timer Tracker",
                    Summary = "Live inspector for all active Countdowns and Stopwatches. Progress bars, memory stats, pagination.",
                    Description = "Two tabs: Countdowns and Stopwatches. Each row shows source GameObject, component name, elapsed/remaining time, and a smooth progress bar. Stats bar displays IsAlive count, Capacity, Free Slots, and Slot Memory via Unsafe.SizeOf. Paginated at 20 items. Clears on exiting Play mode.",
                    Code =
@"// Open: Window > Neko Framework > Timer Tracker

// Timers appear automatically when created via Countdown/Stopwatch:
var countdown = Countdown.Create(this, 10f).OnStop(() => { });
countdown.Start();  // now visible in Timer Tracker",
                    Tags = new[] { "Editor", "Debug", "Timer" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "Scene Switcher (Toolbar)",
                    Namespace = "Edit > Project Settings > NekoLib",
                    Summary = "Toolbar dropdown listing all Build Settings scenes. Right-click to set a Startup Scene for Play mode.",
                    Description = "Lists scenes by name; duplicates disambiguated with their paths. Right-click any entry to mark it as the Startup Scene — Play mode auto-loads it then restores the original on exit. Setting: Activate Loaded Additive On Select reactivates an already-loaded additive scene instead of reopening it.",
                    Code =
@"// Settings: Edit > Project Settings > NekoLib
// Startup Scene Path
//   — auto-loaded when entering Play mode
// Activate Loaded Additive On Select
//   — reactivate an existing additive scene instead of reloading it",
                    Tags = new[] { "Editor", "Scene", "Toolbar" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "Time Scale Tool (Toolbar)",
                    Namespace = "Edit > Project Settings > NekoLib",
                    Summary = "Toolbar slider (0–10) for adjusting Time.timeScale in real time. Auto-restores to 1.0 on exiting Play mode.",
                    Description = "Bidirectionally synced with Project Settings > Time Manager. Reset button snaps to 1.0. Upper bound configurable via Time Scale Max in NekoLib settings (default 10).",
                    Code =
@"// No code needed. Slider appears in the Unity toolbar during Play mode.
// Settings: Edit > Project Settings > NekoLib
//   Time Scale Max — upper bound of the slider (default 10)",
                    Tags = new[] { "Editor", "TimeScale", "Toolbar" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "Clear PlayerPrefs (Toolbar)",
                    Namespace = "Edit > Project Settings > NekoLib",
                    Summary = "One-click toolbar button to clear all PlayerPrefs with a confirmation dialog.",
                    Description = "Shows a confirmation dialog before deletion. In Play mode: exits, clears PlayerPrefs, then optionally re-enters Play mode (controlled by Auto Re-enter Play After Clear setting).",
                    Code =
@"// No code needed. Toolbar button with confirmation dialog.
// Settings: Edit > Project Settings > NekoLib
//   Auto Re-enter Play After Clear — default true",
                    Tags = new[] { "Editor", "PlayerPrefs", "Toolbar" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "Game Screenshot Tool",
                    Namespace = "Tools > Neko Framework > Screenshot",
                    Summary = "In-editor screenshot capture. GameView or SpecificCamera mode. Supersize multiplier up to 4×.",
                    Description = "Capture modes: GameView (resolution × supersize) or SpecificCamera (offscreen — no Screen Space Overlay UI). Supersize 2× on 1080p → 4K output. Shortcut: Ctrl+Shift+K. Output: {SceneName}_{Width}x{Height}_{Timestamp}.png. Requires Play mode.",
                    Code =
@"// Menu: Tools > Neko Framework > Screenshot > Open Settings
// Shortcut: Ctrl+Shift+K (quick capture)
// Supersize table (1080p GameView):
//   1x = 1920x1080
//   2x = 3840x2160  (4K)
//   3x = 5760x3240
//   4x = 7680x4320  (8K)",
                    Tags = new[] { "Editor", "Screenshot", "Capture" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "Setup Window",
                    Namespace = "Window > Neko Framework > Setup",
                    Summary = "One-time project setup wizard: create standard folder structure and install pre-defined packages.",
                    Description = "Tabs: Folders (creates project folder tree from SetupFoldersSettings ScriptableObject) and Packages (installs a pre-defined list from SetupPackagesSettings). Both settings are serialized ScriptableObjects customizable per team.",
                    Code =
@"// Menu: Window > Neko Framework > Setup
// Tabs:
//   Folders  — creates project folder structure
//   Packages — installs Unity packages from a pre-defined list
// Both tabs are driven by ScriptableObject settings assets.",
                    Tags = new[] { "Editor", "Setup", "Wizard" },
                    Category = DocCategory.EditorTools
                },
                new NekoLibDocEntry
                {
                    Title = "NekoLib Project Settings",
                    Namespace = "Edit > Project Settings > NekoLib",
                    Summary = "Central settings for all NekoLib editor tools. Auto-created at Assets/Plugins/NekoLib/Editor/NekoLibSettings.asset.",
                    Description = "Available settings: Startup Scene Path, Activate Loaded Additive On Select, Time Scale Max (default 10), Hide Toolbar, Auto Re-enter Play After Clear (default true). Persisted as a ScriptableObject asset.",
                    Code =
@"// Edit > Project Settings > NekoLib
// Asset: Assets/Plugins/NekoLib/Editor/NekoLibSettings.asset
// Startup Scene Path         — auto-load on entering Play mode
// Time Scale Max             — toolbar slider upper bound (default 10)
// Hide Toolbar               — hide all NekoLib toolbar elements
// Auto Re-enter After Clear  — re-enter Play after clearing prefs",
                    Tags = new[] { "Editor", "Settings", "Configuration" },
                    Category = DocCategory.EditorTools
                },

                // ── NekoSignal ────────────────────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "NekoSignal — Installation",
                    Namespace = "Package: unity-neko-signal",
                    Summary = "NekoSignal is a separate package and must be installed via UPM before use.",
                    Description = "NekoSignal is NOT included in NekoLib. Install NekoLib first, then add NekoSignal via Unity Package Manager → Add package from git URL.",
                    Code =
@"// 1. Install NekoLib first
https://github.com/boobosua/unity-nekolib.git

// 2. Then install NekoSignal
https://github.com/boobosua/unity-neko-signal.git",
                    Tags = new[] { "Install", "UPM", "Setup" },
                    Category = DocCategory.NekoSignal
                },
                new NekoLibDocEntry
                {
                    Title = "ISignal",
                    Namespace = "NekoSignal",
                    Summary = "Marker interface all signal structs must implement. Enforces struct constraint — null payload is impossible by design.",
                    Description = "Signals must be structs (readonly struct recommended for immutability). The subscribe and emit generics enforce `where T : struct, ISignal`, so signals are always stack-allocated value types.",
                    Code =
@"using NekoSignal;

public readonly struct PlayerDied : ISignal { }

public readonly struct PlayerHealthChanged : ISignal
{
    public readonly int NewHealth;
    public readonly int MaxHealth;

    public PlayerHealthChanged(int newHealth, int maxHealth)
    {
        NewHealth = newHealth;
        MaxHealth = maxHealth;
    }
}",
                    Tags = new[] { "Signal", "Struct", "Interface" },
                    Category = DocCategory.NekoSignal
                },
                new NekoLibDocEntry
                {
                    Title = "[OnSignal] + SignalHub.Bind",
                    Namespace = "NekoSignal",
                    Summary = "Attribute-based subscription. Decorate handler methods with [OnSignal] and call SignalHub.Bind(this) once — no manual wiring.",
                    Description = "NekoSignal discovers all [OnSignal]-decorated methods via reflection at bind time only (zero reflection on emit). Always pair Bind in OnEnable with Unbind in OnDisable. Forgetting Unbind leaks the delegate — the Memory Leaks tab in Signal Tracker will surface it.",
                    Code =
@"using NekoSignal;

public class UIHealthBar : MonoBehaviour
{
    private void OnEnable()  => SignalHub.Bind(this);
    private void OnDisable() => SignalHub.Unbind(this);

    [OnSignal]
    private void OnHealthChanged(PlayerHealthChanged s)
    {
        healthBar.fillAmount = (float)s.NewHealth / s.MaxHealth;
    }

    // Priority — higher runs first (default 0)
    [OnSignal(priority: 10)]
    private void OnHealthChangedEarly(PlayerHealthChanged s) { }
}",
                    Tags = new[] { "Subscribe", "Attribute", "Bind" },
                    Category = DocCategory.NekoSignal,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SignalHub.Bind(MonoBehaviour owner)",
                            Summary = "Scans owner for all [OnSignal]-decorated methods and registers them. Call in OnEnable.",
                            Code =
@"private void OnEnable()  => SignalHub.Bind(this);
private void OnDisable() => SignalHub.Unbind(this);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SignalHub.Unbind(MonoBehaviour owner)",
                            Summary = "Unregisters all [OnSignal] handlers for this owner. Call in OnDisable to prevent memory leaks.",
                            Code = @"private void OnDisable() => SignalHub.Unbind(this);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "[OnSignal]",
                            Summary = "Attribute that marks a method as a signal handler. The parameter type determines which signal it receives.",
                            Code =
@"[OnSignal]
private void OnPlayerDied(PlayerDied s)
{
    ShowGameOverScreen();
}

// With priority — higher values run first (default 0)
[OnSignal(priority: 10)]
private void HandleEarlyResponse(PlayerDied s) { }" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "SignalBus.Emit / this.Emit",
                    Namespace = "NekoSignal",
                    Summary = "Emit a signal to all subscribers. Use this.Emit() from a MonoBehaviour to record the emitter in the Signal Tracker.",
                    Description = "Both overloads are equivalent in dispatch behavior. `this.Emit()` attaches the emitting MonoBehaviour as context for the Signal Tracker log. `SignalBus.Emit()` is usable from any non-MonoBehaviour context.",
                    Code =
@"using NekoSignal;

// From a MonoBehaviour — emitter recorded in Signal Tracker
this.Emit(new PlayerHealthChanged(health, maxHealth));

// From anywhere — no emitter context
SignalBus.Emit(new PlayerHealthChanged(health, maxHealth));",
                    Tags = new[] { "Emit", "Dispatch" },
                    Category = DocCategory.NekoSignal,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "this.Emit<T>(T signal)",
                            Summary = "Emits a signal from a MonoBehaviour. The emitter is recorded in Signal Tracker.",
                            Code =
@"// From a MonoBehaviour — emitter visible in Signal Tracker
this.Emit(new PlayerHealthChanged(health, maxHealth));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SignalBus.Emit<T>(T signal)",
                            Summary = "Emits a signal from any context. No emitter is recorded.",
                            Code =
@"// From anywhere — no MonoBehaviour context
SignalBus.Emit(new PlayerHealthChanged(health, maxHealth));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "this.Emit<T>(T signal, ISignalFilter[] filters)",
                            Summary = "Emits with filters. Only subscribers whose owner passes all filters receive the signal.",
                            Code =
@"private ISignalFilter[] _filters;
private void Awake() => _filters = new ISignalFilter[] { new WithTag(""Player"") };

private void Update()
{
    if (detected)
        this.Emit(new EnemyDetected(target), _filters);
}" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Listen<T> / SignalReceiver",
                    Namespace = "NekoSignal",
                    Summary = "Manual subscription returning a SignalReceiver. Call Dispose() to unsubscribe. Use when you need conditional or lifetime-limited subscriptions.",
                    Description = "Listen<T>() returns a SignalReceiver. Call Dispose() to unsubscribe at any time — it is idempotent. SignalReceiver.IsActive is true until Dispose() is called. Abandoned receivers are auto-cleaned when the owner MonoBehaviour is destroyed, but explicit disposal is cleaner.",
                    Code =
@"using NekoSignal;

public class TemporaryListener : MonoBehaviour
{
    private SignalReceiver _receiver;

    private void OnEnable()
    {
        _receiver = this.Listen<GameStarted>(OnGameStarted);
    }

    private void OnDisable()
    {
        _receiver.Dispose();
    }

    private void OnGameStarted(GameStarted s) { }
}

// From anywhere
var rx = SignalBus.Listen<PlayerDied>(owner, OnPlayerDied);
rx.Dispose(); // unsubscribe",
                    Tags = new[] { "Subscribe", "Listen", "Receiver", "Dispose" },
                    Category = DocCategory.NekoSignal,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "this.Listen<T>(Action<T> handler)",
                            Summary = "Subscribes to signal T from a MonoBehaviour. Returns a SignalReceiver.",
                            Code =
@"private SignalReceiver _rx;

private void OnEnable()  => _rx = this.Listen<GameStarted>(OnGameStarted);
private void OnDisable() => _rx.Dispose();

private void OnGameStarted(GameStarted s) { }" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SignalBus.Listen<T>(MonoBehaviour owner, Action<T> handler)",
                            Summary = "Subscribes from any context. Owner is used for tracker display and leak detection.",
                            Code =
@"var rx = SignalBus.Listen<PlayerDied>(this, OnPlayerDied);
rx.Dispose(); // unsubscribe later" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "receiver.IsActive",
                            Summary = "True while the receiver is live. Becomes false after Dispose() is called.",
                            Code =
@"if (_rx.IsActive)
    Debug.Log(""Still listening"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "receiver.Dispose()",
                            Summary = "Unsubscribes and marks the receiver inactive. Safe to call multiple times.",
                            Code =
@"_rx.Dispose(); // idempotent — safe to call again" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "ISignalFilter + Filtered Emit",
                    Namespace = "NekoSignal",
                    Summary = "Emitter-side filters restrict delivery to subscribers whose MonoBehaviour owner passes all provided filters.",
                    Description = "Three built-in filters: HasComponent<T>, InLayer, WithTag. Custom filters implement ISignalFilter.Evaluate(MonoBehaviour owner). For signals emitted every frame, pre-allocate the filter array in Awake to avoid params allocation.",
                    Code =
@"using NekoSignal;

// Fluent one-off
new EnemySpotted(target)
    .ConfigureFilters()
    .Require(new HasComponent<Rigidbody>())
    .Require(new WithTag(""Player""))
    .Emit();

// Inline (allocates params array)
this.Emit(new EnemySpotted(target), new InLayer(LayerMask.GetMask(""Enemy"")));

// Pre-allocated (no allocation on hot path)
private ISignalFilter[] _filters;
private void Awake() => _filters = new ISignalFilter[] { new WithTag(""Player"") };
private void Update()
{
    if (detected)
        SignalBus.Emit(new EnemyDetected(target), _filters);
}

// Custom filter
public sealed class TeamFilter : ISignalFilter
{
    private readonly int _teamId;
    public TeamFilter(int teamId) => _teamId = teamId;
    public bool Evaluate(MonoBehaviour owner)
        => owner.TryGetComponent<TeamMember>(out var m) && m.TeamId == _teamId;
}",
                    Tags = new[] { "Filter", "ISignalFilter", "HasComponent", "InLayer", "WithTag" },
                    Category = DocCategory.NekoSignal,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "signal.ConfigureFilters()",
                            Summary = "Starts a fluent filter chain on the signal. Returns a FilteredEmitBuilder.",
                            Code =
@"new EnemySpotted(target)
    .ConfigureFilters()
    .Require(new HasComponent<Rigidbody>())
    .Emit();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "builder.Require(ISignalFilter filter)",
                            Summary = "Adds a filter to the builder. All filters must pass for a subscriber to receive the signal.",
                            Code =
@"signal.ConfigureFilters()
    .Require(new WithTag(""Player""))
    .Require(new InLayer(LayerMask.GetMask(""Characters"")))
    .Emit();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "builder.Emit()",
                            Summary = "Dispatches the signal with all configured filters applied.",
                            Code =
@"new DamageDealt(damage)
    .ConfigureFilters()
    .Require(new HasComponent<IDamageable>())
    .Emit();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "HasComponent<T>()",
                            Summary = "Built-in filter: subscriber's owner MonoBehaviour must have component T.",
                            Code =
@"this.Emit(new Signal(), new HasComponent<Shield>());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "InLayer(LayerMask mask)",
                            Summary = "Built-in filter: subscriber's owner must be in the given LayerMask.",
                            Code =
@"this.Emit(new Signal(), new InLayer(LayerMask.GetMask(""Enemy"")));" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "WithTag(string tag)",
                            Summary = "Built-in filter: subscriber's owner must have the matching tag.",
                            Code =
@"this.Emit(new Signal(), new WithTag(""Player""));" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "Signal Tracker",
                    Namespace = "Window > Neko Framework > Signal Tracker",
                    Summary = "Editor window with live subscription monitor, emit log, and memory leak detector.",
                    Description = "Three tabs:\n• Subscription Monitor — live subscriber table per signal type (GameObject, component, method, priority). Searchable.\n• Signal Log — emit history with emitter context, timestamp, payload fields, and applied filters.\n• Memory Leaks — MonoBehaviours that called SignalHub.Bind but were destroyed without Unbind. Cleared automatically on exiting Play Mode.",
                    Tags = new[] { "Editor", "Debug", "Tracker", "Memory" },
                    Category = DocCategory.NekoSignal
                },

                // ── NekoFlow ──────────────────────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "NekoFlow — Installation",
                    Namespace = "Package: unity-neko-flow",
                    Summary = "NekoFlow is a separate package and must be installed via UPM before use.",
                    Description = "NekoFlow is NOT included in NekoLib. Install NekoLib first, then add NekoFlow via Unity Package Manager → Add package from git URL.",
                    Code =
@"// 1. Install NekoLib first
https://github.com/boobosua/unity-nekolib.git

// 2. Then install NekoFlow
https://github.com/boobosua/unity-neko-flow.git",
                    Tags = new[] { "Install", "UPM", "Setup" },
                    Category = DocCategory.NekoFlow
                },
                new NekoLibDocEntry
                {
                    Title = "StateBehaviour",
                    Namespace = "NekoFlow.FSM",
                    Summary = "MonoBehaviour-based FSM controller. Derive from it, create states, then declare transitions in Awake.",
                    Description = "StateBehaviour is the 'brain' — it owns transition predicates and state instances. Use GetTimeInCurrentState() for time-based transitions. API: IsInState<T>(), TryGetCurrentState<T>(out T), GetTimeInCurrentState().",
                    Code =
@"using NekoFlow.FSM;

public class EnemyController : StateBehaviour
{
    private EnemyIdleState _idle;
    private EnemyPatrolState _patrol;

    private void Awake()
    {
        _idle   = new EnemyIdleState(this);
        _patrol = new EnemyPatrolState(this);

        this.StartWith(_idle)
            .At(_idle,   _patrol, () => GetTimeInCurrentState() >= 2f)
            .At(_patrol, _idle,   () => GetTimeInCurrentState() >= 5f)
            .Any(_patrol, () => IsAlerted()); // any-state transition
    }
}",
                    Tags = new[] { "FSM", "StateMachine", "Controller" },
                    Category = DocCategory.NekoFlow,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartWith(IState state)",
                            Summary = "Sets the initial state and activates it. Call once in Awake.",
                            Code =
@"private void Awake()
{
    _idle = new IdleState(this);
    this.StartWith(_idle);
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "At(IState from, IState to, Func<bool> predicate)",
                            Summary = "Registers a transition from one state to another when predicate returns true.",
                            Code =
@"this.StartWith(_idle)
    .At(_idle, _patrol, () => GetTimeInCurrentState() >= 2f)
    .At(_patrol, _idle, () => !enemyVisible);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Any(IState to, Func<bool> predicate)",
                            Summary = "Registers a global transition that fires from any state when predicate is true.",
                            Code =
@".Any(_dead, () => health <= 0); // triggers from idle, patrol, or any other state" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "IsInState<T>()",
                            Summary = "Returns true if the machine is currently in state T.",
                            Code =
@"if (IsInState<EnemyIdleState>())
    Debug.Log(""Enemy is idle"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "TryGetCurrentState<T>(out T state)",
                            Summary = "Tries to cast the current state to T. Returns false if current state is a different type.",
                            Code =
@"if (TryGetCurrentState<PatrolState>(out var patrol))
    patrol.SetWaypoint(nextPoint);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "GetTimeInCurrentState()",
                            Summary = "Returns elapsed seconds since the last state transition. Useful for timed transitions.",
                            Code =
@".At(_idle, _patrol, () => GetTimeInCurrentState() >= 3f)" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "BaseState<TContext>",
                    Namespace = "NekoFlow.FSM",
                    Summary = "Convenience base class for states. Provides protected _context, _gameObject, and _transform.",
                    Description = "Implement IState directly for pure C# states, or inherit BaseState<T> when you need convenient access to the controller and its GameObject. State logic only — no transition predicates here.",
                    Code =
@"using NekoFlow.FSM;

public sealed class EnemyIdleState : BaseState<EnemyController>
{
    public EnemyIdleState(EnemyController context) : base(context) { }

    public override void OnEnter() { /* start idle anim */ }

    public override void OnTick(float deltaTime)
    {
        // State behavior only — transitions live in the controller
    }

    public override void OnExit() { /* cleanup */ }
}",
                    Tags = new[] { "FSM", "State", "BaseState" },
                    Category = DocCategory.NekoFlow,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Property, Signature = "_context",
                            Summary = "Reference to the owning TContext controller. Available inside any state method.",
                            Code =
@"public override void OnTick(float deltaTime)
{
    _context.Animator.SetFloat(""Speed"", _context.Speed);
}" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "_gameObject / _transform",
                            Summary = "Shortcuts to _context.gameObject and _context.transform.",
                            Code =
@"public override void OnEnter()
{
    _transform.position = spawnPoint;
}" },
                        new DocMember { Kind = DocMemberKind.Callback, Signature = "OnEnter()",
                            Summary = "Called when this state becomes active. Set up animations, flags, subscriptions here.",
                            Code =
@"public override void OnEnter()
{
    _context.Animator.SetTrigger(""Idle"");
}" },
                        new DocMember { Kind = DocMemberKind.Callback, Signature = "OnTick(float deltaTime)",
                            Summary = "Called every frame while this state is active. State behavior only — no transition logic here.",
                            Code =
@"public override void OnTick(float deltaTime)
{
    _context.MoveTowardsTarget(deltaTime);
}" },
                        new DocMember { Kind = DocMemberKind.Callback, Signature = "OnExit()",
                            Summary = "Called when this state is deactivated. Clean up anything started in OnEnter.",
                            Code =
@"public override void OnExit()
{
    _context.Animator.ResetTrigger(""Idle"");
}" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "StateMachine (pure C#)",
                    Namespace = "NekoFlow.FSM",
                    Summary = "Standalone state machine, no MonoBehaviour required. Tick it manually in Update.",
                    Description = "Use StateMachine directly when you want FSM logic inside a plain C# class. Remember to call Tick(deltaTime) each frame. API: StartWith, At, Any, SetState, CurrentState, TimeInState, Is<T>(), Get<T>().",
                    Code =
@"using NekoFlow.FSM;

public class EnemyBrain : MonoBehaviour
{
    private StateMachine _sm;

    private void Awake()
    {
        _sm = new StateMachine();
        var idle   = new IdleState();
        var patrol = new PatrolState();

        _sm.StartWith(idle)
            .At(idle,   patrol, () => _sm.TimeInState >= 1f)
            .At(patrol, idle,   () => _sm.TimeInState >= 3f);
    }

    private void Update() => _sm?.Tick(Time.deltaTime);
}",
                    Tags = new[] { "FSM", "StateMachine", "PureCSharp" },
                    Category = DocCategory.NekoFlow,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Property, Signature = "CurrentState",
                            Summary = "The currently active IState instance.",
                            Code =
@"Debug.Log(_sm.CurrentState?.GetType().Name);" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "TimeInState",
                            Summary = "Elapsed seconds since the last state transition.",
                            Code =
@".At(idle, patrol, () => _sm.TimeInState >= 2f)" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "StartWith(IState state)",
                            Summary = "Sets the initial state. Call once before the first Tick.",
                            Code =
@"_sm = new StateMachine();
_sm.StartWith(idle);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "At(IState from, IState to, Func<bool> predicate)",
                            Summary = "Registers a transition from one state to another.",
                            Code =
@"_sm.At(idle, patrol, () => _sm.TimeInState >= 1f)
   .At(patrol, idle, () => _sm.TimeInState >= 3f);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Any(IState to, Func<bool> predicate)",
                            Summary = "Registers a global transition that fires from any state.",
                            Code =
@"_sm.Any(dead, () => hp <= 0);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "SetState(IState state)",
                            Summary = "Forces an immediate transition to the given state, bypassing predicates.",
                            Code =
@"_sm.SetState(stunned);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Tick(float deltaTime)",
                            Summary = "Evaluates all transitions and ticks the current state. Call every frame.",
                            Code =
@"private void Update() => _sm?.Tick(Time.deltaTime);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Is<T>()",
                            Summary = "Returns true if the current state is of type T.",
                            Code =
@"if (_sm.Is<PatrolState>())
    DrawDebugPath();" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Get<T>()",
                            Summary = "Returns the current state cast to T, or null if the type doesn't match.",
                            Code =
@"var patrol = _sm.Get<PatrolState>();
patrol?.SetNextWaypoint(wp);" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "SimpleFlow",
                    Namespace = "NekoFlow.Conditional",
                    Summary = "Run one action when a predicate is true, optionally another when false. Returns bool.",
                    Description = "SimpleFlow is a standalone helper — not tied to the state machine. Import NekoFlow.Conditional. Execute() returns true if the predicate matched.",
                    Code =
@"using NekoFlow.Conditional;

var flow = new SimpleFlow(
    predicate: () => Time.timeScale > 0,
    onSuccess: () => Debug.Log(""Running""),
    onFailure: () => Debug.Log(""Paused"")
);

bool ran = flow.Execute();",
                    Tags = new[] { "Flow", "Conditional", "SimpleFlow" },
                    Category = DocCategory.NekoFlow,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "new SimpleFlow(Func<bool> predicate, Action onSuccess, Action onFailure)",
                            Summary = "Creates a flow with a predicate and two optional callbacks.",
                            Code =
@"var flow = new SimpleFlow(
    predicate:  () => Time.timeScale > 0,
    onSuccess:  () => Debug.Log(""Running""),
    onFailure:  () => Debug.Log(""Paused"")
);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Execute()",
                            Summary = "Evaluates the predicate and runs onSuccess or onFailure. Returns true if the predicate passed.",
                            Code =
@"bool ran = flow.Execute();
if (!ran) Debug.Log(""Predicate was false"");" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "BranchFlow",
                    Namespace = "NekoFlow.Conditional",
                    Summary = "Try branches in order; execute the first match. Returns FlowResult: Matched, Fallback, or None.",
                    Description = "BranchFlow evaluates When() branches in registration order and executes the first matching one. Otherwise() runs when no branch matches. Clear() resets all branches. Reusable across frames.",
                    Code =
@"using NekoFlow.Conditional;

var flow = new BranchFlow()
    .When(() => Input.GetKey(KeyCode.Space),     () => Debug.Log(""Jump""))
    .When(() => Input.GetKey(KeyCode.LeftArrow), () => Debug.Log(""Left""))
    .Otherwise(() => Debug.Log(""Idle""));

FlowResult result = flow.Execute();
// FlowResult.Matched  — a When() branch ran
// FlowResult.Fallback — Otherwise() ran
// FlowResult.None     — nothing matched, no fallback

flow.Clear(); // reset all branches",
                    Tags = new[] { "Flow", "Conditional", "BranchFlow" },
                    Category = DocCategory.NekoFlow,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "When(Func<bool> predicate, Action action)",
                            Summary = "Adds a conditional branch. Branches are checked in registration order.",
                            Code =
@"var flow = new BranchFlow()
    .When(() => Input.GetKey(KeyCode.Space),     () => Jump())
    .When(() => Input.GetKey(KeyCode.LeftArrow), () => MoveLeft());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Otherwise(Action action)",
                            Summary = "Sets the fallback action run when no When() branch matches.",
                            Code =
@".Otherwise(() => Idle());" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Execute()",
                            Summary = "Evaluates branches in order. Returns FlowResult.Matched, Fallback, or None.",
                            Code =
@"FlowResult result = flow.Execute();
// FlowResult.Matched  — a When() branch ran
// FlowResult.Fallback — Otherwise() ran
// FlowResult.None     — nothing matched, no fallback" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "Clear()",
                            Summary = "Removes all registered branches and the fallback.",
                            Code =
@"flow.Clear(); // reset and rebuild branches for next phase" },
                    }
                },

                // ── NekoSerializer ────────────────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "NekoSerializer — Installation",
                    Namespace = "Package: unity-neko-serializer",
                    Summary = "NekoSerializer is a separate package and must be installed via UPM before use.",
                    Description = "NekoSerializer is NOT included in NekoLib. Requires Unity 2021+ (Newtonsoft.Json ships by default). Install NekoLib first, then add NekoSerializer via Unity Package Manager → Add package from git URL.",
                    Code =
@"// 1. Install NekoLib first
https://github.com/boobosua/unity-nekolib.git

// 2. Then install NekoSerializer
https://github.com/boobosua/unity-neko-serializer.git",
                    Tags = new[] { "Install", "UPM", "Setup" },
                    Category = DocCategory.NekoSerializer
                },
                new NekoLibDocEntry
                {
                    Title = "NSR.Save / NSR.Load",
                    Namespace = "NekoSerializer",
                    Summary = "Core save/load API. Writes to the configured storage backend (PlayerPrefs or JSON file). Load returns defaultValue when the key is missing.",
                    Description = "All public API lives on the static NSR class. Unity value types (Vector2/3, Quaternion, Color, Rect, Bounds, Transform snapshot, etc.) are serialized automatically via Newtonsoft.Json converters.",
                    Code =
@"using NekoSerializer;

// Save
NSR.Save(""playerName"", ""Neko"");
NSR.Save(""score"", 9001);
NSR.Save(""position"", transform.position);

// Load — second arg is the default if key is missing
string name = NSR.Load<string>(""playerName"", ""Unknown"");
int    score = NSR.Load<int>(""score"", 0);
Vector3 pos  = NSR.Load<Vector3>(""position"");

// Key checks & deletion
if (NSR.Exists(""highScore""))
    NSR.Delete(""highScore"");",
                    Tags = new[] { "Save", "Load", "Persistence" },
                    Category = DocCategory.NekoSerializer,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Save<T>(string key, T value)",
                            Summary = "Saves value under key to the configured storage backend.",
                            Code =
@"NSR.Save(""playerName"", ""Neko"");
NSR.Save(""score"", 9001);
NSR.Save(""position"", transform.position);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Load<T>(string key, T defaultValue = default)",
                            Summary = "Loads value by key. Returns defaultValue when the key is missing.",
                            Code =
@"string name  = NSR.Load<string>(""playerName"", ""Unknown"");
int    score  = NSR.Load<int>(""score"", 0);
Vector3 pos   = NSR.Load<Vector3>(""position"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Exists(string key)",
                            Summary = "Returns true if the key has a saved value in the current storage backend.",
                            Code =
@"if (NSR.Exists(""highScore""))
    Debug.Log(""Previous high score found"");" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Delete(string key)",
                            Summary = "Removes the saved entry for key from the storage backend.",
                            Code =
@"NSR.Delete(""tempData""); // clean up after level complete" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "NSR.SaveAsync / NSR.LoadAsync",
                    Namespace = "NekoSerializer",
                    Summary = "Async variants of Save and Load. Returns Task / Task<T> — await in async methods.",
                    Description = "Async variants are useful when saving large or complex data structures to JSON files to avoid blocking the main thread. Both methods mirror the synchronous API exactly.",
                    Code =
@"using NekoSerializer;

await NSR.SaveAsync(""highScore"", 9999);
int hs = await NSR.LoadAsync<int>(""highScore"", 0);",
                    Tags = new[] { "Async", "Save", "Load" },
                    Category = DocCategory.NekoSerializer,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.SaveAsync<T>(string key, T value)",
                            Summary = "Async save. Returns Task. Use to avoid blocking the main thread on large payloads.",
                            Code =
@"private async void SaveProgress()
{
    await NSR.SaveAsync(""progress"", progressData);
    Debug.Log(""Saved!"");
}" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.LoadAsync<T>(string key, T defaultValue = default)",
                            Summary = "Async load. Returns Task<T>. Mirrors NSR.Load<T> exactly.",
                            Code =
@"private async void LoadProgress()
{
    var data = await NSR.LoadAsync<ProgressData>(""progress"", new ProgressData());
}" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "NSR.Pack / NSR.Unpack",
                    Namespace = "NekoSerializer",
                    Summary = "Bundle multiple saved keys into a portable string for cloud sync or profile transfer. Unpack restores them.",
                    Description = "Pack serializes the specified keys into a single string. Unpack writes them back to the storage backend. Pass overwriteExisting: false to preserve existing values and only write missing keys.",
                    Code =
@"using NekoSerializer;

// Bundle keys into a portable snapshot string
string snapshot = NSR.Pack(""playerName"", ""score"", ""position"");

// Restore — overwrites existing by default
NSR.Unpack(snapshot);

// Restore — skip keys that already exist
NSR.Unpack(snapshot, overwriteExisting: false);",
                    Tags = new[] { "Migration", "Pack", "Unpack", "CloudSync" },
                    Category = DocCategory.NekoSerializer,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Pack(params string[] keys)",
                            Summary = "Bundles specified saved keys into a portable snapshot string for cloud sync or transfer.",
                            Code =
@"string snapshot = NSR.Pack(""playerName"", ""score"", ""position"");
PlayerPrefs.SetString(""cloudBackup"", snapshot); // or send to server" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Unpack(string snapshot)",
                            Summary = "Restores all keys from a snapshot string. Overwrites existing values by default.",
                            Code =
@"string snapshot = PlayerPrefs.GetString(""cloudBackup"");
NSR.Unpack(snapshot);" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Unpack(string snapshot, bool overwriteExisting)",
                            Summary = "Restores keys from snapshot. Pass false to skip keys that already exist locally.",
                            Code =
@"// Only write keys that don't exist yet (first-time restore)
NSR.Unpack(snapshot, overwriteExisting: false);" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "NSR.Serialize / NSR.Deserialize",
                    Namespace = "NekoSerializer",
                    Summary = "Direct JSON serialization helpers — useful for networking, clipboard, or manual file handling.",
                    Description = "These methods bypass the storage backend entirely and work with raw JSON strings. PrettyPrintJson in SerializerSettings controls output formatting.",
                    Code =
@"using NekoSerializer;

string json   = NSR.Serialize(myData);
MyData data   = NSR.Deserialize<MyData>(json);

// Last save timestamps
DateTime utc   = NSR.LastSaveTimeUtc;
DateTime local = NSR.LastSaveTimeLocal;",
                    Tags = new[] { "JSON", "Serialize", "Deserialize" },
                    Category = DocCategory.NekoSerializer,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Serialize<T>(T obj)",
                            Summary = "Serializes an object to a JSON string. Bypasses storage backend.",
                            Code =
@"string json = NSR.Serialize(myData);
SendToServer(json); // networking, clipboard, manual file" },
                        new DocMember { Kind = DocMemberKind.Method, Signature = "NSR.Deserialize<T>(string json)",
                            Summary = "Deserializes a JSON string to T. Bypasses storage backend.",
                            Code =
@"MyData data = NSR.Deserialize<MyData>(receivedJson);" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "NSR.LastSaveTimeUtc",
                            Summary = "UTC timestamp of the most recent Save or SaveAsync call.",
                            Code =
@"Debug.Log($""Last saved: {NSR.LastSaveTimeUtc}"");" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "NSR.LastSaveTimeLocal",
                            Summary = "Local time of the most recent Save or SaveAsync call.",
                            Code =
@"saveLabel.text = $""Saved at {NSR.LastSaveTimeLocal:HH:mm}"";
" },
                    }
                },
                new NekoLibDocEntry
                {
                    Title = "SerializerSettings",
                    Namespace = "NekoSerializer",
                    Summary = "ScriptableObject config asset. Create once at Assets/Resources/SerializerSettings. Defaults apply if missing.",
                    Description = "Properties:\n• StorageOption — PlayerPrefs (default) or JsonFile\n• SaveDirectory — folder under Application.persistentDataPath (default: \"SaveData\")\n• UseEncryption — encrypt strings before writing (default: false)\n• EncryptionKey — key used when encryption is on (default: \"DefaultEncryptionKey\")\n• PrettyPrintJson — indented vs compact JSON (default: true)\n\nCreate via: Assets → Create → Neko Framework → Serialize → Serializer Settings.",
                    Tags = new[] { "Settings", "Config", "Encryption", "Storage" },
                    Category = DocCategory.NekoSerializer,
                    Members = new[]
                    {
                        new DocMember { Kind = DocMemberKind.Property, Signature = "StorageOption",
                            Summary = "PlayerPrefs (default) or JsonFile. Switch to JsonFile for larger data.",
                            Code =
@"// Set via Inspector on the SerializerSettings asset.
// PlayerPrefs — fast, platform-native key/value store
// JsonFile    — writes to Application.persistentDataPath/SaveDirectory" },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "SaveDirectory",
                            Summary = "Subfolder under Application.persistentDataPath for JsonFile storage. Default: \"SaveData\".",
                            Code =
@"// e.g. Application.persistentDataPath + ""/MyGame/""
// Set SaveDirectory = ""MyGame"" in the SerializerSettings asset." },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "UseEncryption",
                            Summary = "Whether to encrypt saved data. Default: false.",
                            Code =
@"// Enable in SerializerSettings asset to encrypt all Save/Load operations.
// Pair with a strong EncryptionKey." },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "EncryptionKey",
                            Summary = "Key used when UseEncryption is true. Default: \"DefaultEncryptionKey\" — change before shipping.",
                            Code =
@"// IMPORTANT: Change from the default before shipping.
// EncryptionKey is not secret from players with file access." },
                        new DocMember { Kind = DocMemberKind.Property, Signature = "PrettyPrintJson",
                            Summary = "Indented vs compact JSON output. Default: true. Disable for smaller file sizes.",
                            Code =
@"// PrettyPrintJson = true  → human-readable, easier to debug
// PrettyPrintJson = false → compact, smaller file size on JsonFile backend" },
                    }
                }
            };
        }
    }
}
#endif

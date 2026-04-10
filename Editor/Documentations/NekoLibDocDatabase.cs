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
                    Category = DocCategory.Core
                },
                new NekoLibDocEntry
                {
                    Title = "LazySingleton<T>",
                    Namespace = "NekoLib.Singleton",
                    Summary = "Auto-created singleton. A new GameObject is spawned on first access if none exists.",
                    Description = "Ideal for utility managers (audio, pooling, etc.) that do not need to be pre-placed in any scene. The instance is created lazily and destroyed when the application quits.",
                    Code =
@"public class AudioManager : LazySingleton<AudioManager>
{
    public void PlayMusic(AudioClip clip) { /* ... */ }
}

// No scene setup needed — created on first access
AudioManager.Instance.PlayMusic(backgroundMusic);",
                    Tags = new[] { "Singleton", "Lazy", "Auto-create" },
                    Category = DocCategory.Core
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
                    Category = DocCategory.Core
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
                    Category = DocCategory.Core
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
                    Category = DocCategory.Core
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
                    Category = DocCategory.Core
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
                    Category = DocCategory.Core
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
                    Summary = "Conditional logger — stripped in release builds unless NEKOLIB_LOG is defined.",
                    Description = "Log.Info, Log.Warn, and Log.Error mirror Debug.Log but are compiled only in the Editor, Development builds, or when NEKOLIB_LOG is defined. Supports an optional context object to ping it in the Console.",
                    Code =
@"using NekoLib.Logger;

Log.Info(""System started"");
Log.Warn(""Low memory"");
Log.Error(""Something broke"");

// With context — click the log entry to ping the object
Log.Info(""Found target"", enemyGameObject);",
                    Tags = new[] { "Logging", "Debug", "Conditional" },
                    Category = DocCategory.Core
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
anim.Play();
anim.PlayOneShot();       // Once mode — stops at end
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
                    Category = DocCategory.Components
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
                    Category = DocCategory.Components
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
                    Category = DocCategory.Components
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
                    Category = DocCategory.Components
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
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
                    Category = DocCategory.Extensions
                },

                // ── Collections ─────────────────────────────────
                new NekoLibDocEntry
                {
                    Title = "Grid<T>",
                    Namespace = "NekoLib.Collections",
                    Summary = "Flat-array backed 2D grid with ref access, Span rows, safe TryGet/TrySet, and search helpers.",
                    Description = "Stores data in a contiguous row-major array for cache-friendly access. The unchecked indexer [x, y] is fast; TryGet/TrySet are bounds-safe. GetRowSpan() returns a Span<T> for a row that writes back into the grid. ForEach with ref gives in-place mutation without index overhead.\n\nPrefer Grid<T> over T[,] for performance-critical 2D buffers and Span-based APIs.",
                    Code =
@"var grid = new Grid<int>(10, 5);

grid[3, 2] = 42;       // fast unchecked
int v = grid[3, 2];

if (grid.TryGet(3, 2, out int val)) Debug.Log(val);
grid.TrySet(0, 0, 7);

grid.Fill(0);
grid.Clear();

grid.ForEach((x, y, ref int cell) => cell += 1);

Span<int> row = grid.GetRowSpan(2);
row[0] = 99;   // writes back into grid

var g2 = Grid<int>.From2D(source2DArray);",
                    Tags = new[] { "Grid", "Span", "Performance", "2D" },
                    Category = DocCategory.Collections
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
                    Category = DocCategory.Services
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
                    Category = DocCategory.Services
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
                    Category = DocCategory.Utilities
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
                    Category = DocCategory.Utilities
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
                    Category = DocCategory.Utilities
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
                    Category = DocCategory.Utilities
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
                    Category = DocCategory.Utilities
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
                    Category = DocCategory.NekoSignal
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
                    Category = DocCategory.NekoSignal
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
                    Category = DocCategory.NekoSignal
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
                    Category = DocCategory.NekoSignal
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
                    Category = DocCategory.NekoFlow
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
                    Category = DocCategory.NekoFlow
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
                    Category = DocCategory.NekoFlow
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
                    Category = DocCategory.NekoFlow
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
                    Category = DocCategory.NekoFlow
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
                    Category = DocCategory.NekoSerializer
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
                    Category = DocCategory.NekoSerializer
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
                    Category = DocCategory.NekoSerializer
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
                    Category = DocCategory.NekoSerializer
                },
                new NekoLibDocEntry
                {
                    Title = "SerializerSettings",
                    Namespace = "NekoSerializer",
                    Summary = "ScriptableObject config asset. Create once at Assets/Resources/SerializerSettings. Defaults apply if missing.",
                    Description = "Properties:\n• StorageOption — PlayerPrefs (default) or JsonFile\n• SaveDirectory — folder under Application.persistentDataPath (default: \"SaveData\")\n• UseEncryption — encrypt strings before writing (default: false)\n• EncryptionKey — key used when encryption is on (default: \"DefaultEncryptionKey\")\n• PrettyPrintJson — indented vs compact JSON (default: true)\n\nCreate via: Assets → Create → Neko Framework → Serialize → Serializer Settings.",
                    Tags = new[] { "Settings", "Config", "Encryption", "Storage" },
                    Category = DocCategory.NekoSerializer
                }
            };
        }
    }
}
#endif

# NekoLib Extensions

Extension methods for Unity and C# types.

```csharp
using NekoLib.Extensions;
```

## Unity Extensions

### GameObjectExtensions

```csharp
// Get or add component
AudioSource audio = gameObject.GetOrAdd<AudioSource>();
Rigidbody rb = monoBehaviour.GetOrAdd<Rigidbody>();

// Layer management
bool inLayer = gameObject.IsInLayer(LayerMask.GetMask("Enemy"));
gameObject.SetLayer("Player");           // By name
gameObject.SetLayer(8);                  // By layer number
gameObject.SetLayer(LayerMask.GetMask("UI")); // By LayerMask

// Child management
gameObject.ClearChildTransforms(); // Destroy all children

// Get children in specific layers
GameObject[] enemyChildren = gameObject.GetChildrenInLayer(LayerMask.GetMask("Enemy"));
GameObject[] allEnemies = gameObject.GetChildrenInLayerRecursive(LayerMask.GetMask("Enemy"));
// MonoBehaviour overloads also available:
GameObject[] fromMono = monoBehaviour.GetChildrenInLayer(LayerMask.GetMask("Enemy"));
```

### TransformExtensions

```csharp
// Child management
transform.Clear(); // Destroy all children
Transform[] children = transform.GetChildren(includeInactive: false);

// 2D look-at
transform.LookAt2D(targetPosition);
transform.LookAt2D(targetTransform, angleOffset: 90f);

// Distance and direction utilities
float distance = transform.DistanceTo(otherTransform);
Vector3 direction = transform.DirectionTo(otherTransform);
bool inRange = transform.InRangeOf(otherTransform, 5f);

// Transform resets
transform.ResetTransform();      // Reset world transform
transform.ResetLocalTransform(); // Reset local transform

// Layer filtering for children
GameObject[] enemyChildren = transform.GetChildrenInLayer(LayerMask.GetMask("Enemy"));
GameObject[] allEnemies = transform.GetChildrenInLayerRecursive(LayerMask.GetMask("Enemy"));
```

### AnimatorExtensions

```csharp
// Animation length queries — hash overload expects a CLIP-name hash (Animator.StringToHash(clipName))
float clipLength = animator.GetAnimationLength("JumpAnimation");
float hashLength = animator.GetAnimationLength(Animator.StringToHash("JumpAnimation"));

// Animation state checks — hash overload expects a STATE-name hash (shortNameHash)
bool isJumping = animator.IsPlayingAnimation("JumpAnimation");
bool isPlaying = animator.IsPlayingAnimation(jumpStateHash);
```

### CameraExtensions

```csharp
// Culling mask management
bool isVisible = camera.IsLayerInCullingMask(LayerMask.GetMask("Enemy"));
camera.AddToCullingMask(LayerMask.GetMask("UI"));
camera.RemoveFromCullingMask(LayerMask.GetMask("UI"));
camera.SetCullingMask(LayerMask.GetMask("Player", "Enemy"));

// FOV control
camera.SetFOV(60f);

// Orthographic control
camera.SetOrthographicSize(5f);
camera.FitBoundsInView(bounds); // adjusts orthographic size to fit bounds

// Screen info
Vector2 screenSize = camera.GetScreenSize(); // pixelWidth x pixelHeight
```

### ColorExtensions

```csharp
// Component modification
Color newColor = originalColor.WithAlpha(0.5f);
Color redModified = originalColor.WithRed(1f);
Color greenModified = originalColor.WithGreen(0.5f);
Color blueModified = originalColor.WithBlue(0f);

// Color operations
Color brighter = color.MultiplyRGB(1.5f);
Color lighter = color.AddRGB(0.1f);
Color inverted = color.Invert();
Color grayscale = color.ToGrayscale();
float brightness = color.GetLuminance();

// Hex conversion
string hex = color.ToHex(); // "#RRGGBBAA"
Color parsed = "#FF0000FF".ToColor();
```

### Vector2Extensions

```csharp
// Component modification
Vector2 modified = vector.With(x: 5f, y: 10f);
Vector2 added = vector.Add(x: 2f);
Vector2 subtracted = vector.Subtract(y: 1f);
Vector2 multiplied = vector.Multiply(x: 2f, y: 0.5f);
Vector2 divided = vector.Divide(x: 2f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector2 direction = fromPos.DirectionTo(toPos);
float distance = fromPos.DistanceTo(toPos);
Vector2 withLength = vector.WithMagnitude(5f);
float maxComp = vector.MaxComponent();
float minComp = vector.MinComponent();
Vector2 perpendicular = vector.Perpendicular();          // counterclockwise
Vector2 perpendicularCW = vector.PerpendicularClockwise(); // clockwise
Vector2 rotated = vector.Rotate(45f);

// Boundary checks
bool insideCircle = point.IsInsideCircle(center, radius);
bool insideRect = point.IsInsideRect(center, size);

// Random points
Vector2 onEdge    = origin.RandomPointOnCircle(radius: 5f);
Vector2 inDisk    = origin.RandomPointInDisk(radius: 5f);
Vector2 inRing    = origin.RandomPointInAnnulus(minRadius: 2f, maxRadius: 8f);
```

### Vector3Extensions

```csharp
// Component modification
Vector3 modified = vector.With(x: 5f, y: 10f, z: 15f);
Vector3 added = vector.Add(y: 1f);
Vector3 subtracted = vector.Subtract(x: 1f);
Vector3 multiplied = vector.Multiply(x: 2f);
Vector3 divided = vector.Divide(z: 2f);
Vector3 rotatedX = vector.RotateX(45f);
Vector3 rotatedY = vector.RotateY(90f);
Vector3 rotatedZ = vector.RotateZ(30f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector3 direction = fromPos.DirectionTo(toPos);
float distance = fromPos.DistanceTo(toPos);
Vector3 withLength = vector.WithMagnitude(5f);
float maxComp = vector.MaxComponent();
float minComp = vector.MinComponent();

// Boundary checks
bool insideSphere = point.IsInsideSphere(center, radius);
bool insideBox = point.IsInsideBox(center, size);             // AABB by center+size
bool insideCollider = point.IsInsideColliderBounds(collider); // AABB of a Collider

// Random points
// Plane2D enum (XY / XZ / YZ) is in NekoLib.Extensions
Vector3 onEdge = origin.RandomPointOnCircle(5f, Plane2D.XZ);
Vector3 inDisk = origin.RandomPointInDisk(5f, Plane2D.XZ);
Vector3 inRing = origin.RandomPointInAnnulus(2f, 8f, Plane2D.XZ);
```

## C# Extensions

### StringExtensions

```csharp
// Number parsing
float value = "3,14".ParseFloatWithComma(); // 3.14f
bool ok = "3,14".TryParseFloatWithComma(out float result); // non-throwing variant

// String utilities
string noSpaces = "hello world".WithoutSpaces();    // "helloworld"
string split = "MyVariableName".SplitCamelCase();   // "My Variable Name"

// Percentage formatting
string percent = 0.25f.AsPercent();              // "25%"  (multiplies by 100)
string percent2 = 0.25f.AsPercent(1);            // "25%"
string exact = 25f.AsExactPercent();             // "25%"  (uses value directly)
string exact2 = 25.5f.AsExactPercent(1);         // "25.5%"

// Enum conversion
MyEnum value = "EnumValue".ToEnum<MyEnum>();
MyEnum safe = "BadValue".ToEnumOrDefault(MyEnum.Default);
```

### BigNumberStyleExtensions

```csharp
// Mini big-number formatter — no dependency on the NekoBigNumber package.
// Two styles via the BigNumberStyle enum. Overloads for int, long, and decimal.

// Compact: K/M/B/T then doubled-letter pairs (aa, bb, cc, ...).
string c1 = 1_500m.ToBigNumber(BigNumberStyle.Compact);                  // "1.5K"
string c2 = 1_500_000m.ToBigNumber(BigNumberStyle.Compact);              // "1.5M"
string c3 = 1_500_000_000_000L.ToBigNumber(BigNumberStyle.Compact);      // "1.5T"
string c4 = 1_000_000_000_000_000m.ToBigNumber(BigNumberStyle.Compact);  // "1aa"
string c5 = 1_000_000_000_000_000_000m.ToBigNumber(BigNumberStyle.Compact); // "1bb"

// Alphabetical: single letters a..z from 1e3 onward.
string a1 = 1_500m.ToBigNumber(BigNumberStyle.Alphabetical);             // "1.5a"
string a2 = 1_500_000m.ToBigNumber(BigNumberStyle.Alphabetical);         // "1.5b"

// Custom precision (trailing zeros always trimmed).
string p0 = 2300m.ToBigNumber(BigNumberStyle.Compact, 0);                // "2K"
string p2 = 1230m.ToBigNumber(BigNumberStyle.Compact, 2);                // "1.23K"

// Negative values keep the sign.
string n  = (-1500m).ToBigNumber(BigNumberStyle.Compact);                // "-1.5K"
```

### NumberExtensions

```csharp
// Percentage calculations
float percentage = current.PercentageOf(total);

// Probability/chance
bool success = 0.75f.IsSuccessfulRoll();          // 75% chance (0–1 range)
bool luckyRoll = 25.IsSuccessfulRoll(0, 100);     // 25% chance out of 100 (int overload)

// Enum conversion
MyEnum enumValue = 1.ToEnum<MyEnum>();
MyEnum safeEnum = 999.ToEnumOrDefault(MyEnum.Default);
```

### CollectionExtensions

```csharp
using NekoLib.Logger;

// Array operations
T randomItem = array.Rand();
int randomIndex = array.RandIndex();
T[] shuffled = array.Shuffle();
T[] swapped = array.SwapAt(0, 1);
T[] swappedByElement = array.Swap(item1, item2);
bool isEmpty = array.IsNullOrEmpty();
bool hasNulls = array.ContainsNull();
string formatted = array.ToLiteral(); // "[item1, item2, item3]"
T[] sliced = array.Slice(2, 5);
T[] multiple = array.RandMultiple(3);
T weighted = array.RandWeighted(item => item.weight);
T[] reversed = array.Reverse();
bool contains = array.Contains(item);

// List operations
T randomItem = list.Rand();
int randomIndex = list.RandIndex();
List<T> shuffled = list.Shuffle();
List<T> swapped = list.SwapAt(0, 1);
bool hasNulls = list.ContainsNull();
string formatted = list.ToLiteral(); // "{item1, item2, item3}"
List<T> multiple = list.RandMultiple(3);
T weighted = list.RandWeighted(item => item.weight);

// Dictionary operations
V randomValue = dict.RandV();
K randomKey = dict.RandK();
bool hasNulls = dict.ContainsNullValues();
string formatted = dict.ToLiteral(); // "{key1: value1, key2: value2}"

// Other collections
string queueFormatted = queue.ToLiteral();
string stackFormatted = stack.ToLiteral();
string setFormatted = hashSet.ToLiteral();
```

### TimeExtensions

```csharp
// Clock string formatting (float/double/int overloads; useCeiling = true by default)
string clock = 3661f.ToClock();          // "01:01:01"
string clockFloor = 3661f.ToClock(useCeiling: false);
string short1 = 125f.ToShortClock();     // "02:05"
string short2 = 125.ToShortClock();      // int overload, no ceiling param

// Readable duration (TimeSpan / double / float / int overloads)
string readable = 93784f.ToReadableFormat();              // "1d 2h 3m" (spacing by default)
string noSpace = 93784f.ToReadableFormat(useSpacing: false); // "1d2h3m"
string fromSpan = TimeSpan.FromSeconds(93784).ToReadableFormat(); // no spacing by default

// DateTime elapsed time (past → now, uses TimeService.Now)
DateTime past = TimeService.Now.AddHours(-2);
TimeSpan elapsed = past.TimeSince();          // TimeSpan
double seconds = past.SecondsSince();         // seconds as double
double minutes = past.MinutesSince();
double hours = past.HoursSince();
double days = past.DaysSince();

// UTC variants
TimeSpan utcElapsed = past.TimeSinceUtc();
double utcHours = past.HoursSinceUtc();

// DateTime time remaining (future → now, uses TimeService.Now)
DateTime future = TimeService.Now.AddHours(3);
TimeSpan remaining = future.TimeFromNow();    // TimeSpan
double secFromNow = future.SecondsFromNow();
double minFromNow = future.MinutesFromNow();
double hrsFromNow = future.HoursFromNow();
double daysFromNow = future.DaysFromNow();

// UTC variants
double utcSecsFromNow = future.SecondsFromNowUtc();
```

### TMPTextExtensions

```csharp
// Set to "HH:MM:SS" from seconds
tmpText.SetClock(3661);        // "01:01:01"
tmpText.SetClock(3661f);

// Set to "MM:SS" from seconds
tmpText.SetShortClock(125);           // "02:05"

// Readable duration (spacing optional)
tmpText.SetReadableTime(93784);      // "1d 2h 3m" (default spacing)
tmpText.SetReadableTime(93784, useSpacing: false); // "1d2h3m"
```

### CoroutineExtensions

```csharp
// Sequential execution
Coroutine sequence = this.StartCoroutineSequence(coroutineA, coroutineB, coroutineC);

// Delayed execution
Coroutine delayed = this.StartCoroutineDelayed(myCoroutine, 2f);

// Conditional execution
Coroutine conditional = this.StartCoroutineWhen(myCoroutine, () => isReady);

// Parallel execution
Coroutine parallel = this.StartCoroutineParallel(coroutineA, coroutineB, coroutineC);

// Convert Coroutine to Task for async/await support
Task task = StartCoroutine(myCoroutine).AsTask(this);
await task;

// Convert IEnumerator to Task
Task enumTask = myEnumerator.AsTask(this);
await enumTask;

// Run multiple coroutines concurrently and wait for all
await this.WhenAll(coroutineA, coroutineB, coroutineC);

// Run multiple coroutines and wait for any one to complete
Task<Task> firstCompleted = await this.WhenAny(coroutineA, coroutineB, coroutineC);
```

### TimerExtensions

> Requires `using NekoLib.Timer;`

```csharp

// Invoke once after a delay; returns a token to cancel before it fires
TimerToken token = this.Delay(2f, () => Log.Info("Delayed"));
TimerToken unscaled = this.Delay(2f, () => Log.Info("Unscaled"), useUnscaledTime: true);
token.Cancel(); // cancels before it fires — silent, no callbacks

// Repeat every interval; returns a token to stop the loop
TimerToken ticker = this.Repeat(1f, () => Log.Info("Tick"));
ticker.Cancel();
```

### TaskExtensions

```csharp
using NekoLib.Logger;

// Fire-and-forget tasks
myAsyncTask.Forget();
myAsyncTask.Forget(ex => Log.Error($"Task failed: {ex}"));

// Convert Task to Coroutine using YieldTask
IEnumerator WebRequestExample()
{
    // Use Task in coroutine
    Task<string> webTask = FetchDataAsync();
    yield return new YieldTask(webTask);
    Log.Info($"Got data: {webTask.Result}");

    // Or use AsCoroutine extension
    yield return AnotherAsyncOperation().AsCoroutine();
}
```

### TextColorizeExtensions

```csharp
// Basic colorization
string colored = "Hello".Colorize(Color.red);
string hexColored = "World".Colorize("#FF0000");

// Selective colorization
string selective = "Hello World".Colorize(Color.red, "Hello");
string chars = "Hello!".Colorize(Color.blue, '!');
string multiple = "Red and Blue".Colorize(Color.red, "Red", "Blue");

// Conditional colorization
string conditional = "Error".Colorize(Color.red, () => hasError);
string predicate = "Some words".Colorize(Color.green, word => word.Length > 4);
```

### TextFormatExtensions

```csharp
// Bold formatting
string bold = "Important".Bold(); // "<b>Important</b>"
string selective = "This is important".Bold("important");

// Italic formatting
string italic = "Emphasis".Italic(); // "<i>Emphasis</i>"

// Size formatting
string sized = "Big Text".Size(24f); // "<size=24>Big Text</size>"

// Chaining
string formatted = "Important Warning"
    .Bold("Important")
    .Italic("Warning")
    .Size(18f, "Warning");
```

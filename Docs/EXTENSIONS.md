# NekoLib Extensions

Extension methods for Unity and C# types.

### GameObjectExtensions

```csharp
// Get or add component
AudioSource audio = gameObject.GetOrAdd<AudioSource>();
Rigidbody rb = monoBehaviour.GetOrAdd<Rigidbody>();

// Active state management
monoBehaviour.SetActive();   // Set active to true
monoBehaviour.SetInactive(); // Set active to false

// Layer management
bool inLayer = gameObject.IsInLayer(LayerMask.GetMask("Enemy"));
gameObject.SetLayer("Player");           // By name
gameObject.SetLayer(8);                  // By layer number
gameObject.SetLayer(LayerMask.GetMask("UI")); // By LayerMask

// Child management
gameObject.ClearChildTransforms();

// Get children in specific layers
GameObject[] enemyChildren = gameObject.GetChildrenInLayer(LayerMask.GetMask("Enemy"));
GameObject[] allEnemies = gameObject.GetChildrenInLayerRecursive(LayerMask.GetMask("Enemy"));
```

### TransformExtensions

```csharp
// Child management
transform.Clear(); // Destroy all children
Transform[] children = transform.GetChildren(includeInactive: false);

// Orbital positioning (static)
transform.SetOrbitRotation(target, horizontalAngle: 45f, verticalAngle: 30f, distance: 5f);
transform.SetOrbitRotationClamped(target, 45f, 30f, 5f, -80f, 80f);

// Orbital movement (dynamic - call in Update)
float currentAngle = 0f;
transform.OrbitAround(target, Orientation.Horizontal, speed: 30f, staticAngle: 0f, distance: 5f, ref currentAngle);

// Rotation around point
transform.RotateAround(center, Vector3.up, 90f);

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
// Animation length queries
float clipLength = animator.GetAnimationLength("JumpAnimation");
float hashLength = animator.GetAnimationLength(Animator.StringToHash("JumpAnimation"));

// Animation progress tracking
float progress = animator.GetCurrentAnimationProgress(layerIndex: 0);
float remainingTime = animator.GetCurrentAnimationRemainingTime(layerIndex: 0);

// Animation state checks
bool isJumping = animator.IsPlayingAnimation("JumpAnimation");
bool isPlaying = animator.IsPlayingAnimation(jumpAnimHash);

// Play and wait for completion (coroutines)
yield return animator.PlayAndWait("AttackAnimation");
yield return animator.CrossFadeAndWait("IdleAnimation", transitionDuration: 0.2f);

// Wait for specific animation to complete
yield return AnimatorExtensions.WaitForAnimation(animator, "DeathAnimation");
```

### CameraExtensions

```csharp
// Culling mask management
bool isVisible = camera.IsLayerInCullingMask(LayerMask.GetMask("Enemy"));
camera.AddToCullingMask(LayerMask.GetMask("UI"));
camera.SetCullingMask(LayerMask.GetMask("Player", "Enemy"));

// FOV control
camera.ZoomIn(15f);
camera.SetFOV(60f);
```

### ColorExtensions

```csharp
// Component modification
Color newColor = originalColor.WithAlpha(0.5f);

// Color operations
Color brighter = color.MultiplyRGB(1.5f);
Color inverted = color.Invert();

// Hex conversion
string hex = color.ColorToHex(); // "#RRGGBBAA"
Color color = "#FF0000FF".HexToColor();
```

### Vector2Extensions

```csharp
// Component modification
Vector2 modified = vector.With(x: 5f, y: 10f);
Vector2 added = vector.Add(x: 2f);
Vector2 multiplied = vector.Multiply(x: 2f, y: 0.5f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector2 direction = fromPos.DirectionTo(toPos);
float distance = fromPos.DistanceTo(toPos);
Vector2 perpendicular = vector.Perpendicular();
Vector2 rotated = vector.Rotate(45f);

// Boundary checks
bool insideCircle = point.IsInsideCircle(center, radius);
bool insideRect = point.IsInsideRect(center, size);

// Random point in annulus
Vector2 randomPoint = origin.RandomPointInAnnulus(minRadius: 2f, maxRadius: 8f);
```

### Vector3Extensions

```csharp
// Component modification
Vector3 modified = vector.With(x: 5f, y: 10f, z: 15f);
Vector3 rotatedX = vector.RotateX(45f);
Vector3 rotatedY = vector.RotateY(90f);

// Vector operations
bool inRange = currentPos.InRangeOf(targetPos, 5f);
Vector3 direction = fromPos.DirectionTo(toPos);
Vector3 reflected = vector.Reflect(normal);
Vector3 projected = vector.ProjectOnto(onto);

// Boundary checks
bool insideSphere = point.IsInsideSphere(center, radius);
bool insideBox = point.IsInsideBox(center, size);
bool insideBounds = point.IsInsideBounds(bounds);

// Random point in 3D annulus
Vector3 randomPoint = origin.RandomPointInAnnulus(2f, 8f, Plane2D.XZ);
```

## C# Extensions

### StringExtensions

```csharp
// Number parsing
float value = "3,14".ParseFloatWithComma(); // 3.14f

// Percentage formatting
string percent = 0.25f.AsPercent(); // "25%"

// Large number formatting
string short = 1500000.ToShortFormat(1); // "1.5M"

// Enum conversion
MyEnum value = "EnumValue".ToEnum<MyEnum>();
```

### NumberExtensions

```csharp
// Number checks
bool isEven = 42.IsEven();
bool isOdd = 13.IsOdd();

// Percentage calculations
float percentage = current.PercentageOf(total);

// Range clamping
int clamped = value.AtLeast(10).AtMost(100);
float clampedFloat = value.AtLeast(0f).AtMost(1f);

// Probability/chance
bool success = 0.75f.RollChance(); // 75% chance
bool luckyRoll = 25.RollChance(0, 100); // 25% chance out of 100

// Enum conversion
MyEnum enumValue = 1.ToEnum<MyEnum>();
MyEnum safeEnum = 999.ToEnumOrDefault(MyEnum.Default);
```

### CollectionExtensions

```csharp
// Array operations
T randomItem = array.Rand();
int randomIndex = array.RandIndex();
T[] shuffled = array.Shuffle();
T[] swapped = array.Swap(0, 1);
T[] swappedByElement = array.Swap(item1, item2);
bool isEmpty = array.IsNullOrEmpty();
bool hasNulls = array.ContainsNull();
string formatted = array.ToLiteral(); // "[item1, item2, item3]"
T first = array.First();
T last = array.Last();
T[] sliced = array.Slice(2, 5);
T[] multiple = array.RandMultiple(3);
T weighted = array.RandWeighted(item => item.weight);
T[] reversed = array.Reverse();
bool contains = array.Contains(item);

// List operations
T randomItem = list.Rand();
int randomIndex = list.RandIndex();
List<T> shuffled = list.Shuffle();
List<T> swapped = list.Swap(0, 1);
bool hasNulls = list.ContainsNull();
string formatted = list.ToLiteral(); // "{item1, item2, item3}"
T first = list.First();
T last = list.Last();
List<T> multiple = list.RandMultiple(3);
T weighted = list.RandWeighted(item => item.weight);

// Dictionary operations
V randomValue = dict.RandV();
K randomKey = dict.RandK();
bool hasNulls = dict.ContainsNullValues();
Dictionary<K, V> copy = dict.AsNewCopy();
string formatted = dict.ToLiteral(); // "{key1: value1, key2: value2}"

// Grid<T> operations
Grid<int> grid = new Grid<int>(10, 5);
grid[3, 2] = 42;                       // unchecked fast indexer
int v = grid[3, 2];

bool contains = grid.Contains(42);    // linear scan over contiguous buffer
// IndexOf returns (x,y) tuple and throws if not found
var pos = grid.IndexOf(42);            // (int x, int y)
// Safe variants
if (grid.TryIndexOf(42, out var foundPos)) Debug.Log(foundPos);

// Last occurrence helpers
var last = grid.LastIndexOf(42);
if (grid.TryLastIndexOf(42, out var lastPos)) Debug.Log(lastPos);

int first = grid.First();              // element at (0,0)
int lastElem = grid.Last();            // element at (width-1,height-1)
bool emptyGrid = grid.IsNullOrEmpty();
// For reference-type grids:
bool hasNullsInGrid = default(Grid<object>).IsNullOrEmpty() ? false : grid.ContainsNull();
string literal = grid.ToLiteral();     // compact textual representation

// Other collections
string queueFormatted = queue.ToLiteral();
string stackFormatted = stack.ToLiteral();
string setFormatted = hashSet.ToLiteral();
```

### TimeExtensions

```csharp
// Clock formatting
string clock = 3661f.ToClock(); // "01:01:01"
string short = 125f.ToShortClock(); // "02:05"

// DateTime calculations
DateTime pastTime = DateTime.Now.AddHours(-2);
double hours = pastTime.HoursUntilNow(); // 2.0

// DateTime manipulation
DateTime newDate = original.WithDate(year: 2024);
DateTime newTime = original.WithTime(hour: 9, minute: 0);
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

```csharp
// Create / start countdowns
Countdown cd = this.CreateCountdown(10f);
Countdown started = this.StartCountdown(5f);
Countdown conditional = this.StartCountdown(30f, () => isReady);

// Stopwatches
Stopwatch sw = this.CreateStopwatch();
Stopwatch running = this.StartStopwatch();

// Delayed and repeated actions
this.InvokeDelayed(2f, () => Debug.Log("Delayed"), useUnscaledTime: true);
IDisposable every = this.InvokeEvery(1f, () => Debug.Log("Tick"));
IDisposable secondsTicker = this.TickEverySeconds(1, 5, secs => Debug.Log($"Ticked {secs}s"), () => Debug.Log("Done"));

// Cleanup timers
this.CleanupTimers();
this.CleanupComponentTimers();
```

### SerializeExtensions

```csharp
// JSON serialization
string json = myObject.Serialize(prettyPrint: true);
MyObject restored = json.Deserialize<MyObject>();
```

### TaskExtensions

```csharp
// Fire-and-forget tasks
myAsyncTask.Forget();
myAsyncTask.Forget(ex => Debug.LogError($"Task failed: {ex}"));

// Convert Task to Coroutine using YieldTask
IEnumerator WebRequestExample()
{
    // Use Task in coroutine
    Task<string> webTask = FetchDataAsync();
    yield return new YieldTask(webTask);
    Debug.Log($"Got data: {webTask.Result}");

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

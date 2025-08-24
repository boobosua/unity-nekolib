# Extensions

## Table of Contents

- [Collection](#collection)
- [GameObject](#gameobject)
- [Number](#number)
- [String](#string)
- [Task](#task)
- [Time](#time)
- [Timer](#timer)
- [Transform](#transform)
- [Vector2/3](#vector23)
- [Serialize](#serialize)

---

## Collection

### Rand

Get random element from array.

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int randomNum = numbers.Rand();
```

### RandIndex

Get random index from array.

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int randomIndex = numbers.RandIndex();
```

### Shuffle

Shuffle array elements (returns new array).

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int[] shuffled = numbers.Shuffle();
```

### Swap

Swap two elements in array by index.

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int[] swapped = numbers.Swap(0, 4); // Swaps first and last
```

### IsNullOrEmpty

Check if array is null or empty.

```csharp
int[] numbers = {1, 2, 3};
if (!numbers.IsNullOrEmpty()) { /* ... */ }
```

### Format

Format array as string for display.

```csharp
List<string> items = new List<string> {"apple", "banana"};
Debug.Log(items.Format()); // "apple, banana"
```

### Last

Get last element from array.

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int last = numbers.Last(); // 5
```

### First

Get first element from array.

```csharp
int[] numbers = {1, 2, 3, 4, 5};
int first = numbers.First(); // 1
```

---

## GameObject

### GetOrAdd

Get component or add if missing.

```csharp
GameObject player = GameObject.FindWithTag("Player");
Rigidbody rb = player.GetOrAdd<Rigidbody>();
```

### SetActive

Set GameObject active (for MonoBehaviour).

```csharp
myMonoBehaviour.SetActive(); // Sets gameObject.SetActive(true)
```

### SetInactive

Set GameObject inactive (for MonoBehaviour).

```csharp
myMonoBehaviour.SetInactive(); // Sets gameObject.SetActive(false)
```

### IsInLayer

Check if GameObject is in specified layer.

```csharp
GameObject player = GameObject.FindWithTag("Player");
if (player.IsInLayer("Player")) { }
```

### ClearChildTransforms

Clear all child transforms.

```csharp
GameObject parent = GameObject.Find("Container");
parent.ClearChildTransforms();
```

---

## Number

### IsEven

Check if number is even.

```csharp
int number = 42;
bool isEven = number.IsEven(); // true
```

### IsOdd

Check if number is odd.

```csharp
int number = 43;
bool isOdd = number.IsOdd(); // true
```

### PercentageOf

Calculate percentage of current relative to total.

```csharp
float percent = 75f.PercentageOf(100f); // 0.75
```

### AtLeast

Ensure number doesn't go below minimum.

```csharp
int value = 5.AtLeast(10); // 10
```

### AtMost

Ensure number doesn't go above maximum.

```csharp
int value = 15.AtMost(10); // 10
```

### RollChance (float)

Roll chance with float probability (0.0 to 1.0).

```csharp
if (0.7f.RollChance())
{
    Debug.Log("70% chance succeeded!");
}
```

### RollChance (int)

Roll chance with integer percentage (0 to 100).

```csharp
if (85.RollChance())
{
    Debug.Log("85% chance succeeded!");
}
```

### ToEnum

Convert int to enum.

```csharp
public enum Direction { North, East, South, West }
Direction dir = 2.ToEnum<Direction>(); // Direction.South
```

### ToEnumOrDefault

Convert int to enum with default fallback.

```csharp
Direction safeDir = 10.ToEnumOrDefault(Direction.North);
```

---

## String

### Bold

Apply bold rich text formatting.

```csharp
string text = "Important".Bold();
```

### Italic

Apply italic rich text formatting.

```csharp
string text = "Emphasis".Italic();
```

### Colorize

Apply color to text.

```csharp
string colored = "Error".Colorize(Swatch.CR);
```

### SplitCamelCase

Split camelCase into readable text.

```csharp
string readable = "HelloWorldTest".SplitCamelCase(); // "Hello World Test"
```

### ParseFloatWithComma

Parse float with comma as decimal separator.

```csharp
float value = "3,14".ParseFloatWithComma(); // 3.14f
```

### ToShortFormat

Format large numbers into short readable format.

```csharp
string shortFormat = 1500000.ToShortFormat(); // "1.5M"
```

### AsPercent

Convert float to percentage string.

```csharp
string percent = 0.75f.AsPercent(); // "75%"
```

### WithoutSpaces

Remove all spaces from string.

```csharp
string noSpaces = "Hello World".WithoutSpaces(); // "HelloWorld"
```

---

## Task

### Forget

Fire-and-forget task execution.

```csharp
SomeAsyncMethod().Forget();
```

### Forget (with error handling)

Fire-and-forget with error callback.

```csharp
SomeAsyncMethod().Forget(exception =>
{
    Debug.LogError($"Task failed: {exception.Message}");
});
```

---

## Time

### ToClock

Convert seconds to clock format.

```csharp
float seconds = 3665.5f;
string clockTime = seconds.ToClock(); // "01:01:05"
```

### ToReadableFormat

Convert TimeSpan to readable format.

```csharp
TimeSpan duration = TimeSpan.FromSeconds(7890);
string readable = duration.ToReadableFormat(); // "2h 11m 30s"
```

---

## Timer

### CreateCountdown

Create countdown timer from MonoBehaviour.

```csharp
var countdown = this.CreateCountdown(5f);
countdown.Start();
```

### CreateStopwatch

Create stopwatch timer from MonoBehaviour.

```csharp
var stopwatch = this.CreateStopwatch();
stopwatch.Start();
```

---

## Transform

### Clear

Destroy all child objects.

```csharp
Transform parent = transform;
parent.Clear();
```

### GetChildren

Get all direct children transforms.

```csharp
Transform[] children = transform.GetChildren();
```

### SetOrbitRotation

Set orbit position around target using angles.

```csharp
transform.SetOrbitRotation(target, 45f, 30f, 5f);
```

### LookAt2D

2D look-at for 2D games.

```csharp
Vector2 targetPosition = Vector2.right * 5f;
transform.LookAt2D(targetPosition);
```

### Distance

Calculate distance between transforms.

```csharp
float distance = transform.Distance(otherTransform);
```

### InRangeOf

Check if transform is within range of target.

```csharp
if (transform.InRangeOf(target, 5f)) { }
```

---

## Vector2/3

### InRangeOf

Check if vector is within range of target.

```csharp
Vector2 position = transform.position;
Vector2 target = Vector2.right * 5f;
if (position.InRangeOf(target, 2f)) { }
```

### DirectionTo

Get direction vector to target.

```csharp
Vector2 direction = position.DirectionTo(target);
```

### Rotate

Rotate Vector2 by angle.

```csharp
Vector2 rotated = direction.Rotate(45f);
```

### WithY

Replace Y component of Vector3.

```csharp
Vector3 worldPos = transform.position;
Vector3 onlyY = worldPos.WithY(0f);
```

### RotateY

Rotate Vector3 around Y axis.

```csharp
Vector3 rotatedY = worldPos.RotateY(45f);
```

---

## Serialize

### Serialize

Convert object to JSON string.

```csharp
[System.Serializable]
public class PlayerData
{
    public string playerName;
    public int level;
}

PlayerData data = new PlayerData { playerName = "Player1", level = 5 };
string json = data.Serialize(prettyPrint: true);
```

### Deserialize

Convert JSON string to object.

```csharp
PlayerData loadedData = json.Deserialize<PlayerData>();
```

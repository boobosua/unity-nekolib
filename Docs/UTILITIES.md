# TRnK.Toolkit Utilities

Static utility classes for common operations.

```csharp
using TRnK.Utilities;
```

## Static Utilities

### TimeUtils

#### GetWaitForSeconds()

```csharp
// Get cached WaitForSeconds to avoid memory allocation
yield return Utils.GetWaitForSeconds(1.5f);
```

#### GetWaitForSecondsRealtime()

```csharp
// Get cached WaitForSecondsRealtime for unscaled time
yield return Utils.GetWaitForSecondsRealtime(2.0f);
```

### TransformUtils

#### GetAngleFromVector()

```csharp
// Get angle in degrees from Vector3
float angle = Utils.GetAngleFromVector(direction);
```

#### GetRandomRotation()

```csharp
// Get random rotation on specific axes with custom ranges
Quaternion randomY = Utils.GetRandomRotation(Axis.Y);
Quaternion custom = Utils.GetRandomRotation(Axis.XY, new Vector2(0f, 90f), new Vector2(-45f, 45f));
```

### EnumUtils

#### GetRandomEnum()

```csharp
// Get random enum value
MyEnum randomValue = Utils.GetRandomEnum<MyEnum>();
```

#### GetRandomEnum() with exclusions

```csharp
// Get random enum excluding specific values
MyEnum randomExcluding = Utils.GetRandomEnum(MyEnum.Value1, MyEnum.Value2);
```

#### CountEnum()

```csharp
// Get count of enum values
int count = Utils.CountEnum<MyEnum>();
```

#### AllEnum()

```csharp
// Get all enum values
MyEnum[] allValues = Utils.AllEnum<MyEnum>();
```

#### AllEnum() with exclusions

```csharp
// Get all enum values excluding specific ones
MyEnum[] someValues = Utils.AllEnum(MyEnum.Value1, MyEnum.Value2);
```

#### ForEnum()

```csharp
// Iterate over all enum values
Utils.ForEnum<MyEnum>(value => Debug.Log(value));
```

### MouseUtils

#### IsMouseInGameWindow()

```csharp
// Check if mouse cursor is within game window bounds
bool isInWindow = Utils.IsMouseInGameWindow();
```

#### GetMousePosition2D()

```csharp
// Get 2D mouse world position (for orthographic cameras)
Vector2 mousePos2D = Utils.GetMousePosition2D();
```

#### GetMousePosition2D() with camera

```csharp
// Get 2D mouse world position with specific camera
Vector2 mousePos2DCustom = Utils.GetMousePosition2D(myCamera);
```

#### GetMousePosition3D()

```csharp
// Get 3D mouse world position at specified distance
Vector3 mousePos3D = Utils.GetMousePosition3D(10f);
```

#### GetMousePosition3D() with camera

```csharp
// Get 3D mouse world position with specific camera and distance
Vector3 mousePos3DCustom = Utils.GetMousePosition3D(5f, myCamera);
```

#### GetMousePosition3DFromRaycast()

```csharp
// Get 3D position from raycast hit
Vector3 raycastPos = Utils.GetMousePosition3DFromRaycast();
```

#### GetMousePosition3DFromRaycast() with layer mask

```csharp
// Get 3D position from raycast with layer filtering
Vector3 raycastPosFiltered = Utils.GetMousePosition3DFromRaycast(LayerMask.GetMask("Ground"));
```

#### GetMouseRay()

```csharp
// Get mouse ray from main camera
Ray mouseRay = Utils.GetMouseRay();
```

#### GetMouseRay() with camera

```csharp
// Get mouse ray from specific camera
Ray mouseRayCustom = Utils.GetMouseRay(myCamera);
```

### EventUtils

#### UnityEvent type aliases

```csharp
// Serialisable UnityEvent subclasses for common value types
[SerializeField] private FloatEvent onValueChanged;
[SerializeField] private IntEvent onCountChanged;
[SerializeField] private StringEvent onTextChanged;
[SerializeField] private BoolEvent onToggled;

onValueChanged.Invoke(0.5f);
onCountChanged.Invoke(3);
onTextChanged.Invoke("Hello");
onToggled.Invoke(true);
```

### TaskUtils

#### YieldTask

```csharp
// Use YieldTask to await async operations in coroutines
yield return new YieldTask(myAsyncTask);
```

## Raycast Utilities

### Raycast2DUtils

#### IsPointerOverAny2DObject()

```csharp
// Check if mouse is over any 2D collider
bool isOverObject = Utils.IsPointerOverAny2DObject();
```

#### IsPointerOverAny2DObject() with layer mask

```csharp
// Check if mouse is over any 2D collider with layer filtering
bool isOverObjectFiltered = Utils.IsPointerOverAny2DObject(LayerMask.GetMask("Interactive"));
```

#### IsPointerOverAny2DObject() with camera

```csharp
// Check if mouse is over any 2D collider with specific camera
bool isOverObjectCustom = Utils.IsPointerOverAny2DObject(myCamera);
```

#### IsPointerOverAny2DObject() with output

```csharp
// Check if mouse is over any 2D collider and get the collider
bool isOver = Utils.IsPointerOverAny2DObject(out Collider2D hit);
```

#### IsPointerOver2DObject() with component output

```csharp
// Check if mouse is over object and get a component of type T
bool isOver = Utils.IsPointerOver2DObject<Button>(out Button button);
```

### Raycast3DUtils

#### IsPointerOverAny3DObject()

```csharp
// Check if mouse is over any 3D collider
bool isOverObject = Utils.IsPointerOverAny3DObject();
```

#### IsPointerOverAny3DObject() with layer mask

```csharp
// Check if mouse is over any 3D collider with layer filtering
bool isOverObjectFiltered = Utils.IsPointerOverAny3DObject(LayerMask.GetMask("Interactive"));
```

#### IsPointerOverAny3DObject() with distance

```csharp
// Check if mouse is over any 3D collider within distance
bool isOverObjectNear = Utils.IsPointerOverAny3DObject(50f);
```

#### IsPointerOverAny3DObject() with camera

```csharp
// Check if mouse is over any 3D collider with specific camera
bool isOverObjectCustom = Utils.IsPointerOverAny3DObject(myCamera);
```

#### IsPointerOverAny3DObject() with output

```csharp
// Check if mouse is over any 3D collider and get raycast info
bool isOver = Utils.IsPointerOverAny3DObject(out RaycastHit hit);
```

#### IsPointerOver3DObject() with component output

```csharp
// Check if mouse is over object and get a component of type T
bool isOver = Utils.IsPointerOver3DObject<Interactable>(out Interactable interactable);
```

### UIElementUtils

#### IsPointerOverUI()

```csharp
// Check if pointer is over UI elements in specific layer
bool isOverUI = Utils.IsPointerOverUI(LayerMask.GetMask("UI"));
```

## Editor Utilities

### EditorUtils

#### FindAssets()

```csharp
// Find all assets of any type in a directory (Editor only)
// Works with ScriptableObjects, Sprites, AudioClips, Materials, Prefabs, etc.
MyScriptableObject[] configs = Utils.FindAssets<MyScriptableObject>("Assets/Data/");
Sprite[] icons = Utils.FindAssets<Sprite>("Assets/Art/Icons/");
AudioClip[] sfx = Utils.FindAssets<AudioClip>("Assets/Audio/SFX/");
```

#### DrawAnnulusGizmo()

```csharp
// Draw an annulus (ring) gizmo in the Scene view (Editor only, call from OnDrawGizmos)
Utils.DrawAnnulusGizmo(center, innerRadius: 2f, outerRadius: 5f);
Utils.DrawAnnulusGizmo(center, 2f, 5f, up: Vector3.up, color: Color.yellow, segments: 36);
```

#### DrawCircleGizmo()

```csharp
// Draw a circle gizmo in the Scene view (Editor only, call from OnDrawGizmos)
Utils.DrawCircleGizmo(center, radius: 3f);
Utils.DrawCircleGizmo(center, 3f, up: Vector3.forward, color: Color.cyan, segments: 64);
```

#### IsReloadDomainDisabled()

```csharp
// Check whether Domain Reload is disabled in Enter Play Mode Options
bool noDomainReload = Utils.IsReloadDomainDisabled();
```

# NekoLib Utilities

Static utility classes for common operations.

### TimeUtils

Cached WaitForSeconds to reduce garbage collection.

```csharp
// Get cached WaitForSeconds
yield return Utils.GetWaitForSeconds(1.5f);

// Get cached WaitForSecondsRealtime
yield return Utils.GetWaitForSecondsRealtime(2.0f);
```

### TransformUtils

Mathematical utilities for transforms and vectors.

```csharp
// Angle from Vector3
float angle = Utils.GetAngleFromVector(direction);

// Random rotation
Quaternion randomY = Utils.GetRandomRotation(Axis.Y);
Quaternion randomXZ = Utils.GetRandomRotation(Axis.XZ);

// Custom ranges
Quaternion custom = Utils.GetRandomRotation(
    Axis.XY,
    xRange: new Vector2(0f, 90f),
    yRange: new Vector2(-45f, 45f)
);
```

### EnumUtils

Utilities for working with enumerations.

```csharp
// Get random enum value
MyEnum randomValue = Utils.GetRandomEnum<MyEnum>();

// Get random enum excluding specific values
MyEnum randomExcluding = Utils.GetRandomEnum(MyEnum.Value1, MyEnum.Value2);

// Count enum values
int count = Utils.CountEnum<MyEnum>();

// Get all enum values (optionally excluding some)
MyEnum[] allValues = Utils.AllEnum<MyEnum>();
MyEnum[] someValues = Utils.AllEnum(MyEnum.Value1);

// Iterate over all enum values
Utils.ForEnum<MyEnum>(value => Debug.Log(value));
```

### MouseUtils

Mouse input utilities and helper functions.

```csharp
// Check if mouse is in game window
bool isInWindow = Utils.IsMouseInGameWindow();

// Get 2D mouse world position (orthographic cameras)
Vector2 mousePos2D = Utils.GetMousePosition2D();
Vector2 mousePos2DCustom = Utils.GetMousePosition2D(myCamera);

// Get 3D mouse world position at distance
Vector3 mousePos3D = Utils.GetMousePosition3D(10f);
Vector3 mousePos3DCustom = Utils.GetMousePosition3D(5f, myCamera);

// Get 3D position from raycast
Vector3 raycastPos = Utils.GetMousePosition3DFromRaycast();
Vector3 raycastPosLayer = Utils.GetMousePosition3DFromRaycast(LayerMask.GetMask("Ground"));

// Get mouse ray
Ray mouseRay = Utils.GetMouseRay();
Ray mouseRayCustom = Utils.GetMouseRay(myCamera);
```

### EventUtils

Unity event handling utilities.

```csharp
// FloatEvent class available for Unity Events
[SerializeField] private FloatEvent onValueChanged;
```

### TaskUtils

Async/await and Task utilities.

```csharp
// Use YieldTask to await tasks in coroutines
yield return new YieldTask(myAsyncTask);
```

## Raycast Utilities

### Raycast2DUtils

2D raycasting operations and helpers.

```csharp
// Check if mouse is over any 2D object
bool isOverAny = Utils.IsPointerOverAny2DObject();
bool isOverAnyLayer = Utils.IsPointerOverAny2DObject(LayerMask.GetMask("Interactive"));
bool isOverAnyCamera = Utils.IsPointerOverAny2DObject(myCamera);

// Get hit info
bool hitSomething = Utils.IsPointerOverAny2DObject(out Collider2D hit);

// Check specific object
bool isOverSpecific = Utils.IsPointerOver2DObject(myGameObject);

// Check for component type
bool hasComponent = Utils.IsPointerOver2DObject<MyComponent>();
bool hasComponentOut = Utils.IsPointerOver2DObject<MyComponent>(out MyComponent component);
```

### Raycast3DUtils

3D raycasting operations and helpers.

```csharp
// Check if mouse is over any 3D object
bool isOverAny = Utils.IsPointerOverAny3DObject();
bool isOverAnyDistance = Utils.IsPointerOverAny3DObject(10f);
bool isOverAnyLayer = Utils.IsPointerOverAny3DObject(LayerMask.GetMask("Interactive"));

// Get hit info
bool hitSomething = Utils.IsPointerOverAny3DObject(out RaycastHit hit);
bool hitSomethingCustom = Utils.IsPointerOverAny3DObject(out hit, myCamera, 15f);

// Check specific object
bool isOverSpecific = Utils.IsPointerOver3DObject(myGameObject);

// Check for component type
bool hasComponent = Utils.IsPointerOver3DObject<MyComponent>();
bool hasComponentOut = Utils.IsPointerOver3DObject<MyComponent>(out MyComponent component);
```

### UIElementUtils

UI element manipulation and utilities.

```csharp
// Check if pointer is over UI element
bool isOverUI = Utils.IsPointerOverUI(LayerMask.GetMask("UI"));
```

### EditorUtils

Editor-specific utilities (only available in editor builds).

```csharp
#if UNITY_EDITOR
// Find all assets of type in directory
MyScriptableObject[] assets = Utils.FindAllAssets<MyScriptableObject>("Assets/Data/");

// Draw gizmos in Scene view
Utils.DrawCircleGizmo(transform.position, 5f, Vector3.up, Color.red);
Utils.DrawAnnulusGizmo(transform.position, 3f, 8f, Vector3.up, Color.blue, 64);

// Check reload domain setting
bool isDisabled = Utils.IsReloadDomainDisabled();
#endif
```

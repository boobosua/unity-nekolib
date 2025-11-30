# NekoLib Components

MonoBehaviour components for common game functionality.

### SpriteAnimator

Frame-based sprite animation for SpriteRenderer.

```csharp
var animator = GetComponent<SpriteAnimator>();
animator.Play();
animator.SetFrameRate(24f);
animator.GoToFrame(5);
```

### AutoDestroy

Automatically destroys GameObject after a delay.

```csharp
var autoDestroy = gameObject.AddComponent<AutoDestroy>();
// Destroys after 5 seconds (default)
```

### LookAtCamera

Makes GameObject always face the camera.

```csharp
// Automatically faces main camera
// Perfect for billboards, nameplates, UI elements
```

### AutoOrbitAround

Automatically orbits around a target.

```csharp
var orbit = GetComponent<AutoOrbitAround>();
orbit.Target = playerTransform;
orbit.Distance = 5f;
// Orbits around player
```

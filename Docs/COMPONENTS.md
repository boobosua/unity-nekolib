# COMPONENTS

MonoBehaviour components for common game functionality.

## Timer Component

### Ticker

Configurable timer component with events and state tracking.

```csharp
// Start timer for 5 seconds
ticker.StartTimer(5f);

// Check timer state
float progress = ticker.Progress; // 0.0 to 1.0
float timeLeft = ticker.TimeLeft;
string clock = ticker.ClockFormat; // "00:05"

// Control timer
ticker.Pause();
ticker.Resume();
ticker.Stop();
```

## Destruction Component

### AutoDestroy

Automatically destroys GameObject after specified time.

```csharp
// Set destruction time in inspector or via script
autoDestroy.OnBeforeDestroy.AddListener(() => {
    Debug.Log("About to be destroyed!");
});
```

## Camera Components

### LookAtCamera

Makes object always face the camera with different modes.

```csharp
// Component automatically handles camera facing
// Configure mode in inspector:
// - LookAt: Face camera center
// - LookAtInverted: Face away from camera
// - CameraForward: Match camera direction
// - CameraForwardInverted: Opposite camera direction
```

### AutoOrbitAround

Automatically orbits around a target transform.

```csharp
// Set target, distance, and speed in inspector
// Two orbit modes available:
// - AutoHorizontalOnly: Horizontal rotation only
// - AutoVerticalOnly: Vertical rotation only
```

## Animation Components

### SpriteAnimator

Frame-based sprite animation for SpriteRenderer.

```csharp
// Assign sprites array in inspector
// Configure frame rate and loop settings
// Animation plays automatically on start
```

### UISpriteAnimator

Frame-based sprite animation for UI Image components.

```csharp
// Same as SpriteAnimator but for UI Images
// Assign sprites and configure timing in inspector
```

## Background Scrolling Components

### ScrollingImage

Scrolling background using UI Image component.

```csharp
// Basic control
scrollingImage.Play();
scrollingImage.Pause();
scrollingImage.Stop();

// Runtime speed changes
scrollingImage.SetSpeed(-2f, 0f);
scrollingImage.SetSpeedX(-3f);
```

### ScrollingRawImage

Scrolling background using RawImage component.

```csharp
// Same API as ScrollingImage
scrollingRawImage.SetSpeed(-1.5f, 0f);
bool isPlaying = scrollingRawImage.IsPlaying;
```

### ScrollingSpriteRenderer

Scrolling background using SpriteRenderer.

```csharp
// Same API, for world space objects
scrollingSpriteRenderer.SetSpeed(-2f, 0f);
Vector2 speed = scrollingSpriteRenderer.Speed;
```

### ScrollingMeshRenderer

Scrolling background using MeshRenderer.

```csharp
// Same API, best performance
scrollingMeshRenderer.SetSpeed(-2f, 0f);
scrollingMeshRenderer.Resume();
```

# Components

## Table of Contents

- [AutoDestroy](#autodestroy)
- [AutoOrbitAround](#autoorbitaround)
- [LookAtCamera](#lookatcamera)
- [Timer](#timer)

---

## AutoDestroy

Automatically destroys a GameObject after a specified lifetime.

```csharp
public class ExplosionEffect : MonoBehaviour
{
    private void Start()
    {
        // Add AutoDestroy component and set lifetime to 3 seconds
        AutoDestroy autoDestroy = gameObject.AddComponent<AutoDestroy>();
        // Configure lifetime in inspector or via serialized field
    }
}
```

---

## AutoOrbitAround

Makes a GameObject orbit around a target Transform with configurable speed and distance.

```csharp
public class SatelliteController : MonoBehaviour
{
    public Transform planet;

    private void Start()
    {
        AutoOrbitAround orbit = gameObject.AddComponent<AutoOrbitAround>();
        // Configure target, distance, and speeds in inspector
        // Supports horizontal and vertical orbit modes
    }
}
```

---

## LookAtCamera

Makes GameObjects always face the camera with different look-at modes.

```csharp
public class Billboard : MonoBehaviour
{
    private void Start()
    {
        LookAtCamera lookAtCam = gameObject.AddComponent<LookAtCamera>();
        // Configure mode (LookAt, LookAtInverted, CameraForward, CameraForwardInverted)
        // Set custom camera if needed in inspector
    }
}
```

---

## Timer

Event-driven timer component with UnityEvents for countdown functionality.

```csharp
public class GameTimer : MonoBehaviour
{
    private Timer timer;

    private void Start()
    {
        timer = GetComponent<Timer>();
        timer.SetWaitTime(10f);
        timer.OnTimeOut.AddListener(OnTimerExpired);
        timer.StartTimer();
    }

    private void OnTimerExpired()
    {
        Debug.Log("Timer finished!");
    }
}
```

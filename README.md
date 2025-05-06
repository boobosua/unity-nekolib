# NekoLib

A collection of useful extensions, utilities, and design patterns for Unity
game development.

## Timer Example Usage

```csharp
    private Countdown _countdown;
    private Countdown _reverseCountdown;
    private Stopwatch _stopwatch;

    private void Awake()
    {
        _countdown = new Countdown(5);
        _reverseCountdown = new Countdown(10);

        _stopwatch = new Stopwatch(() =>
        {
            return Input.GetKey(KeyCode.Escape);
        });
    }

    private void Start()
    {
        _countdown.Start();
        _reverseCountdown.SetLoop(true).Start();
        _stopwatch.Start();
    }

    private void OnEnable()
    {
        _countdown.OnStop += () =>
        {
            // your action when the timer stops.

            _countdown.SetTime(Random.Range(10f, 30f)).Start();
        };

        _reverseCountdown.OnStop += () =>
        {
            // your action when the timer stops.
        };
    }

    private void Update()
    {
        _countdown.Tick(Time.deltaTime);
        _reverseCountdown.Tick(Time.deltaTime);
        _stopwatch.Tick(Time.deltaTime);
    }
```

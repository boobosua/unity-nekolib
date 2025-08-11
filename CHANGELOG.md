## [1.3.3] - 2025-08-12

### Timer Component Enhacement

- Refactor timer component
- Add editor debug for timer component
- Add update event for timers

## [1.3.2] - 2025-08-11

### Refactor Timer API

- Manage every timer inside TimerManager
- Add extensions to create timers directly in Monobehavior
- Add a fluent builder pattern to create timers

## [1.3.1] - 2025-08-10

### Refactor Services

- Convert WorldTimeAPI into DateTimeManager

## [1.3.0] - 2025-08-10

### Refactor Services

- Improve and fix bugs for Singleton patterns
- Refactor NetworkManager to use UniTask instead of Coroutine

## [1.1.0] - 2025-06-20

### World API Request

- Request world time from WorldAPI
- Rework NetworkManager

## [1.0.2] - 2025-06-07

### Text Formatter

- Improve text colorization
- Add text formatting extensions (bold, italic, and underline)
- Add my own color palette
- Detect mouse over game object (2D and 3D)

## [1.0.1] - 2025-05-31

### Network Manager and Improvements

- Add a NetworkManager class to frequently check for internet reachability
- Add a YieldTask utility class to yield a task inside a coroutine
- Improve singletons

## [1.0.0] - 2025-05-07

### First Release

- Insert 3 useful Singleton components (scene, persistent, and lazy)
- Insert 2 class based timers and 1 monobehavior timer
- Some useful extensions
- Some useful utilities

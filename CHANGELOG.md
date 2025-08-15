## [1.5.2] - 2025-08-15

### Additional Vector3 and Transform Extensions

- Add new Vector3 extensions
- Add new Transform extensions
- Fix minor bugs

## [1.5.1] - 2025-08-15

### Enum Utilities and Minor Fixes

- Remove some comments
- Add Enum utilities along with int and string extensions for Enum

## [1.5.0] - 2025-08-15

### Overhaul Time and Vector Extensions

- Add more helper methods to DateTimeManager
- Add more time extensions
- Add more vector extensions (bug fixes too)
- Remove long comments

## [1.4.0] - 2025-08-13

### Fix Object Detections

- New mouse detections for 2D/3D
- New object detections for 2D/3D
- Remove camera related extensions for vector2 and vector3

## [1.3.6] - 2025-08-12

### Minor Fixes

- Testing new Task extension

## [1.3.5] - 2025-08-12

### Breaking Changes

- **Remove UniTask dependency**: Convert all async operations from UniTask to standard C# Task
- NetworkManager and DateTimeManager now use System.Threading.Tasks instead of Cysharp.Threading.Tasks
- Update all async method signatures to use Task<T> instead of UniTask<T>
- Improve cancellation token handling with proper destroyCancellationToken integration
- No external dependencies required - library is now fully self-contained

## [1.3.4] - 2025-08-12

### Fix Import Errors

- Fix dependencies section for UniTask

## [1.3.3] - 2025-08-12

### Timer Component Enhancement

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

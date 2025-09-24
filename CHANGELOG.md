## [1.9.3] - 2025-09-24

### Toolbar Features

- Add a scene switcher dropdown to the toolbar for quick scene switching.
- Add a button to clear player prefs directly from the toolbar.
- Add a time scale progress bar to visualize the current time scale.

## [1.9.2] - 2025-09-21

### Scene Switcher Tool

- Fix some UI text capitalization for better readability.
- Add support for Unity 2020.1 and newer for UIElements in the SceneSwitcherTool and StartupSceneLoaderTool.

## [1.9.0] - 2025-09-15

### Rework Timer System

- Remove TimerManager singleton.
- Add TimerRegistry component to manage all timers in a single game object.

## [1.8.6] - 2025-09-14

### New Animator, Transform, and GameObject Extensions

- Add new extensions for animator to query animation state and progress.
- Add coroutine support for waiting on animation events.
- Add new extensions for transform and game object to query for child objects in specific layer masks.

## [1.8.5] - 2025-09-07

### Update Coroutine and Task Extensions

- Add more coroutine extensions.
- Add more task extensions.

## [1.8.4] - 2025-09-07

### Network & DateTime Services

- Convert DateTimeManager to DateTimeService with static API for easy access.
- Convert NetworkManager to NetworkService with static API for easy access.
- Add more debug logs for both services.

## [1.8.3] - 2025-09-03

### Coroutine Extensions

- Add null element check for collection extensions.
- Add simple Monobehavior coroutine extensions.

## [1.8.2] - 2025-09-02

### Improve Timer System

- New Timer Tracker Window.
- New utilities for countdown and stopwatch.
- New extensions for timer.

## [1.8.1] - 2025-09-02

### Scrolling Background Components (Parallax)

- Add ScrollingImage component for UI Image backgrounds.
- Add ScrollingRawImage component for UI RawImage backgrounds.
- Add ScrollingSprite component for SpriteRenderer backgrounds.
- Add ScrollingMesh component for MeshRenderer backgrounds.
- Fix document formatting.

## [1.8.0] - 2025-09-01

### Update Library Structure

- Move singleton, timer, and color swatch to Core.
- Update other related libraries.

## [1.7.4] - 2025-09-01

### Redesign Component Inspector

- Change designs for Timer, AutoDestroy and SpriteAnimators.
- Switch back to normal UnityEvent add/remove listeners for all components.

## [1.7.3] - 2025-09-01

### Simple sprite & UI animator

- Sprite animator to animate an array of sprites on a SpriteRenderer.
- UI sprite animator to animate an array of sprites on an Image.
- Refactor singleton pattern.

## [1.7.2] - 2025-08-27

### New Time Extensions

- Suppress warnings when getting time not from online sources.
- New clock formats.

## [1.7.1] - 2025-08-26

### Camera & GameObject Extensions

- Add game object extensions to add layer mask.
- Add camera extensions related to culling mask and FOV.

## [1.7.0] - 2025-08-24

### Refactor Singleton Pattern

- Change DateTimeManager, TimerManager, and NetworkManger from Lazy to Persistent Singleton.
- Be cautious when using a LazySingleton (single thread), and a SceneSingleton (Simple but no errors handling).
- Rewrite documentations.

## [1.6.3] - 2025-08-23

### Color Utilities

- Rename color palette for shorter calls.

## [1.6.2] - 2025-08-23

### Refactor Utilities

- Add TimeSystem convenient class for shorter way to call for DateTimeManager.
- Reorganize util methods into different scripts.

## [1.6.1] - 2025-08-22

### More Components and Extensions

- Add AutoOrbitAround component.
- Adjust some text format extensions.
- Add some new draw gizmo functions.
- Add some new collection extensions.

## [1.6.0] - 2025-08-21

### Exception Handling Improvements

- Add new extensions for color, transform and game object.
- Add exception handling for some extensions.

## [1.5.3] - 2025-08-15

### Minor Improvements

- Add auto destroy component
- Add look at camera component

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

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Utilities
{
    /// <summary>
    /// Timer Tracker Window displays active timers in a tabbed interface with:
    /// - Two tabs: Countdowns and Stopwatches
    /// - Zebra striped rows for better readability
    /// - Pagination (25 items per page)
    /// - Clickable GameObjects that ping and select the object
    /// - Smooth animated progress bars with color coding:
    ///   * Blue: Running timers
    ///   * Green: Completed timers
    ///   * Yellow: Paused timers
    /// - Statistics bar showing running, paused, completed, and leaked timers
    /// - Time format: "1h25m25s (55%)" for countdowns, "1h25m25s" for stopwatches
    /// 
    /// Only available during Play Mode.
    /// </summary>
    public class TimerTrackerWindow : EditorWindow
    {
        [MenuItem("Tools/Neko Indie/Timer Tracker")]
        public static void ShowWindow()
        {
            GetWindow<TimerTrackerWindow>("Timer Tracker");
        }

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdowns", "Stopwatches" };

        // Pagination
        private const int ITEMS_PER_PAGE = 25;
        private int _currentCountdownPage = 0;
        private int _currentStopwatchPage = 0;

        // UI Colors
        private readonly Color PROGRESS_BLUE = new Color(0.3f, 0.7f, 1f, 1f);
        private readonly Color PROGRESS_GREEN = new Color(0.2f, 0.8f, 0.2f, 1f);
        private readonly Color PROGRESS_YELLOW = new Color(1f, 0.8f, 0.2f, 1f);
        private readonly Color ZEBRA_STRIPE = new Color(0f, 0f, 0f, 0.1f);

        private Vector2 _scrollPosition;

        // Smooth progress animation
        private Dictionary<TimerBase, float> _smoothProgressValues = new Dictionary<TimerBase, float>();
        private const float SMOOTH_SPEED = 5f;
        private bool _wasPlaying = false;

        private void OnEnable()
        {
            titleContent = new GUIContent("Timer Tracker", "Track active timers across the project");
            minSize = new Vector2(600, 400);
        }

        private void OnGUI()
        {
            try
            {
                if (!TimerManager.HasInstance)
                {
                    EditorGUILayout.HelpBox("TimerManager not found in the scene.", MessageType.Warning);
                    return;
                }

                var timerManager = TimerManager.Instance;

                // Check if we just exited play mode and cleanup
                if (_wasPlaying && !Application.isPlaying)
                {
                    _smoothProgressValues.Clear();
                    _selectedTab = 0;
                    _currentCountdownPage = 0;
                    _currentStopwatchPage = 0;
                    _scrollPosition = Vector2.zero;
                }
                _wasPlaying = Application.isPlaying;

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Timer Tracker is only available during Play Mode.", MessageType.Info);
                    return;
                }

                EditorGUILayout.Space(5);

                // Get timer data
                var allTimers = GetAllTimers(timerManager);
                var countdowns = allTimers.OfType<Countdown>().ToList();
                var stopwatches = allTimers.OfType<Stopwatch>().ToList();

                // Clean up smooth progress values for removed timers
                var timersToRemove = _smoothProgressValues.Keys.Where(timer => !allTimers.Contains(timer)).ToList();
                foreach (var timer in timersToRemove)
                {
                    _smoothProgressValues.Remove(timer);
                }

                // Calculate statistics
                var stats = CalculateStatistics(allTimers);

                // Draw tabs
                _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
                EditorGUILayout.Space(5);

                // Add left and right margins for the table content
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10); // Left margin

                EditorGUILayout.BeginVertical();

                // Draw content based on selected tab
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition); try
                {
                    if (_selectedTab == 0)
                    {
                        DrawCountdownTab(countdowns);
                    }
                    else
                    {
                        DrawStopwatchTab(stopwatches);
                    }
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();

                GUILayout.Space(10); // Right margin
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // Draw active timer count and compact statistics together at the bottom
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Active timer count header
                EditorGUILayout.LabelField($"Active Timers: {allTimers.Count}", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);

                // Draw compact statistics
                DrawCompactStatistics(stats);

                EditorGUILayout.EndVertical();

                // Auto-refresh
                if (Application.isPlaying)
                {
                    Repaint();
                }
            }
            catch (System.Exception e)
            {
                // Ensure we exit GUI properly on error
                GUIUtility.ExitGUI();
                Debug.LogError($"[TimerTracker] OnGUI Error: {e}");
            }
        }

        private List<TimerBase> GetAllTimers(TimerManager timerManager)
        {
            var timers = new List<TimerBase>();

            // Use reflection to access private _activeTimers field
            var field = typeof(TimerManager).GetField("_activeTimers",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                var activeTimers = field.GetValue(timerManager) as List<TimerBase>;
                if (activeTimers != null)
                {
                    timers.AddRange(activeTimers);
                }
            }

            return timers;
        }

        private void DrawCountdownTab(List<Countdown> countdowns)
        {
            if (countdowns.Count == 0)
            {
                EditorGUILayout.HelpBox("No countdowns are currently active.", MessageType.Info);
                return;
            }

            // Pagination
            int totalPages = Mathf.CeilToInt((float)countdowns.Count / ITEMS_PER_PAGE);
            _currentCountdownPage = Mathf.Clamp(_currentCountdownPage, 0, totalPages - 1);

            DrawPaginationControls(ref _currentCountdownPage, totalPages, countdowns.Count, "countdowns");

            // Get items for current page
            var pageItems = countdowns
                .Skip(_currentCountdownPage * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            // Draw countdown items
            for (int i = 0; i < pageItems.Count; i++)
            {
                DrawCountdownRow(pageItems[i], i);
            }
        }

        private void DrawStopwatchTab(List<Stopwatch> stopwatches)
        {
            if (stopwatches.Count == 0)
            {
                EditorGUILayout.HelpBox("No stopwatches are currently active.", MessageType.Info);
                return;
            }

            // Pagination
            int totalPages = Mathf.CeilToInt((float)stopwatches.Count / ITEMS_PER_PAGE);
            _currentStopwatchPage = Mathf.Clamp(_currentStopwatchPage, 0, totalPages - 1);

            DrawPaginationControls(ref _currentStopwatchPage, totalPages, stopwatches.Count, "stopwatches");

            // Get items for current page
            var pageItems = stopwatches
                .Skip(_currentStopwatchPage * ITEMS_PER_PAGE)
                .Take(ITEMS_PER_PAGE)
                .ToList();

            // Draw stopwatch items
            for (int i = 0; i < pageItems.Count; i++)
            {
                DrawStopwatchRow(pageItems[i], i);
            }
        }

        private void DrawPaginationControls(ref int currentPage, int totalPages, int totalItems, string itemType)
        {
            if (totalPages <= 1) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("◀", GUILayout.Width(30)) && currentPage > 0)
            {
                currentPage--;
            }

            EditorGUILayout.LabelField($"Page {currentPage + 1} of {totalPages} ({totalItems} {itemType})",
                EditorStyles.centeredGreyMiniLabel, GUILayout.Width(150));

            if (GUILayout.Button("▶", GUILayout.Width(30)) && currentPage < totalPages - 1)
            {
                currentPage++;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        private void DrawCountdownRow(Countdown countdown, int index)
        {
            try
            {
                // Zebra stripe background
                if (index % 2 == 1)
                {
                    Rect backgroundRect = EditorGUILayout.GetControlRect(false, 40);
                    backgroundRect.x = 0;
                    backgroundRect.width = EditorGUIUtility.currentViewWidth;
                    EditorGUI.DrawRect(backgroundRect, ZEBRA_STRIPE);
                    GUILayout.Space(-40);
                }

                EditorGUILayout.BeginHorizontal(GUILayout.Height(35));

                try
                {
                    // GameObject column
                    DrawCompactGameObjectColumn(countdown);

                    // Progress column
                    DrawCompactCountdownProgressColumn(countdown);
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(2);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimerTracker] Error drawing countdown row: {e}");
                GUIUtility.ExitGUI();
            }
        }

        private void DrawStopwatchRow(Stopwatch stopwatch, int index)
        {
            try
            {
                // Zebra stripe background
                if (index % 2 == 1)
                {
                    Rect backgroundRect = EditorGUILayout.GetControlRect(false, 40);
                    backgroundRect.x = 0;
                    backgroundRect.width = EditorGUIUtility.currentViewWidth;
                    EditorGUI.DrawRect(backgroundRect, ZEBRA_STRIPE);
                    GUILayout.Space(-40);
                }

                EditorGUILayout.BeginHorizontal(GUILayout.Height(35));

                try
                {
                    // GameObject column
                    DrawCompactGameObjectColumn(stopwatch);

                    // Progress column  
                    DrawCompactStopwatchProgressColumn(stopwatch);
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(2);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimerTracker] Error drawing stopwatch row: {e}");
                GUIUtility.ExitGUI();
            }
        }

        private void DrawCompactGameObjectColumn(TimerBase timer)
        {
            try
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(140));

                try
                {
                    string objectName = timer.Owner != null ? timer.Owner.name : "NULL";
                    string componentName = timer.OwnerComponent != null ?
                        timer.OwnerComponent.GetType().Name : "None";

                    // Main GameObject button
                    if (timer.Owner != null)
                    {
                        if (GUILayout.Button(objectName, EditorStyles.linkLabel, GUILayout.Height(16)))
                        {
                            EditorGUIUtility.PingObject(timer.Owner);
                            Selection.activeGameObject = timer.Owner;
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("🔴 NULL", EditorStyles.miniLabel, GUILayout.Height(16));
                    }

                    // Component info
                    var componentStyle = new GUIStyle(EditorStyles.miniLabel);
                    componentStyle.fontSize = 8;
                    componentStyle.fontStyle = FontStyle.Italic;
                    componentStyle.normal.textColor = Color.gray;

                    EditorGUILayout.LabelField(componentName, componentStyle, GUILayout.Height(12));
                }
                finally
                {
                    EditorGUILayout.EndVertical();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimerTracker] Error drawing GameObject column: {e}");
            }
        }

        private void DrawCompactCountdownProgressColumn(Countdown countdown)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            float inverseProgress = countdown.InverseProgress; // Counts down from 1.0 to 0.0
            float normalProgress = countdown.Progress; // Counts up from 0.0 to 1.0
            bool isCompleted = !countdown.IsRunning && normalProgress >= 1.0f;
            bool isPaused = countdown.IsPausedDueToOwner || (!countdown.IsRunning && normalProgress < 1.0f);

            // Determine progress bar color
            Color progressColor = PROGRESS_BLUE;
            if (isCompleted)
                progressColor = PROGRESS_GREEN;
            else if (isPaused)
                progressColor = PROGRESS_YELLOW;

            // Time display - show remaining time with inverse progress percentage
            float remainingTime = countdown.TotalTime - countdown.ElapsedTime;
            string timeText = $"{FormatTime(remainingTime)} ({inverseProgress * 100:F0}%)";

            // Compact progress bar - use inverse progress so bar shrinks as time counts down
            DrawCompactProgressBar(inverseProgress, timeText, progressColor, countdown);

            // Compact status info
            string statusText = "";
            if (countdown.IsLooping) statusText += $"Loop {countdown.CurrentLoopIteration + 1} ";
            if (isPaused) statusText += "Paused";
            else if (isCompleted) statusText += "Done";
            else if (countdown.UseUnscaledTime) statusText += "Unscaled";

            if (!string.IsNullOrEmpty(statusText))
            {
                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 8,
                    alignment = TextAnchor.UpperLeft
                };
                statusStyle.normal.textColor = Color.gray;
                EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Height(10));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCompactStopwatchProgressColumn(Stopwatch stopwatch)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            bool isPaused = stopwatch.IsPausedDueToOwner || !stopwatch.IsRunning;

            // Time display only (no progress bar for stopwatches)
            string timeText = $"{FormatTime(stopwatch.ElapsedTime)}";

            var timeStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft
            };
            if (isPaused)
                timeStyle.normal.textColor = PROGRESS_YELLOW;
            else
                timeStyle.normal.textColor = PROGRESS_BLUE;

            EditorGUILayout.LabelField(timeText, timeStyle, GUILayout.Height(16));

            // Compact status info
            string statusText = "";
            if (isPaused) statusText = "Paused";
            else if (stopwatch.UseUnscaledTime) statusText = "Unscaled";

            if (!string.IsNullOrEmpty(statusText))
            {
                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 8,
                    alignment = TextAnchor.UpperLeft
                };
                statusStyle.normal.textColor = Color.gray;
                EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Height(10));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCompactProgressBar(float targetProgress, string text, Color color, TimerBase timer = null)
        {
            // Smooth progress animation - only lerp while timer is running
            float displayProgress = targetProgress;
            if (timer != null && Application.isPlaying)
            {
                if (!_smoothProgressValues.ContainsKey(timer))
                {
                    _smoothProgressValues[timer] = targetProgress;
                }

                // If timer is not running (stopped/completed), instantly set to target progress
                if (!timer.IsRunning)
                {
                    _smoothProgressValues[timer] = targetProgress;
                    displayProgress = targetProgress;
                }
                else
                {
                    // Timer is running, smoothly lerp to target
                    float currentSmooth = _smoothProgressValues[timer];
                    float newSmooth = Mathf.Lerp(currentSmooth, targetProgress, Time.unscaledDeltaTime * SMOOTH_SPEED);
                    _smoothProgressValues[timer] = newSmooth;
                    displayProgress = newSmooth;
                }
            }

            Rect progressRect = EditorGUILayout.GetControlRect(false, 16);

            // Background
            Color originalColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f);
            GUI.DrawTexture(progressRect, EditorGUIUtility.whiteTexture);

            // Progress fill
            if (displayProgress > 0)
            {
                Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * displayProgress, progressRect.height);
                GUI.color = color;
                GUI.DrawTexture(fillRect, EditorGUIUtility.whiteTexture);
            }

            GUI.color = originalColor;

            // Text overlay
            var textStyle = new GUIStyle(EditorStyles.boldLabel);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontSize = 9;
            textStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUI.Label(progressRect, text, textStyle);
        }

        private TimerStatistics CalculateStatistics(List<TimerBase> timers)
        {
            var stats = new TimerStatistics();

            foreach (var timer in timers)
            {
                if (!timer.IsOwnerValid)
                {
                    stats.Leaked++;
                }
                else if (timer.IsRunning && timer.IsOwnerActiveAndEnabled)
                {
                    stats.Running++;
                }
                else if (timer.IsRunning && !timer.IsOwnerActiveAndEnabled)
                {
                    stats.Paused++;
                }
                else
                {
                    // Check if it's completed
                    if (timer is Countdown countdown && countdown.Progress >= 1.0f)
                    {
                        stats.Completed++;
                    }
                    else
                    {
                        stats.Paused++;
                    }
                }
            }

            return stats;
        }

        private void DrawCompactStatistics(TimerStatistics stats)
        {
            EditorGUILayout.BeginHorizontal();

            DrawCompactStatItem("Running", stats.Running, PROGRESS_BLUE);
            DrawCompactStatItem("Paused", stats.Paused, PROGRESS_YELLOW);
            DrawCompactStatItem("Completed", stats.Completed, PROGRESS_GREEN);
            if (stats.Leaked > 0)
            {
                DrawCompactStatItem("Leaked", stats.Leaked, Color.red);
            }

            EditorGUILayout.EndHorizontal();

            if (stats.Leaked > 0)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.HelpBox($"{stats.Leaked} leaked timer(s)", MessageType.Warning);
            }
        }

        private void DrawCompactStatItem(string label, int count, Color color)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            var countStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            countStyle.normal.textColor = color;

            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = Color.gray;

            EditorGUILayout.LabelField(count.ToString(), countStyle);
            EditorGUILayout.LabelField(label, labelStyle);

            EditorGUILayout.EndVertical();
        }

        private string FormatTime(float timeInSeconds)
        {
            int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
            int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

            if (hours > 0)
                return $"{hours}h{minutes:00}m{seconds:00}s";
            else if (minutes > 0)
                return $"{minutes}m{seconds:00}s";
            else
                return $"{seconds}s";
        }

        private struct TimerStatistics
        {
            public int Running;
            public int Paused;
            public int Completed;
            public int Leaked;
        }
    }
}
#endif
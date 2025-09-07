#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Core
{
    public class TimerTrackerWindow : EditorWindow
    {
        [MenuItem("Window/Neko Indie/Timer Tracker")]
        public static void ShowWindow()
        {
            GetWindow<TimerTrackerWindow>("Timer Tracker");
        }

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdowns", "Stopwatches" };

        // Pagination
        private const int ITEMS_PER_PAGE = 20;
        private int _currentCountdownPage = 0;
        private int _currentStopwatchPage = 0;

        // UI Colors - Enhanced for better contrast and readability
        private readonly Color RUNNING_BLUE = new(0.2f, 0.6f, 0.9f, 0.3f);      // Lighter blue background
        private readonly Color COMPLETED_GREEN = new(0.2f, 0.7f, 0.3f, 0.25f);   // Lighter green background
        private readonly Color PAUSED_YELLOW = new(0.9f, 0.8f, 0.2f, 0.2f);     // Lighter yellow background
        private readonly Color ZEBRA_STRIPE = new(0f, 0f, 0f, 0.05f);           // Lighter zebra stripe
        private readonly Color HEADER_BG = new(0.2f, 0.2f, 0.2f, 0.4f);         // Header background
        private readonly Color BORDER_COLOR = new(0.5f, 0.5f, 0.5f, 0.3f);      // Table borders

        // Text Colors
        private readonly Color RUNNING_TEXT = new(0.1f, 0.4f, 0.8f, 1f);        // Darker blue text
        private readonly Color COMPLETED_TEXT = new(0.1f, 0.5f, 0.2f, 1f);      // Darker green text
        private readonly Color PAUSED_TEXT = new(0.7f, 0.6f, 0.1f, 1f);         // Darker yellow text


        // Table configuration
        private const float ROW_HEIGHT = 38f;
        private const float HEADER_HEIGHT = 42f;

        private Vector2 _scrollPosition;

        // Smooth progress animation
        private readonly Dictionary<TimerBase, float> _smoothProgressValues = new();
        private const float SMOOTH_SPEED = 5f;
        private bool _wasPlaying = false;

        private void OnEnable()
        {
            titleContent = new GUIContent("Timer Tracker", "Track active timers across the project");
            minSize = new Vector2(900, 600);
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

                EditorGUILayout.Space(15);

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

                // Draw tabs with better styling
                var originalStyle = GUI.skin.button;
                var tabStyle = new GUIStyle(originalStyle)
                {
                    fontSize = 14,
                    fixedHeight = 35
                };

                _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, tabStyle);
                EditorGUILayout.Space(12);

                // Draw table content without margins to match tab width
                EditorGUILayout.BeginVertical();

                // Draw Excel-like table
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                try
                {
                    if (_selectedTab == 0)
                    {
                        DrawCountdownTable(countdowns);
                    }
                    else
                    {
                        DrawStopwatchTable(stopwatches);
                    }
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(20);

                // Draw statistics at the bottom
                DrawStatisticsPanel(stats, allTimers.Count);

                // Auto-refresh
                if (Application.isPlaying)
                {
                    Repaint();
                }
            }
            catch (System.Exception e)
            {
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

        private void DrawCountdownTable(List<Countdown> countdowns)
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

            // Draw table header
            DrawCountdownTableHeader();

            // Draw table rows
            for (int i = 0; i < pageItems.Count; i++)
            {
                DrawCountdownTableRow(pageItems[i], i);
            }
        }

        private void DrawStopwatchTable(List<Stopwatch> stopwatches)
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

            // Draw table header
            DrawStopwatchTableHeader();

            // Draw table rows
            for (int i = 0; i < pageItems.Count; i++)
            {
                DrawStopwatchTableRow(pageItems[i], i);
            }
        }

        private void DrawCountdownTableHeader()
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, HEADER_HEIGHT);

            // Header background
            EditorGUI.DrawRect(headerRect, HEADER_BG);

            // Header borders
            DrawTableBorders(headerRect);

            // Dynamic column widths - flexible and proportional (full width to match tabs)
            float totalWidth = headerRect.width; // Use full width
            float gameObjectWidth = totalWidth * 0.25f; // 25% for GameObject
            float totalTimeWidth = totalWidth * 0.15f;  // 15% for Total Time
            float currentTimeWidth = totalWidth * 0.15f; // 15% for Remaining
            float progressWidth = totalWidth * 0.3f;     // 30% for Progress
            float statusWidth = totalWidth * 0.15f;      // 15% for Status

            float x = headerRect.x;
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // Headers with vertical dividers
            DrawHeaderColumn(new Rect(x, headerRect.y, gameObjectWidth, headerRect.height), "GameObject", headerStyle);
            x += gameObjectWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, totalTimeWidth, headerRect.height), "Total Time", headerStyle);
            x += totalTimeWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, currentTimeWidth, headerRect.height), "Remaining", headerStyle);
            x += currentTimeWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, progressWidth, headerRect.height), "Progress", headerStyle);
            x += progressWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, statusWidth, headerRect.height), "Status", headerStyle);
        }

        private void DrawStopwatchTableHeader()
        {
            Rect headerRect = EditorGUILayout.GetControlRect(false, HEADER_HEIGHT);

            // Header background
            EditorGUI.DrawRect(headerRect, HEADER_BG);

            // Header borders
            DrawTableBorders(headerRect);

            // Dynamic column widths - flexible and proportional (full width to match tabs)
            float totalWidth = headerRect.width; // Use full width
            float gameObjectWidth = totalWidth * 0.4f;   // 40% for GameObject
            float elapsedTimeWidth = totalWidth * 0.3f;  // 30% for Elapsed Time
            float statusWidth = totalWidth * 0.3f;       // 30% for Status

            float x = headerRect.x;
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // Headers with vertical dividers
            DrawHeaderColumn(new Rect(x, headerRect.y, gameObjectWidth, headerRect.height), "GameObject", headerStyle);
            x += gameObjectWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, elapsedTimeWidth, headerRect.height), "Elapsed Time", headerStyle);
            x += elapsedTimeWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, statusWidth, headerRect.height), "Status", headerStyle);
        }

        private void DrawCountdownTableRow(Countdown countdown, int index)
        {
            try
            {
                Rect rowRect = EditorGUILayout.GetControlRect(false, ROW_HEIGHT);

                // Light zebra stripe background only (no colored row highlighting)
                if (index % 2 == 1)
                {
                    EditorGUI.DrawRect(rowRect, ZEBRA_STRIPE);
                }

                // Row borders
                DrawTableBorders(rowRect);

                // Dynamic column widths matching header (full width to match tabs)
                float totalWidth = rowRect.width; // Use full width
                float gameObjectWidth = totalWidth * 0.25f; // 25% for GameObject
                float totalTimeWidth = totalWidth * 0.15f;  // 15% for Total Time
                float currentTimeWidth = totalWidth * 0.15f; // 15% for Remaining
                float progressWidth = totalWidth * 0.3f;     // 30% for Progress
                float statusWidth = totalWidth * 0.15f;      // 15% for Status

                float x = rowRect.x;

                // Determine timer state for progress bar and status text
                bool isCompleted = !countdown.IsRunning && countdown.Progress >= 1.0f;
                bool isPaused = countdown.IsPausedDueToOwner || (!countdown.IsRunning && countdown.Progress < 1.0f);

                // Use dimmed text color for content to distinguish from headers
                Color baseTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                Color dimmedTextColor = Color.Lerp(baseTextColor, Color.gray, 0.3f);

                // Status text color matches progress bar color
                Color statusTextColor;
                if (isCompleted)
                    statusTextColor = COMPLETED_TEXT;
                else if (isPaused)
                    statusTextColor = PAUSED_TEXT;
                else if (countdown.IsRunning)
                    statusTextColor = RUNNING_TEXT;
                else
                    statusTextColor = dimmedTextColor;

                // GameObject column
                DrawGameObjectColumn(new Rect(x, rowRect.y, gameObjectWidth, rowRect.height), countdown, dimmedTextColor);
                x += gameObjectWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Total Time column
                DrawCenteredText(new Rect(x, rowRect.y, totalTimeWidth, rowRect.height),
                    FormatTime(countdown.TotalTime), dimmedTextColor, 12);
                x += totalTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Remaining Time column
                float remainingTime = countdown.ElapsedTime;
                DrawCenteredText(new Rect(x, rowRect.y, currentTimeWidth, rowRect.height),
                    FormatTime(remainingTime), dimmedTextColor, 12);
                x += currentTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Progress column (ONLY the progress bar gets colored based on timer state)
                DrawProgressColumn(new Rect(x, rowRect.y, progressWidth, rowRect.height),
                    countdown.InverseProgress, countdown.Progress, isCompleted, isPaused, countdown.IsRunning, countdown);
                x += progressWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Status column
                string status = GetTimerStatus(countdown, isCompleted, isPaused);
                DrawCenteredText(new Rect(x, rowRect.y, statusWidth, rowRect.height), status.ToUpper(), statusTextColor, 11);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimerTracker] Error drawing countdown row: {e}");
            }
        }

        private void DrawStopwatchTableRow(Stopwatch stopwatch, int index)
        {
            try
            {
                Rect rowRect = EditorGUILayout.GetControlRect(false, ROW_HEIGHT);

                // Light zebra stripe background only (no colored row highlighting)
                if (index % 2 == 1)
                {
                    EditorGUI.DrawRect(rowRect, ZEBRA_STRIPE);
                }

                // Row borders
                DrawTableBorders(rowRect);

                // Dynamic column widths matching header (full width to match tabs)
                float totalWidth = rowRect.width; // Use full width
                float gameObjectWidth = totalWidth * 0.4f;   // 40% for GameObject
                float elapsedTimeWidth = totalWidth * 0.3f;  // 30% for Elapsed Time
                float statusWidth = totalWidth * 0.3f;       // 30% for Status

                float x = rowRect.x;

                // Determine timer state for status text
                bool isPaused = stopwatch.IsPausedDueToOwner || !stopwatch.IsRunning;

                // Use dimmed text color for content to distinguish from headers
                Color baseTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                Color dimmedTextColor = Color.Lerp(baseTextColor, Color.gray, 0.3f);

                // Status text color matches progress bar color
                Color statusTextColor;
                if (isPaused)
                    statusTextColor = PAUSED_TEXT;
                else if (stopwatch.IsRunning)
                    statusTextColor = RUNNING_TEXT;
                else
                    statusTextColor = dimmedTextColor;

                // GameObject column
                DrawGameObjectColumn(new Rect(x, rowRect.y, gameObjectWidth, rowRect.height), stopwatch, dimmedTextColor);
                x += gameObjectWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Elapsed Time column
                DrawCenteredText(new Rect(x, rowRect.y, elapsedTimeWidth, rowRect.height),
                    FormatTime(stopwatch.ElapsedTime), dimmedTextColor, 13);
                x += elapsedTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Status column
                string status = GetTimerStatus(stopwatch, false, isPaused);
                DrawCenteredText(new Rect(x, rowRect.y, statusWidth, rowRect.height), status.ToUpper(), statusTextColor, 11);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimerTracker] Error drawing stopwatch row: {e}");
            }
        }

        private void DrawTableBorders(Rect rect)
        {
            // Bottom border
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), BORDER_COLOR);
        }

        private void DrawHeaderColumn(Rect rect, string text, GUIStyle style)
        {
            GUI.Label(rect, text, style);
        }

        private void DrawVerticalDivider(float x, float y, float height)
        {
            EditorGUI.DrawRect(new Rect(x - 1, y, 1, height), BORDER_COLOR);
        }

        private void DrawGameObjectColumn(Rect rect, TimerBase timer, Color textColor)
        {
            var centeredRect = new Rect(rect.x + 8, rect.y, rect.width - 16, rect.height);

            string objectName = timer.Owner != null ? timer.Owner.name : "NULL";
            string componentName = timer.OwnerComponent != null ?
                timer.OwnerComponent.GetType().Name : "None";

            // GameObject name (clickable)
            var objectStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };
            objectStyle.normal.textColor = textColor;

            if (timer.Owner != null && GUI.Button(
                new Rect(centeredRect.x, centeredRect.y + 4, centeredRect.width, 16),
                objectName, objectStyle))
            {
                EditorGUIUtility.PingObject(timer.Owner);
                Selection.activeGameObject = timer.Owner;
            }

            // Component name
            var componentStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Italic
            };
            componentStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.4f);

            GUI.Label(new Rect(centeredRect.x, centeredRect.y + 22, centeredRect.width, 12),
                componentName, componentStyle);
        }

        private void DrawCenteredText(Rect rect, string text, Color textColor, int fontSize)
        {
            var textStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            textStyle.normal.textColor = textColor;

            GUI.Label(rect, text, textStyle);
        }

        private void DrawProgressColumn(Rect rect, float inverseProgress, float normalProgress, bool isCompleted, bool isPaused, bool isRunning, TimerBase timer)
        {
            var progressRect = new Rect(rect.x + 8, rect.y + 8, rect.width - 16, 22);

            // Smooth progress animation
            float displayProgress = inverseProgress;
            if (timer != null && Application.isPlaying)
            {
                if (!_smoothProgressValues.ContainsKey(timer))
                {
                    _smoothProgressValues[timer] = inverseProgress;
                }

                if (!timer.IsRunning)
                {
                    _smoothProgressValues[timer] = inverseProgress;
                    displayProgress = inverseProgress;
                }
                else
                {
                    float currentSmooth = _smoothProgressValues[timer];
                    float newSmooth = Mathf.Lerp(currentSmooth, inverseProgress, Time.unscaledDeltaTime * SMOOTH_SPEED);
                    _smoothProgressValues[timer] = newSmooth;
                    displayProgress = newSmooth;
                }
            }

            // Progress bar background
            Color bgColor = EditorGUIUtility.isProSkin ? new Color(0.2f, 0.2f, 0.2f, 0.8f) : new Color(0.8f, 0.8f, 0.8f, 0.8f);
            EditorGUI.DrawRect(progressRect, bgColor);

            // Progress bar fill with state-based colors
            if (displayProgress > 0)
            {
                Rect fillRect = new Rect(progressRect.x, progressRect.y,
                    progressRect.width * displayProgress, progressRect.height);

                Color fillColor;
                if (isCompleted)
                    fillColor = COMPLETED_GREEN;
                else if (isPaused)
                    fillColor = PAUSED_YELLOW;
                else if (isRunning)
                    fillColor = RUNNING_BLUE;
                else
                    fillColor = Color.gray;

                EditorGUI.DrawRect(fillRect, fillColor);
            }

            // Progress text
            string progressText = inverseProgress.AsPercent();
            var progressTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };
            progressTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            GUI.Label(progressRect, progressText, progressTextStyle);
        }

        private string GetTimerStatus(TimerBase timer, bool isCompleted, bool isPaused)
        {
            if (isCompleted)
                return "Completed";
            else if (isPaused)
                return timer.IsPausedDueToOwner ? "Owner Disabled" : "Paused";
            else if (timer.IsRunning)
            {
                string status = "Running";
                if (timer.UseUnscaledTime) status += " (Unscaled)";
                if (timer is Countdown countdown && countdown.IsLooping)
                    status += $" Loop {countdown.CurrentLoopIteration + 1}";
                return status;
            }
            return "Stopped";
        }

        private void DrawPaginationControls(ref int currentPage, int totalPages, int totalItems, string itemType)
        {
            if (totalPages <= 1) return;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fixedHeight = 28
            };

            if (GUILayout.Button("◀", buttonStyle, GUILayout.Width(40)) && currentPage > 0)
            {
                currentPage--;
            }

            var labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            labelStyle.fontSize = 12;
            EditorGUILayout.LabelField($"Page {currentPage + 1} of {totalPages} ({totalItems} {itemType})",
                labelStyle, GUILayout.Width(200));

            if (GUILayout.Button("▶", buttonStyle, GUILayout.Width(40)) && currentPage < totalPages - 1)
            {
                currentPage++;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(8);
        }

        private void DrawStatisticsPanel(TimerStatistics stats, int totalCount)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Title
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField($"Timer Statistics ({totalCount} Total)", titleStyle, GUILayout.Height(30));

            EditorGUILayout.Space(8);

            // Statistics grid
            EditorGUILayout.BeginHorizontal();

            // Use default text color for all stat cards, only backgrounds are colored
            Color defaultTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            DrawStatCard("Running", stats.Running, defaultTextColor, RUNNING_BLUE);
            DrawStatCard("Paused", stats.Paused, defaultTextColor, PAUSED_YELLOW);
            DrawStatCard("Completed", stats.Completed, defaultTextColor, COMPLETED_GREEN);

            if (stats.Leaked > 0)
            {
                DrawStatCard("Leaked", stats.Leaked, Color.red, new Color(1f, 0.3f, 0.3f, 0.2f));
            }

            EditorGUILayout.EndHorizontal();

            if (stats.Leaked > 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox($"{stats.Leaked} leaked timer(s) detected - their GameObjects have been destroyed", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatCard(string label, int count, Color textColor, Color bgColor)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(75));

            // Background
            Rect cardRect = EditorGUILayout.GetControlRect(false, 70);
            EditorGUI.DrawRect(cardRect, bgColor);

            // Border
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, cardRect.width, 1), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y + cardRect.height - 1, cardRect.width, 1), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 1, cardRect.height), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x + cardRect.width - 1, cardRect.y, 1, cardRect.height), BORDER_COLOR);

            // Count
            var countStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            countStyle.normal.textColor = textColor;

            GUI.Label(new Rect(cardRect.x, cardRect.y + 12, cardRect.width, 25), count.ToString(), countStyle);

            // Label
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.3f);

            GUI.Label(new Rect(cardRect.x, cardRect.y + 42, cardRect.width, 18), label, labelStyle);

            EditorGUILayout.EndVertical();
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
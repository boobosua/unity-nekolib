#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Core
{
    public class TimerTrackerWindow : EditorWindow
    {
        [MenuItem("Window/Neko Framework/Timer Tracker")]
        public static void ShowWindow()
        {
            GetWindow<TimerTrackerWindow>("Timer Tracker");
        }

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdowns", "Stopwatches", "Pooling" };

        // Pagination (compact style via EditorPagination utility)
        private const int ITEMS_PER_PAGE = 20; // enforce 20 items per page
        private EditorPagination.State _countdownPageState;
        private EditorPagination.State _stopwatchPageState;

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


        // Table configuration (compact but tall enough for 2-line object/component)
        private const float ROW_HEIGHT = 30f;
        private const float HEADER_HEIGHT = 26f;

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
                // Check if we just exited play mode and cleanup
                if (_wasPlaying && !Application.isPlaying)
                {
                    _smoothProgressValues.Clear();
                    _selectedTab = 0;
                    _countdownPageState = default;
                    _stopwatchPageState = default;
                    _scrollPosition = Vector2.zero;
                }
                _wasPlaying = Application.isPlaying;

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Timer Tracker is only available during Play Mode.", MessageType.Info);
                    return;
                }

                EditorGUILayout.Space(4);

                // Get timer data from the global PlayerLoop driver
                var allTimers = GetAllTimers();
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
                _selectedTab = NekoEditorTabBar.Draw(_selectedTab, _tabNames, 24f);
                EditorGUILayout.Space(2);

                // Handle Pooling tab separately (full-page view)
                if (_selectedTab == 2)
                {
                    // Prevent visual overlap: reset scroll for pooling page
                    _scrollPosition = Vector2.zero;

                    DrawPoolingFullPage(countdowns.Count, stopwatches.Count);
                }
                else
                {
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

                    EditorGUILayout.Space(6);
                    DrawStatisticsPanel(stats, allTimers.Count);
                }

                // Auto-refresh
                if (Application.isPlaying)
                {
                    Repaint();
                }
            }
            catch (System.Exception e)
            {
                GUIUtility.ExitGUI();
                Log.Error($"[TimerTracker] OnGUI Error: {e}");
            }
        }

        private List<TimerBase> GetAllTimers()
        {
            if (!Application.isPlaying)
                return new List<TimerBase>();

            // Read directly from the global PlayerLoop driver
            return TimerPlayerLoopDriver.GetActiveTimersSnapshot();
        }

        private void DrawCountdownTable(List<Countdown> countdowns)
        {
            if (countdowns.Count == 0)
            {
                EditorGUILayout.HelpBox("No countdowns are currently active.", MessageType.Info);
                return;
            }

            // Pagination via utility
            var slice = EditorPagination.Draw(ref _countdownPageState, countdowns.Count, ITEMS_PER_PAGE, HEADER_HEIGHT - 8, "Countdown", "Countdowns");

            // Get items for current slice
            var pageItems = countdowns.Skip(slice.Start).Take(slice.End - slice.Start).ToList();

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

            // Pagination via utility
            var slice = EditorPagination.Draw(ref _stopwatchPageState, stopwatches.Count, ITEMS_PER_PAGE, HEADER_HEIGHT - 8, "Stopwatch", "Stopwatches");

            // Get items for current slice
            var pageItems = stopwatches.Skip(slice.Start).Take(slice.End - slice.Start).ToList();

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
            var headerStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
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
            var headerStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
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
                float progressWidth = totalWidth * 0.30f;     // 30% for Progress
                float statusWidth = totalWidth * 0.15f;      // 15% for Status

                float x = rowRect.x;

                // Determine timer state for progress bar and status text
                bool ownerActive = countdown.IsOwnerActiveAndEnabled;
                // Countdown Progress is remaining/total, so completion is when remaining ~ 0
                bool reachedEnd = countdown.RemainTime <= 0.0001f || countdown.InverseProgress >= 0.999f;
                bool manualStopNonLoop = !countdown.IsRunning && ownerActive && !countdown.IsLooping && !reachedEnd;
                bool isCompleted = !countdown.IsRunning && (reachedEnd || manualStopNonLoop);
                bool isPaused = !isCompleted && (countdown.IsPausedDueToOwner || (countdown.IsRunning && !ownerActive));

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
                    FormatTime(countdown.TotalTime), dimmedTextColor, 10);
                x += totalTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Remaining Time column
                float remainingTime = countdown.RemainTime;
                DrawCenteredText(new Rect(x, rowRect.y, currentTimeWidth, rowRect.height),
                    FormatTime(remainingTime), dimmedTextColor, 10);
                x += currentTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Progress column (ONLY the progress bar gets colored based on timer state)
                DrawProgressColumn(new Rect(x, rowRect.y, progressWidth, rowRect.height),
                    countdown.InverseProgress, countdown.Progress, isCompleted, isPaused, countdown.IsRunning, countdown);
                x += progressWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Status column
                string status = GetTimerStatus(countdown, isCompleted, isPaused);
                DrawCenteredText(new Rect(x, rowRect.y, statusWidth, rowRect.height), status.ToUpper(), statusTextColor, 9);
            }
            catch (System.Exception e)
            {
                Log.Error($"[TimerTracker] Error drawing countdown row: {e}");
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
                float gameObjectWidth = totalWidth * 0.40f;   // 40% for GameObject
                float elapsedTimeWidth = totalWidth * 0.30f;  // 30% for Elapsed Time
                float statusWidth = totalWidth * 0.30f;       // 30% for Status

                float x = rowRect.x;

                // Determine timer state for status text
                bool ownerActive = stopwatch.IsOwnerActiveAndEnabled;
                bool isCompleted = !stopwatch.IsRunning && stopwatch.IsOwnerValid && ownerActive;
                bool isPaused = !isCompleted && (stopwatch.IsPausedDueToOwner || (stopwatch.IsRunning && !ownerActive));

                // Use dimmed text color for content to distinguish from headers
                Color baseTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                Color dimmedTextColor = Color.Lerp(baseTextColor, Color.gray, 0.3f);

                // Status text color matches progress bar color
                Color statusTextColor;
                if (isCompleted)
                    statusTextColor = COMPLETED_TEXT;
                else if (isPaused)
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
                    FormatTime(stopwatch.RemainTime), dimmedTextColor, 10);
                x += elapsedTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Status column
                string status = GetTimerStatus(stopwatch, isCompleted, isPaused);
                DrawCenteredText(new Rect(x, rowRect.y, statusWidth, rowRect.height), status.ToUpper(), statusTextColor, 9);
            }
            catch (System.Exception e)
            {
                Log.Error($"[TimerTracker] Error drawing stopwatch row: {e}");
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
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            objectStyle.normal.textColor = textColor;

            if (timer.Owner != null && GUI.Button(
                new Rect(centeredRect.x, centeredRect.y + 3, centeredRect.width, 14),
                objectName, objectStyle))
            {
                EditorGUIUtility.PingObject(timer.Owner);
                Selection.activeGameObject = timer.Owner;
            }

            // Component name
            var componentStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Italic,
                padding = new RectOffset(0, 0, 0, 0)
            };
            componentStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.4f);

            GUI.Label(new Rect(centeredRect.x, centeredRect.y + 17, centeredRect.width, 11),
                componentName, componentStyle);
        }

        private void DrawCenteredText(Rect rect, string text, Color textColor, int fontSize)
        {
            var textStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            textStyle.normal.textColor = textColor;
            var rr = new Rect(rect.x, rect.y - 1, rect.width, rect.height + 2); // slight nudge for perfect vertical centering
            GUI.Label(rr, text, textStyle);
        }

        private void DrawProgressColumn(Rect rect, float inverseProgress, float normalProgress, bool isCompleted, bool isPaused, bool isRunning, TimerBase timer)
        {
            var progressRect = new Rect(rect.x + 6, rect.y + 4, rect.width - 12, 16);

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
            var progressTextStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            progressTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var pr = new Rect(progressRect.x, progressRect.y - 1, progressRect.width, progressRect.height + 2);
            GUI.Label(pr, progressText, progressTextStyle);
        }

        private string GetTimerStatus(TimerBase timer, bool isCompleted, bool isPaused)
        {
            if (isCompleted)
                return "Done";

            if (isPaused)
            {
                if (timer.IsPausedDueToOwner)
                    return "Owner Off";
                return "Paused";
            }

            if (timer.IsRunning)
            {
                string status = "Running";
                if (timer.UseUnscaledTime) status += " (Unscaled)";
                if (timer is Countdown countdown && countdown.IsLooping)
                    status += $" Loop {countdown.CurrentLoopIteration + 1}";
                return status;
            }

            // Non-running and not completed (e.g., manually stopped before start or state indeterminate)
            return "Stopped";
        }

        private void DrawStatisticsPanel(TimerStatistics stats, int totalCount)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Title
            var titleStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            EditorGUILayout.LabelField($"Timers ({totalCount})", titleStyle, GUILayout.Height(18));
            EditorGUILayout.Space(2);

            // Statistics grid
            EditorGUILayout.BeginHorizontal();

            // Use default text color for all stat cards, only backgrounds are colored
            Color defaultTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            DrawStatCard("Run", stats.Running, defaultTextColor, RUNNING_BLUE);
            DrawStatCard("Pause", stats.Paused, defaultTextColor, PAUSED_YELLOW);
            DrawStatCard("Done", stats.Completed, defaultTextColor, COMPLETED_GREEN);

            if (stats.Leaked > 0)
            {
                DrawStatCard("Leaked", stats.Leaked, Color.red, new Color(1f, 0.3f, 0.3f, 0.2f));
            }

            EditorGUILayout.EndHorizontal();

            if (stats.Leaked > 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox($"{stats.Leaked} leaked timer(s) detected - their GameObjects have been destroyed", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawStatCard(string label, int count, Color textColor, Color bgColor)
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.Height(54));

            // Background
            Rect cardRect = EditorGUILayout.GetControlRect(false, 50);
            EditorGUI.DrawRect(cardRect, bgColor);

            // Border
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, cardRect.width, 1), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y + cardRect.height - 1, cardRect.width, 1), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, 1, cardRect.height), BORDER_COLOR);
            EditorGUI.DrawRect(new Rect(cardRect.x + cardRect.width - 1, cardRect.y, 1, cardRect.height), BORDER_COLOR);

            // Count
            var countStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            countStyle.normal.textColor = textColor;

            GUI.Label(new Rect(cardRect.x, cardRect.y + 10, cardRect.width, 18), count.ToString(), countStyle);

            // Label
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            labelStyle.normal.textColor = Color.Lerp(textColor, Color.gray, 0.3f);

            GUI.Label(new Rect(cardRect.x, cardRect.y + 28, cardRect.width, 14), label, labelStyle);

            EditorGUILayout.EndVertical();
        }

        private void DrawPoolingFullPage(int activeCountdowns, int activeStopwatches)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Title
            var titleStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            EditorGUILayout.LabelField("Pooling", titleStyle, GUILayout.Height(20));
            EditorGUILayout.Space(4);

            // Top summary cards
            EditorGUILayout.BeginHorizontal();
            var defaultColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            DrawStatCard("Active", TimerPlayerLoopDriver.ActiveTimerCount, defaultColor, RUNNING_BLUE);
            DrawStatCard("CD Pool", TimerPlayerLoopDriver.CountdownPoolCount, defaultColor, COMPLETED_GREEN);
            DrawStatCard("SW Pool", TimerPlayerLoopDriver.StopwatchPoolCount, defaultColor, COMPLETED_GREEN);
            DrawStatCard("Max Pool", TimerPlayerLoopDriver.MaxPoolSize, defaultColor, ZEBRA_STRIPE);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            // Details section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Active Countdowns: {activeCountdowns}");
            EditorGUILayout.LabelField($"Active Stopwatches: {activeStopwatches}");
            EditorGUILayout.LabelField($"Countdown Pool Available: {TimerPlayerLoopDriver.CountdownPoolCount}");
            EditorGUILayout.LabelField($"Stopwatch Pool Available: {TimerPlayerLoopDriver.StopwatchPoolCount}");
            EditorGUILayout.LabelField($"Max Pool Size (per type approx.): {TimerPlayerLoopDriver.MaxPoolSize}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private TimerStatistics CalculateStatistics(List<TimerBase> timers)
        {
            var stats = new TimerStatistics();

            foreach (var timer in timers)
            {
                bool ownerValid = timer.IsOwnerValid;
                bool ownerActive = timer.IsOwnerActiveAndEnabled;

                if (!ownerValid)
                {
                    stats.Leaked++;
                    continue;
                }

                // Running (active and enabled)
                if (timer.IsRunning && ownerActive)
                {
                    stats.Running++;
                    continue;
                }

                // Paused due to owner disabled/inactive
                if (timer.IsPausedDueToOwner || (timer.IsRunning && !ownerActive))
                {
                    stats.Paused++;
                    continue;
                }

                // Not running here and owner is valid (likely either Paused or Done)
                if (timer is Stopwatch)
                {
                    // Stopwatches are considered Done when not running
                    stats.Completed++;
                    continue;
                }

                if (timer is Countdown cd)
                {
                    // Done if reached end OR manually stopped (non-loop) while owner is active
                    bool reachedEnd = cd.Progress >= 1.0f || cd.RemainTime <= 0.0001f;
                    bool manualStopNonLoop = !cd.IsRunning && ownerActive && !cd.IsLooping && !reachedEnd; // treat manual stop as done

                    if (reachedEnd || manualStopNonLoop)
                    {
                        stats.Completed++;
                    }
                    else
                    {
                        stats.Paused++;
                    }
                    continue;
                }

                // Fallback
                stats.Paused++;
            }

            return stats;
        }

        private string FormatTime(float timeInSeconds)
        {
            return timeInSeconds.ToReadableFormat();
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
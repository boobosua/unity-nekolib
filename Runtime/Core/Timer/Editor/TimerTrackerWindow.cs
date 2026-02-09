#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using NekoLib.Extensions;
using NekoLib.Logger;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Core
{
    /// <summary>Editor window that displays active countdowns/stopwatches while in Play Mode.</summary>
    public class TimerTrackerWindow : EditorWindow
    {
        [MenuItem("Window/Neko Framework/Timer Tracker")]
        /// <summary>Opens the Timer Tracker window.</summary>
        public static void ShowWindow()
        {
            GetWindow<TimerTrackerWindow>("Timer Tracker");
        }

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdowns", "Stopwatches" };

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

        private const float COUNTDOWN_GAMEOBJECT_COL_WIDTH = 160f;
        private const float STOPWATCH_GAMEOBJECT_COL_WIDTH = 160f;

        private Vector2 _scrollPosition;

        // Smooth progress animation
        private readonly Dictionary<ulong, float> _smoothProgressValues = new();
        private const float SMOOTH_SPEED = 5f;
        private bool _wasPlaying = false;

        private readonly List<TimerPlayerLoopDriver.TimerDebugInfo> _allTimers = new(128);
        private readonly List<TimerPlayerLoopDriver.TimerDebugInfo> _countdowns = new(128);
        private readonly List<TimerPlayerLoopDriver.TimerDebugInfo> _stopwatches = new(128);
        private readonly HashSet<ulong> _timerKeys = new();
        private readonly List<ulong> _keysToRemove = new(64);

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
                GetAllTimers(_allTimers);
                SplitTimers(_allTimers, _countdowns, _stopwatches);

                // Clean up smooth progress values for removed timers
                CleanupSmoothProgress(_allTimers);

                // Draw tabs with better styling
                _selectedTab = NekoEditorTabBar.Draw(_selectedTab, _tabNames, 24f);
                EditorGUILayout.Space(2);

                // Draw table content without margins to match tab width
                EditorGUILayout.BeginVertical();

                bool hasRows;
                EditorPagination.Slice slice;
                if (_selectedTab == 0)
                {
                    hasRows = _countdowns.Count > 0;
                    if (!hasRows)
                    {
                        EditorGUILayout.HelpBox("No countdowns are currently active.", MessageType.Info);
                        slice = default;
                    }
                    else
                    {
                        slice = EditorPagination.Draw(ref _countdownPageState, _countdowns.Count, ITEMS_PER_PAGE, HEADER_HEIGHT - 8, "Countdown", "Countdowns");
                    }
                }
                else
                {
                    hasRows = _stopwatches.Count > 0;
                    if (!hasRows)
                    {
                        EditorGUILayout.HelpBox("No stopwatches are currently active.", MessageType.Info);
                        slice = default;
                    }
                    else
                    {
                        slice = EditorPagination.Draw(ref _stopwatchPageState, _stopwatches.Count, ITEMS_PER_PAGE, HEADER_HEIGHT - 8, "Stopwatch", "Stopwatches");
                    }
                }

                // Draw Excel-like table
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                try
                {
                    if (!hasRows)
                    {
                        // Keep scroll view valid even with no rows.
                    }
                    else if (_selectedTab == 0)
                    {
                        DrawCountdownTable(_countdowns, slice);
                    }
                    else
                    {
                        DrawStopwatchTable(_stopwatches, slice);
                    }
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(6);
                DrawPoolingBottomBar();

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

        private void GetAllTimers(List<TimerPlayerLoopDriver.TimerDebugInfo> results)
        {
            TimerPlayerLoopDriver.GetActiveTimersSnapshot(results);
        }

        private static void SplitTimers(
            List<TimerPlayerLoopDriver.TimerDebugInfo> allTimers,
            List<TimerPlayerLoopDriver.TimerDebugInfo> countdowns,
            List<TimerPlayerLoopDriver.TimerDebugInfo> stopwatches)
        {
            countdowns.Clear();
            stopwatches.Clear();

            for (int i = 0; i < allTimers.Count; i++)
            {
                var timer = allTimers[i];
                if (timer.Kind == TimerPlayerLoopDriver.TimerKind.Countdown)
                {
                    countdowns.Add(timer);
                }
                else if (timer.Kind == TimerPlayerLoopDriver.TimerKind.Stopwatch)
                {
                    stopwatches.Add(timer);
                }
            }
        }

        private void CleanupSmoothProgress(List<TimerPlayerLoopDriver.TimerDebugInfo> allTimers)
        {
            _timerKeys.Clear();
            for (int i = 0; i < allTimers.Count; i++)
            {
                _timerKeys.Add(allTimers[i].Key);
            }

            _keysToRemove.Clear();
            foreach (var key in _smoothProgressValues.Keys)
            {
                if (!_timerKeys.Contains(key))
                {
                    _keysToRemove.Add(key);
                }
            }

            for (int i = 0; i < _keysToRemove.Count; i++)
            {
                _smoothProgressValues.Remove(_keysToRemove[i]);
            }
        }

        private void DrawCountdownTable(List<TimerPlayerLoopDriver.TimerDebugInfo> countdowns, EditorPagination.Slice slice)
        {
            // Draw table header
            DrawCountdownTableHeader();

            // Draw table rows
            for (int i = slice.Start; i < slice.End; i++)
            {
                DrawCountdownTableRow(countdowns[i], i - slice.Start);
            }
        }

        private void DrawStopwatchTable(List<TimerPlayerLoopDriver.TimerDebugInfo> stopwatches, EditorPagination.Slice slice)
        {
            // Draw table header
            DrawStopwatchTableHeader();

            // Draw table rows
            for (int i = slice.Start; i < slice.End; i++)
            {
                DrawStopwatchTableRow(stopwatches[i], i - slice.Start);
            }
        }

        private void DrawCountdownTableHeader()
        {
            Rect headerRect = GUILayoutUtility.GetRect(GetCountdownTableMinWidth(), HEADER_HEIGHT, GUILayout.ExpandWidth(true));

            // Header background
            EditorGUI.DrawRect(headerRect, HEADER_BG);

            // Header borders
            DrawTableBorders(headerRect);

            // Column widths (fixed-ish; allows horizontal scroll when needed)
            float idWidth = 60f;
            float slotWidth = 60f;
            float gameObjectWidth = COUNTDOWN_GAMEOBJECT_COL_WIDTH;
            float totalTimeWidth = 110f;
            float currentTimeWidth = 110f;
            float progressWidth = 240f;
            float statusWidth = 170f;

            float x = headerRect.x;
            var headerStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // Headers with vertical dividers
            DrawHeaderColumn(new Rect(x, headerRect.y, idWidth, headerRect.height), "Id", headerStyle);
            x += idWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, slotWidth, headerRect.height), "Slot", headerStyle);
            x += slotWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

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
            Rect headerRect = GUILayoutUtility.GetRect(GetStopwatchTableMinWidth(), HEADER_HEIGHT, GUILayout.ExpandWidth(true));

            // Header background
            EditorGUI.DrawRect(headerRect, HEADER_BG);

            // Header borders
            DrawTableBorders(headerRect);

            // Column widths (fixed-ish; allows horizontal scroll when needed)
            float idWidth = 60f;
            float slotWidth = 60f;
            float gameObjectWidth = STOPWATCH_GAMEOBJECT_COL_WIDTH;
            float elapsedTimeWidth = 120f;
            float statusWidth = 220f;

            float x = headerRect.x;
            var headerStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            // Headers with vertical dividers
            DrawHeaderColumn(new Rect(x, headerRect.y, idWidth, headerRect.height), "Id", headerStyle);
            x += idWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, slotWidth, headerRect.height), "Slot", headerStyle);
            x += slotWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, gameObjectWidth, headerRect.height), "GameObject", headerStyle);
            x += gameObjectWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, elapsedTimeWidth, headerRect.height), "Elapsed Time", headerStyle);
            x += elapsedTimeWidth;
            DrawVerticalDivider(x, headerRect.y, headerRect.height);

            DrawHeaderColumn(new Rect(x, headerRect.y, statusWidth, headerRect.height), "Status", headerStyle);
        }

        private void DrawCountdownTableRow(TimerPlayerLoopDriver.TimerDebugInfo countdown, int index)
        {
            try
            {
                Rect rowRect = GUILayoutUtility.GetRect(GetCountdownTableMinWidth(), ROW_HEIGHT, GUILayout.ExpandWidth(true));

                // Light zebra stripe background only (no colored row highlighting)
                if (index % 2 == 1)
                {
                    EditorGUI.DrawRect(rowRect, ZEBRA_STRIPE);
                }

                // Row borders
                DrawTableBorders(rowRect);

                // Column widths matching header
                float idWidth = 60f;
                float slotWidth = 60f;
                float gameObjectWidth = COUNTDOWN_GAMEOBJECT_COL_WIDTH;
                float totalTimeWidth = 110f;
                float currentTimeWidth = 110f;
                float progressWidth = 240f;
                float statusWidth = 170f;

                float x = rowRect.x;

                // Determine timer state for progress bar and status text
                bool ownerActive = countdown.IsOwnerActiveAndEnabled;
                float total = countdown.TotalTime;
                float remaining = countdown.RemainingTime;
                float progress = total <= 0f ? 1f : Mathf.Clamp01(1f - (remaining / total));
                float inverseProgress = 1f - progress;

                bool reachedEnd = remaining <= 0.0001f || progress >= 0.999f;
                bool isCompleted = !countdown.IsRunning && reachedEnd;
                bool isPaused = !isCompleted && countdown.IsRunning && !ownerActive;

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
                GetSlotAndId(countdown.Key, out int slot, out int id);

                DrawCenteredText(new Rect(x, rowRect.y, idWidth, rowRect.height), id.ToString(), dimmedTextColor, 10);
                x += idWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                DrawCenteredText(new Rect(x, rowRect.y, slotWidth, rowRect.height), slot.ToString(), dimmedTextColor, 10);
                x += slotWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                DrawGameObjectColumn(new Rect(x, rowRect.y, gameObjectWidth, rowRect.height), countdown.Owner, countdown.OwnerComponent, dimmedTextColor);
                x += gameObjectWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Total Time column
                DrawCenteredText(new Rect(x, rowRect.y, totalTimeWidth, rowRect.height),
                    FormatTime(total), dimmedTextColor, 10);
                x += totalTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Remaining Time column
                DrawCenteredText(new Rect(x, rowRect.y, currentTimeWidth, rowRect.height),
                    FormatTimeRemaining(remaining), dimmedTextColor, 10);
                x += currentTimeWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Progress column (ONLY the progress bar gets colored based on timer state)
                DrawProgressColumn(new Rect(x, rowRect.y, progressWidth, rowRect.height),
                    inverseProgress, progress, isCompleted, isPaused, countdown.IsRunning, countdown.Key);
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

        private void DrawStopwatchTableRow(TimerPlayerLoopDriver.TimerDebugInfo stopwatch, int index)
        {
            try
            {
                Rect rowRect = GUILayoutUtility.GetRect(GetStopwatchTableMinWidth(), ROW_HEIGHT, GUILayout.ExpandWidth(true));

                // Light zebra stripe background only (no colored row highlighting)
                if (index % 2 == 1)
                {
                    EditorGUI.DrawRect(rowRect, ZEBRA_STRIPE);
                }

                // Row borders
                DrawTableBorders(rowRect);

                // Column widths matching header
                float idWidth = 60f;
                float slotWidth = 60f;
                float gameObjectWidth = STOPWATCH_GAMEOBJECT_COL_WIDTH;
                float elapsedTimeWidth = 120f;
                float statusWidth = 220f;

                float x = rowRect.x;

                // Determine timer state for status text
                bool ownerActive = stopwatch.IsOwnerActiveAndEnabled;
                bool isCompleted = !stopwatch.IsRunning && stopwatch.IsOwnerValid && ownerActive;
                bool isPaused = !isCompleted && stopwatch.IsRunning && !ownerActive;

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
                GetSlotAndId(stopwatch.Key, out int slot, out int id);

                DrawCenteredText(new Rect(x, rowRect.y, idWidth, rowRect.height), id.ToString(), dimmedTextColor, 10);
                x += idWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                DrawCenteredText(new Rect(x, rowRect.y, slotWidth, rowRect.height), slot.ToString(), dimmedTextColor, 10);
                x += slotWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                DrawGameObjectColumn(new Rect(x, rowRect.y, gameObjectWidth, rowRect.height), stopwatch.Owner, stopwatch.OwnerComponent, dimmedTextColor);
                x += gameObjectWidth;
                DrawVerticalDivider(x, rowRect.y, rowRect.height);

                // Elapsed Time column
                DrawCenteredText(new Rect(x, rowRect.y, elapsedTimeWidth, rowRect.height),
                    FormatTime(stopwatch.ElapsedTime), dimmedTextColor, 10);
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

        private void DrawGameObjectColumn(Rect rect, GameObject owner, MonoBehaviour ownerComponent, Color textColor)
        {
            var centeredRect = new Rect(rect.x + 8, rect.y, rect.width - 16, rect.height);

            string objectName = owner != null ? owner.name : "NULL";
            string componentName = ownerComponent != null ? ownerComponent.GetType().Name : "None";

            // GameObject name (clickable)
            var objectStyle = new GUIStyle(EditorStyles.linkLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(0, 0, 0, 0)
            };
            objectStyle.normal.textColor = textColor;

            if (owner != null && GUI.Button(
                new Rect(centeredRect.x, centeredRect.y + 3, centeredRect.width, 14),
                objectName, objectStyle))
            {
                EditorGUIUtility.PingObject(owner);
                Selection.activeGameObject = owner;
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

        private void DrawProgressColumn(Rect rect, float inverseProgress, float normalProgress, bool isCompleted, bool isPaused, bool isRunning, ulong timerKey)
        {
            var progressRect = new Rect(rect.x + 6, rect.y + 4, rect.width - 12, 16);

            // Smooth progress animation
            float displayProgress = inverseProgress;
            if (Application.isPlaying)
            {
                if (!_smoothProgressValues.ContainsKey(timerKey)) _smoothProgressValues[timerKey] = inverseProgress;

                if (!isRunning)
                {
                    _smoothProgressValues[timerKey] = inverseProgress;
                    displayProgress = inverseProgress;
                }
                else
                {
                    float currentSmooth = _smoothProgressValues[timerKey];
                    float newSmooth = Mathf.Lerp(currentSmooth, inverseProgress, Time.unscaledDeltaTime * SMOOTH_SPEED);
                    _smoothProgressValues[timerKey] = newSmooth;
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

        private string GetTimerStatus(TimerPlayerLoopDriver.TimerDebugInfo timer, bool isCompleted, bool isPaused)
        {
            if (isCompleted)
                return "Done";

            if (isPaused)
            {
                return "Owner Off";
            }

            if (timer.IsRunning)
            {
                string status = "Running";
                if (timer.UseUnscaledTime) status += " (Unscaled)";
                if (timer.Kind == TimerPlayerLoopDriver.TimerKind.Countdown && timer.CurrentLoopIteration > 0)
                    status += $" Loop {timer.CurrentLoopIteration + 1}";
                return status;
            }

            // Non-running and not completed (e.g., manually stopped before start or state indeterminate)
            return "Stopped";
        }

        private void DrawPoolingBottomBar()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            try
            {
                Color defaultTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

                int active;
                int inPool;
                int capacity;
                int maxSize;
                string poolLabel;

                if (_selectedTab == 0)
                {
                    active = _countdowns.Count;
                    inPool = TimerPlayerLoopDriver.CountdownPoolCount;
                    capacity = active + inPool;
                    maxSize = TimerPlayerLoopDriver.MaxCountdownPoolSize;
                    poolLabel = "CD In Pool";
                }
                else
                {
                    active = _stopwatches.Count;
                    inPool = TimerPlayerLoopDriver.StopwatchPoolCount;
                    capacity = active + inPool;
                    maxSize = TimerPlayerLoopDriver.MaxStopwatchPoolSize;
                    poolLabel = "SW In Pool";
                }

                DrawStatCard("Active", active, defaultTextColor, RUNNING_BLUE);
                DrawStatCard(poolLabel, inPool, defaultTextColor, COMPLETED_GREEN);
                DrawStatCard("Capacity", capacity, defaultTextColor, ZEBRA_STRIPE);
                DrawStatCard("Max Size", maxSize, defaultTextColor, ZEBRA_STRIPE);
            }
            finally
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
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

        private string FormatTime(float timeInSeconds)
        {
            // Round to nearest 0.25s and omit trailing .0
            float rounded = RoundToQuarterSeconds(timeInSeconds);
            return rounded.ToString("0.##", CultureInfo.InvariantCulture) + "s";
        }

        private static string FormatTimeRemaining(float timeInSeconds)
        {
            // Remaining should be shown with 2 decimals and no 0.25s snapping.
            float clamped = Mathf.Max(0f, timeInSeconds);
            return clamped.ToString("0.00", CultureInfo.InvariantCulture) + "s";
        }

        private static float RoundToQuarterSeconds(float seconds)
        {
            const float step = 0.25f;
            return Mathf.Round(seconds / step) * step;
        }

        private static void GetSlotAndId(ulong key, out int slot, out int id)
        {
            slot = (int)(key >> 32);
            id = unchecked((int)(key & 0xFFFFFFFF));
        }

        private static float GetCountdownTableMinWidth()
        {
            // Id + Slot + GameObject + Total + Remaining + Progress + Status
            return 60f + 60f + COUNTDOWN_GAMEOBJECT_COL_WIDTH + 110f + 110f + 240f + 170f;
        }

        private static float GetStopwatchTableMinWidth()
        {
            // Id + Slot + GameObject + Elapsed + Status
            return 60f + 60f + STOPWATCH_GAMEOBJECT_COL_WIDTH + 120f + 220f;
        }

    }
}
#endif
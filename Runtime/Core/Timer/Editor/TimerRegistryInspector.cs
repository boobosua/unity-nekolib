#if UNITY_EDITOR
using NekoLib; // for EditorPagination
using NekoLib.Extensions;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Core
{
    [CustomEditor(typeof(TimerRegistry))]
    public class TimerRegistryInspector : Editor
    {
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdown", "Stopwatch" };
        private EditorPagination.State _countdownState;
        private EditorPagination.State _stopwatchState;

        public override void OnInspectorGUI()
        {
            // Don't draw the default inspector (this hides the script field)

            var timerRegistry = (TimerRegistry)target;

            // Show summary only during play mode
            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("Timer summary is available during Play Mode.", MessageType.Info);
                return;
            }

            if (timerRegistry.ActiveTimerCount == 0)
            {
                return;
            }

            // Get active timers and separate by type
            var activeTimers = timerRegistry.GetActiveTimers();
            var countdowns = new System.Collections.Generic.List<Countdown>();
            var stopwatches = new System.Collections.Generic.List<Stopwatch>();

            foreach (var timer in activeTimers)
            {
                if (!timer.IsOwnerValid) continue;

                if (timer is Countdown countdown)
                    countdowns.Add(countdown);
                else if (timer is Stopwatch stopwatch)
                    stopwatches.Add(stopwatch);
            }

            EditorGUILayout.Space(10);

            // Draw tabs
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames, new GUIStyle(GUI.skin.button) { fontSize = 11, fixedHeight = 20 });
            EditorGUILayout.Space(3);

            // Draw selected tab content
            if (_selectedTab == 0)
            {
                DrawCountdownTab(countdowns);
            }
            else
            {
                DrawStopwatchTab(stopwatches);
            }

            // Repaint during play mode for real-time updates
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        private void DrawCountdownTab(System.Collections.Generic.List<Countdown> countdowns)
        {
            if (countdowns.Count == 0)
            {
                EditorGUILayout.HelpBox("No active countdowns.", MessageType.Info);
                return;
            }

            // Pagination bar
            var slice = EditorPagination.Draw(ref _countdownState, countdowns.Count, 10, null, "Countdown", "Countdowns");

            // Column widths
            const float RightColWidth = 55f;
            const float Spacing = 6f;
            const float RowH = 18f;

            // Header (drawn with rects for consistent alignment) with horizontal margins
            var headerRect = EditorGUILayout.GetControlRect(false, RowH);
            headerRect = new Rect(headerRect.x + 8, headerRect.y, headerRect.width - 16, headerRect.height);
            DrawRowBackground(headerRect, -1); // no stripe
            var monoRect = new Rect(headerRect.x, headerRect.y, headerRect.width - (RightColWidth * 2 + Spacing * 2), headerRect.height);
            var elapsedRect = new Rect(monoRect.xMax + Spacing, headerRect.y, RightColWidth, headerRect.height);
            var totalRect = new Rect(elapsedRect.xMax + Spacing, headerRect.y, RightColWidth, headerRect.height);
            var hdr = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            GUI.Label(monoRect, "Mono", hdr);
            var hdrRight = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleRight };
            GUI.Label(elapsedRect, "Elapsed", hdrRight);
            GUI.Label(totalRect, "Total", hdrRight);
            var sep = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(sep, new Color(0.5f, 0.5f, 0.5f, 0.25f));

            // Rows
            for (int i = slice.Start, uiRow = 0; i < slice.End; i++, uiRow++)
            {
                var countdown = countdowns[i];
                var row = EditorGUILayout.GetControlRect(false, RowH);
                row = new Rect(row.x + 8, row.y, row.width - 16, row.height);
                DrawRowBackground(row, uiRow);

                monoRect = new Rect(row.x, row.y, row.width - (RightColWidth * 2 + Spacing * 2), row.height);
                elapsedRect = new Rect(monoRect.xMax + Spacing, row.y, RightColWidth, row.height);
                totalRect = new Rect(elapsedRect.xMax + Spacing, row.y, RightColWidth, row.height);

                var left = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
                var right = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

                string componentName = countdown.OwnerComponent != null ? countdown.OwnerComponent.GetType().Name : "NULL";
                GUI.Label(monoRect, componentName, left);
                GUI.Label(elapsedRect, FormatTime(countdown.RemainTime), right);
                GUI.Label(totalRect, FormatTime(countdown.TotalTime), right);
            }
        }

        private void DrawStopwatchTab(System.Collections.Generic.List<Stopwatch> stopwatches)
        {
            if (stopwatches.Count == 0)
            {
                EditorGUILayout.HelpBox("No active stopwatches.", MessageType.Info);
                return;
            }

            // Pagination bar
            var slice = EditorPagination.Draw(ref _stopwatchState, stopwatches.Count, 10, null, "Stopwatch", "Stopwatches");

            const float RightColWidth = 55f;
            const float Spacing = 6f;
            const float RowH = 18f;

            var headerRect = EditorGUILayout.GetControlRect(false, RowH);
            headerRect = new Rect(headerRect.x + 8, headerRect.y, headerRect.width - 16, headerRect.height);
            DrawRowBackground(headerRect, -1);
            var monoRect = new Rect(headerRect.x, headerRect.y, headerRect.width - (RightColWidth + Spacing), headerRect.height);
            var elapsedRect = new Rect(monoRect.xMax + Spacing, headerRect.y, RightColWidth, headerRect.height);
            var hdr = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            var hdrRight = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleRight };
            GUI.Label(monoRect, "Mono", hdr);
            GUI.Label(elapsedRect, "Elapsed", hdrRight);
            var sep = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(sep, new Color(0.5f, 0.5f, 0.5f, 0.25f));

            for (int i = slice.Start, uiRow = 0; i < slice.End; i++, uiRow++)
            {
                var sw = stopwatches[i];
                var row = EditorGUILayout.GetControlRect(false, RowH);
                row = new Rect(row.x + 8, row.y, row.width - 16, row.height);
                DrawRowBackground(row, uiRow);
                monoRect = new Rect(row.x, row.y, row.width - (RightColWidth + Spacing), row.height);
                elapsedRect = new Rect(monoRect.xMax + Spacing, row.y, RightColWidth, row.height);

                var left = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
                var right = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight };

                string componentName = sw.OwnerComponent != null ? sw.OwnerComponent.GetType().Name : "NULL";
                GUI.Label(monoRect, componentName, left);
                GUI.Label(elapsedRect, FormatTime(sw.RemainTime), right);
            }
        }

        private void DrawRowBackground(Rect r, int rowIndex)
        {
            if (rowIndex >= 0 && (rowIndex % 2 == 1))
            {
                EditorGUI.DrawRect(r, new Color(0f, 0f, 0f, 0.06f));
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            return timeInSeconds.ToReadableFormat();
        }
    }
}
#endif
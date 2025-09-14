#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib.Core
{
    [CustomEditor(typeof(TimerRegistry))]
    public class TimerRegistryInspector : Editor
    {
        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Countdown", "Stopwatch" };

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
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space(5);

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

            // Draw header with flexible spacing
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MonoBehaviour", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Elapsed", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Total", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            // Draw separator
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            // Draw countdown list
            foreach (var countdown in countdowns)
            {
                EditorGUILayout.BeginHorizontal();

                // MonoBehaviour name
                string componentName = countdown.OwnerComponent != null ? countdown.OwnerComponent.GetType().Name : "NULL";
                EditorGUILayout.LabelField(componentName);
                GUILayout.FlexibleSpace();

                // Elapsed time
                string elapsedTime = FormatTime(countdown.RemainTime);
                EditorGUILayout.LabelField(elapsedTime, GUILayout.Width(60));
                GUILayout.Space(10);

                // Total time
                string totalTime = FormatTime(countdown.TotalTime);
                EditorGUILayout.LabelField(totalTime, GUILayout.Width(60));

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawStopwatchTab(System.Collections.Generic.List<Stopwatch> stopwatches)
        {
            if (stopwatches.Count == 0)
            {
                EditorGUILayout.HelpBox("No active stopwatches.", MessageType.Info);
                return;
            }

            // Draw header with flexible spacing
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("MonoBehaviour", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Elapsed", EditorStyles.boldLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            // Draw separator
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            // Draw stopwatch list
            foreach (var stopwatch in stopwatches)
            {
                EditorGUILayout.BeginHorizontal();

                // MonoBehaviour name
                string componentName = stopwatch.OwnerComponent != null ? stopwatch.OwnerComponent.GetType().Name : "NULL";
                EditorGUILayout.LabelField(componentName);
                GUILayout.FlexibleSpace();

                // Elapsed time
                string elapsedTime = FormatTime(stopwatch.RemainTime);
                EditorGUILayout.LabelField(elapsedTime, GUILayout.Width(60));

                EditorGUILayout.EndHorizontal();
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

            if (minutes > 0)
                return $"{minutes}m{seconds:00}s";
            else
                return $"{seconds}s";
        }
    }
}
#endif
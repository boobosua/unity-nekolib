#if UNITY_EDITOR
using System.Collections.Generic;
using NekoLib.Logger;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Timer
{
    public partial class TimerTrackerWindow : EditorWindow
    {
        [MenuItem("Window/Neko Framework/Timer Tracker")]
        public static void ShowWindow() => GetWindow<TimerTrackerWindow>("Timer Tracker");

        // ─── UI state ────────────────────────────────────────────────────────────────
        private int _selectedTab;
        private readonly string[] _tabNames = { "Countdowns", "Stopwatches" };

        private PaginationEditor.State _countdownPageState;
        private PaginationEditor.State _stopwatchPageState;

        private Vector2 _scrollPosition;
        private bool _wasPlaying;

        // ─── Timer data (reused per-frame, no per-frame alloc) ───────────────────────
        private readonly List<TimerWorld.TimerDebugEntry> _allTimers = new(128);
        private readonly List<TimerWorld.TimerDebugEntry> _countdowns = new(64);
        private readonly List<TimerWorld.TimerDebugEntry> _stopwatches = new(64);

        // Smooth progress (countdown fill only); key = SmoothKey(slot, version)
        private readonly Dictionary<long, float> _smoothProgress = new();
        private readonly HashSet<long> _activeSmoothKeys = new(64);
        private readonly List<long> _smoothKeysToRemove = new(16);

        // ─── Lifecycle ───────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            titleContent = new GUIContent("Timer Tracker", "Track active timers across the project");
            minSize = new Vector2(900, 500);
            TimerWorld.EditorOnCapacityChanged += Repaint;
        }

        private void OnDisable()
        {
            TimerWorld.EditorOnCapacityChanged -= Repaint;
        }

        private void OnGUI()
        {
            try
            {
                HandlePlayModeTransition();

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("Timer Tracker is only available during Play Mode.", MessageType.Info);
                    return;
                }

                EditorGUILayout.Space(4);

                RefreshData();

                _selectedTab = EditorTabBar.Draw(_selectedTab, _tabNames, 24f);
                EditorGUILayout.Space(2);

                DrawTabContent();

                EditorGUILayout.Space(6);
                DrawStatsBar();

                Repaint();
            }
            catch (System.Exception e)
            {
                GUIUtility.ExitGUI();
                Log.Error($"[TimerTracker] OnGUI error: {e}");
            }
        }

        // ─── Data management ─────────────────────────────────────────────────────────
        private void RefreshData()
        {
            TimerWorld.GetDebugSnapshot(_allTimers);
            SplitTimers();
            CleanupSmoothProgress();
        }

        private void SplitTimers()
        {
            _countdowns.Clear();
            _stopwatches.Clear();
            for (int i = 0; i < _allTimers.Count; i++)
            {
                var t = _allTimers[i];
                if (t.Kind == TimerWorld.TimerKind.Countdown) _countdowns.Add(t);
                else if (t.Kind == TimerWorld.TimerKind.Stopwatch) _stopwatches.Add(t);
            }
        }

        private void CleanupSmoothProgress()
        {
            _activeSmoothKeys.Clear();
            for (int i = 0; i < _countdowns.Count; i++)
            {
                var e = _countdowns[i];
                _activeSmoothKeys.Add(SmoothKey(e.Slot, e.Version));
            }

            _smoothKeysToRemove.Clear();
            foreach (var key in _smoothProgress.Keys)
            {
                if (!_activeSmoothKeys.Contains(key))
                    _smoothKeysToRemove.Add(key);
            }

            for (int i = 0; i < _smoothKeysToRemove.Count; i++)
                _smoothProgress.Remove(_smoothKeysToRemove[i]);
        }

        // ─── Tab paging + scroll ─────────────────────────────────────────────────────
        private void DrawTabContent()
        {
            EditorGUILayout.BeginVertical();

            bool hasRows;
            PaginationEditor.Slice slice;

            if (_selectedTab == 0)
            {
                hasRows = _countdowns.Count > 0;
                slice = hasRows
                    ? PaginationEditor.Draw(ref _countdownPageState, _countdowns.Count, ItemsPerPage, HeaderHeight - 8f, "Countdown", "Countdowns")
                    : default;
                if (!hasRows)
                    EditorGUILayout.HelpBox("No countdowns are currently active.", MessageType.Info);
            }
            else
            {
                hasRows = _stopwatches.Count > 0;
                slice = hasRows
                    ? PaginationEditor.Draw(ref _stopwatchPageState, _stopwatches.Count, ItemsPerPage, HeaderHeight - 8f, "Stopwatch", "Stopwatches")
                    : default;
                if (!hasRows)
                    EditorGUILayout.HelpBox("No stopwatches are currently active.", MessageType.Info);
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            try
            {
                if (hasRows)
                {
                    if (_selectedTab == 0) DrawCountdownTable(slice);
                    else DrawStopwatchTable(slice);
                }
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        // ─── Play mode transition ────────────────────────────────────────────────────
        private void HandlePlayModeTransition()
        {
            if (_wasPlaying && !Application.isPlaying)
            {
                _smoothProgress.Clear();
                _selectedTab = 0;
                _countdownPageState = default;
                _stopwatchPageState = default;
                _scrollPosition = Vector2.zero;
            }
            _wasPlaying = Application.isPlaying;
        }
    }
}
#endif

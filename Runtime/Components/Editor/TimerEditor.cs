#if UNITY_EDITOR
using NekoLib.Extensions;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Timer))]
    public class TimerEditor : Editor
    {
        // Foldout states (static so they persist while the editor is open)
        private static bool s_ShowProgress = true;
        private static bool s_ShowEvents = true;

        private void EnsureStyles() { /* kept for future skin needs */ }

        private enum Glyph
        {
            Play,
            Pause,
            Stop
        }

        private static bool IconButton(Rect rect, Glyph glyph, string tooltip, bool disabled)
        {
            // Draw the clickable button first
            bool clicked = GUI.Button(rect, new GUIContent("", tooltip));

            // Determine glyph color respecting editor skin and disabled state
            Color col;
            if (glyph == Glyph.Play)
            {
                // Green-ish suitable for both skins
                col = EditorGUIUtility.isProSkin ? new Color(0.40f, 0.90f, 0.55f, 1f) : new Color(0.16f, 0.60f, 0.24f, 1f);
            }
            else if (glyph == Glyph.Stop)
            {
                // Red-ish suitable for both skins
                col = EditorGUIUtility.isProSkin ? new Color(0.96f, 0.42f, 0.42f, 1f) : new Color(0.85f, 0.25f, 0.25f, 1f);
            }
            else
            {
                col = EditorStyles.label.normal.textColor; // neutral for pause
            }
            if (disabled) col = new Color(col.r, col.g, col.b, 0.5f);

            // Draw glyph on top for high-DPI crispness
            var pad = 5f; // inner padding
            var inner = new Rect(rect.x + pad, rect.y + pad, rect.width - 2 * pad, rect.height - 2 * pad);

            Handles.BeginGUI();
            var prev = Handles.color;
            Handles.color = col;

            switch (glyph)
            {
                case Glyph.Play:
                    {
                        // Right-pointing triangle
                        Vector3 p1 = new Vector2(inner.xMin, inner.yMin);
                        Vector3 p2 = new Vector2(inner.xMin, inner.yMax);
                        Vector3 p3 = new Vector2(inner.xMax, inner.center.y);
                        Handles.DrawAAConvexPolygon(p1, p2, p3);
                        break;
                    }
                case Glyph.Pause:
                    {
                        float barW = inner.width * 0.28f;
                        float gap = inner.width * 0.16f;
                        var left = new Rect(inner.xMin, inner.yMin, barW, inner.height);
                        var right = new Rect(inner.xMax - barW, inner.yMin, barW, inner.height);
                        EditorGUI.DrawRect(left, col);
                        EditorGUI.DrawRect(right, col);
                        break;
                    }
                case Glyph.Stop:
                    {
                        var size = Mathf.Min(inner.width, inner.height);
                        var sq = new Rect(inner.center.x - size / 2f, inner.center.y - size / 2f, size, size);
                        EditorGUI.DrawRect(sq, col);
                        break;
                    }
            }

            Handles.color = prev;
            Handles.EndGUI();

            return clicked && !disabled;
        }

        public override void OnInspectorGUI()
        {
            EnsureStyles();
            var ticker = (Timer)target;
            serializedObject.Update();

            // === SETTINGS ===
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_waitTime"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoStart"));

            // Show warning immediately after autoStart if enabled
            SerializedProperty autoStartProp = serializedObject.FindProperty("_autoStart");
            if (autoStartProp != null && autoStartProp.boolValue)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("AutoStart fires in Awake(). OnBegin event may fire before other scripts register listeners in OnEnable/Start. Consider registering listeners in Awake() or disabling AutoStart.", MessageType.Warning);
                EditorGUILayout.Space(3);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_oneShot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_ignoreTimeScale"));

            // === TIMER PROGRESS (foldout section) ===
            s_ShowProgress = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowProgress, "Progress");
            if (s_ShowProgress)
            {
                float progress = 0f;
                string clockText = "00:00:00";
                if (Application.isPlaying)
                {
                    progress = Mathf.Clamp01(ticker.Progress);
                    clockText = ticker.ElapsedTime.ToClock();
                }

                // Single row: [progress bar + time label] [play/pause/resume] [stop]
                float rowHeight = Mathf.Max(22f, EditorGUIUtility.singleLineHeight + 6f);
                Rect rowRect = EditorGUILayout.GetControlRect(false, rowHeight);
                rowRect = EditorGUI.IndentedRect(rowRect);

                const float buttonSize = 20f; // a bit bigger, high-DPI friendly
                const float gap = 4f;

                Rect stopRect = new Rect(rowRect.xMax - buttonSize, rowRect.y + (rowRect.height - buttonSize) * 0.5f, buttonSize, buttonSize);
                Rect playRect = new Rect(stopRect.x - gap - buttonSize, stopRect.y, buttonSize, buttonSize);
                Rect progressRect = new Rect(rowRect.x, rowRect.y + (rowRect.height - EditorGUIUtility.singleLineHeight) * 0.5f, playRect.x - gap - rowRect.x, EditorGUIUtility.singleLineHeight);

                EditorGUI.ProgressBar(progressRect, progress, clockText);

                bool disabled = !Application.isPlaying;
                EditorGUI.BeginDisabledGroup(disabled);
                if (!Application.isPlaying || ticker.IsStopped)
                {
                    if (IconButton(playRect, Glyph.Play, "Start", disabled)) ticker.StartTimer();
                    if (IconButton(stopRect, Glyph.Stop, "Stop", disabled)) ticker.Stop();
                }
                else if (ticker.Paused)
                {
                    if (IconButton(playRect, Glyph.Play, "Resume", disabled)) ticker.Resume();
                    if (IconButton(stopRect, Glyph.Stop, "Stop", disabled)) ticker.Stop();
                }
                else
                {
                    if (IconButton(playRect, Glyph.Pause, "Pause", disabled)) ticker.Pause();
                    if (IconButton(stopRect, Glyph.Stop, "Stop", disabled)) ticker.Stop();
                }
                EditorGUI.EndDisabledGroup();

                if (Application.isPlaying)
                {
                    EditorUtility.SetDirty(ticker);
                    Repaint();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // === EVENTS (foldout section) ===
            s_ShowEvents = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowEvents, "Events");
            if (s_ShowEvents)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_onBegin"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_onUpdate"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_onTimeOut"));
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
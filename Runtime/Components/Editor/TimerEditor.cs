#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using NekoLib.Extensions;

namespace NekoLib.Components
{
    [CustomEditor(typeof(Timer))]
    public class TimerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var timer = (Timer)target;
            serializedObject.Update();

            EditorGUILayout.Space(10);

            // === TIMER SETTINGS ===
            DrawSectionHeader("⚙️ Settings", new Color(0.4f, 0.7f, 1f, 1f));
            EditorGUILayout.Space(8);

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

            EditorGUILayout.Space(15);
            DrawSeparator();
            EditorGUILayout.Space(15);

            // === TIMER PROGRESS ===
            DrawSectionHeader("📊 Progress", new Color(0.9f, 0.6f, 0.2f, 1f));
            EditorGUILayout.Space(8);

            // Progress bar with percentage
            float progress = 0f;
            string progressText = "0.0%";
            bool isCompleted = false;

            if (Application.isPlaying)
            {
                progress = timer.Progress;
                progressText = $"{progress * 100:F1}%";
                isCompleted = timer.IsStopped && progress >= 1.0f;
            }

            // Custom progress bar styling
            Rect progressRect = EditorGUILayout.GetControlRect(false, 24);
            progressRect = EditorGUI.IndentedRect(progressRect);

            // Draw progress bar background
            Color originalColor = GUI.color;
            GUI.color = EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0.8f, 0.8f, 0.8f, 1f);
            GUI.DrawTexture(progressRect, EditorGUIUtility.whiteTexture);

            // Draw progress fill
            if (progress > 0)
            {
                Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);

                if (isCompleted)
                {
                    GUI.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green for completed
                }
                else
                {
                    GUI.color = new Color(0.3f, 0.7f, 1f, 1f); // Blue for in progress
                }
                GUI.DrawTexture(fillRect, EditorGUIUtility.whiteTexture);
            }

            GUI.color = originalColor;

            // Draw progress text
            GUIStyle progressTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            progressTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUI.Label(progressRect, progressText, progressTextStyle);

            EditorGUILayout.Space(8);

            // Clock display with better styling
            string clockText;
            if (Application.isPlaying)
            {
                float elapsedTime = timer.ElapsedTime;
                string formattedTime = elapsedTime.ToClock();

                if (timer.IsStopped)
                {
                    clockText = $"⏱️ {formattedTime} (Stopped)";
                }
                else if (timer.Paused)
                {
                    clockText = $"⏱️ {formattedTime} (Paused)";
                }
                else
                {
                    clockText = $"⏱️ {formattedTime} (Running)";
                }
            }
            else
            {
                clockText = "⏱️ 00:00:00 (Editor)";
            }

            GUIStyle clockStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            clockStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f, 1f) : new Color(0.4f, 0.4f, 0.4f, 1f);

            EditorGUILayout.LabelField(clockText, clockStyle);

            // Add timer control buttons in play mode
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (timer.IsStopped)
                {
                    if (GUILayout.Button("▶ Start", GUILayout.Width(80)))
                    {
                        timer.StartTimer();
                    }
                }
                else if (timer.Paused)
                {
                    if (GUILayout.Button("▶ Resume", GUILayout.Width(80)))
                    {
                        timer.Resume();
                    }
                    if (GUILayout.Button("⏹ Stop", GUILayout.Width(80)))
                    {
                        timer.Stop();
                    }
                }
                else
                {
                    if (GUILayout.Button("⏸ Pause", GUILayout.Width(80)))
                    {
                        timer.Pause();
                    }
                    if (GUILayout.Button("⏹ Stop", GUILayout.Width(80)))
                    {
                        timer.Stop();
                    }
                }

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            // Force repaint to update progress bar during play mode
            if (Application.isPlaying)
            {
                EditorUtility.SetDirty(timer);
                Repaint();
            }

            EditorGUILayout.Space(15);
            DrawSeparator();
            EditorGUILayout.Space(15);

            // === EVENTS ===
            DrawSectionHeader("🎯 Events", new Color(0.8f, 0.4f, 0.8f, 1f));
            EditorGUILayout.Space(8);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onBegin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onTimeOut"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onUpdate"));

            EditorGUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSectionHeader(string title, Color accentColor)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            headerStyle.normal.textColor = accentColor;

            Rect headerRect = EditorGUILayout.GetControlRect(false, 20);
            GUI.Label(headerRect, title, headerStyle);
        }

        private void DrawSeparator()
        {
            Rect separatorRect = EditorGUILayout.GetControlRect(false, 1);
            separatorRect.height = 1;

            Color separatorColor = EditorGUIUtility.isProSkin
                ? new Color(0.4f, 0.4f, 0.4f, 1f)
                : new Color(0.6f, 0.6f, 0.6f, 1f);

            Color originalColor = GUI.color;
            GUI.color = separatorColor;
            GUI.DrawTexture(separatorRect, EditorGUIUtility.whiteTexture);
            GUI.color = originalColor;
        }
    }
}
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NekoLib.Components
{
    [CustomEditor(typeof(Timer))]
    public class TimerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var timer = (Timer)target;
            serializedObject.Update();

            // Timer Settings Section
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.Space(8);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        // Enhanced section header style
                        GUIStyle headerStyle = new(EditorStyles.boldLabel)
                        {
                            fontSize = 13
                        };
                        headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        EditorGUILayout.LabelField("Settings", headerStyle);

                        EditorGUILayout.Space(8);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_waitTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoStart"));

                        // Show warning immediately after autoStart if enabled
                        SerializedProperty autoStartProp = serializedObject.FindProperty("_autoStart");
                        if (autoStartProp != null && autoStartProp.boolValue)
                        {
                            EditorGUILayout.HelpBox("AutoStart fires in Awake(). OnBegin event may fire before other scripts register listeners in OnEnable/Start. Consider registering listeners in Awake() or disabling AutoStart.", MessageType.Warning);
                        }

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_oneShot"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_ignoreTimeScale"));
                    }
                    GUILayout.Space(10);
                }

                EditorGUILayout.Space(8);
            }

            // Timer Progress Section
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.Space(8);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        // Enhanced progress section header
                        GUIStyle progressHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                        progressHeaderStyle.fontSize = 13;
                        progressHeaderStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        EditorGUILayout.LabelField("Progress", progressHeaderStyle);

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

                        Rect progressRect = EditorGUILayout.GetControlRect(false, 20);

                        // Change progress bar color to green when completed
                        if (isCompleted)
                        {
                            Color originalColor = GUI.color;
                            GUI.color = new Color(0.3f, 0.8f, 0.3f, 1f); // Nice green color
                            EditorGUI.ProgressBar(progressRect, progress, progressText);
                            GUI.color = originalColor;
                        }
                        else
                        {
                            EditorGUI.ProgressBar(progressRect, progress, progressText);
                        }

                        EditorGUILayout.Space(5);

                        // Clock format below the bar
                        string clockText;
                        if (Application.isPlaying)
                        {
                            clockText = $"Elapsed: {timer.ClockFormat}";
                        }
                        else
                        {
                            clockText = "Elapsed: 00:00:00";
                        }

                        // Create a larger font style for the clock display
                        GUIStyle clockStyle = new(EditorStyles.centeredGreyMiniLabel)
                        {
                            fontSize = 12
                        };
                        clockStyle.normal.textColor = Color.gray;

                        EditorGUILayout.LabelField(clockText, clockStyle);

                        // Force repaint to update progress bar during play mode
                        if (Application.isPlaying && !timer.Paused)
                        {
                            EditorUtility.SetDirty(timer);
                            Repaint();
                        }
                    }
                    GUILayout.Space(10);
                }

                EditorGUILayout.Space(8);
            }

            // Events Section
            EditorGUILayout.Space(5);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.Space(8);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(10);
                    using (new EditorGUILayout.VerticalScope())
                    {
                        // Enhanced events section header
                        GUIStyle eventsHeaderStyle = new GUIStyle(EditorStyles.boldLabel);
                        eventsHeaderStyle.fontSize = 13;
                        eventsHeaderStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                        EditorGUILayout.LabelField("Events", eventsHeaderStyle);

                        EditorGUILayout.Space(8);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnBegin"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTimeOut"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUpdate"));
                    }
                    GUILayout.Space(10);
                }

                EditorGUILayout.Space(8);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
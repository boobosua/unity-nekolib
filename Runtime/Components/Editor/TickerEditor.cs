#if UNITY_EDITOR
using NekoLib.Extensions;
using UnityEditor;
using UnityEngine;

namespace NekoLib.Components
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Ticker))]
    public class TickerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var ticker = (Ticker)target;
            serializedObject.Update();

            // === TIMER SETTINGS ===
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

            // === TIMER PROGRESS ===
            EditorGUILayout.LabelField("Progress", EditorStyles.boldLabel);

            float progress = 0f;
            string clockText = "00:00:00";
            if (Application.isPlaying)
            {
                progress = Mathf.Clamp01(ticker.Progress);
                clockText = ticker.ElapsedTime.ToClock();
            }

            Rect progressRect = EditorGUILayout.GetControlRect(false, 18);
            progressRect = EditorGUI.IndentedRect(progressRect);
            EditorGUI.ProgressBar(progressRect, progress, clockText);

            // Controls row (always visible; grayed out in Edit mode)
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (!Application.isPlaying || ticker.IsStopped)
            {
                if (GUILayout.Button("Start", GUILayout.Width(80))) ticker.StartTimer();
                if (GUILayout.Button("Stop", GUILayout.Width(80))) ticker.Stop();
            }
            else if (ticker.Paused)
            {
                if (GUILayout.Button("Resume", GUILayout.Width(80))) ticker.Resume();
                if (GUILayout.Button("Stop", GUILayout.Width(80))) ticker.Stop();
            }
            else
            {
                if (GUILayout.Button("Pause", GUILayout.Width(80))) ticker.Pause();
                if (GUILayout.Button("Stop", GUILayout.Width(80))) ticker.Stop();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying)
            {
                EditorUtility.SetDirty(ticker);
                Repaint();
            }

            // === EVENTS ===
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onBegin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onUpdate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onTimeOut"));

            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
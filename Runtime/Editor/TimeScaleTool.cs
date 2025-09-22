#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.UIElements; // ToolbarButton
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace NekoLib
{
    // Original layout preserved. Only logic changed to sync with Project Settings Time Scale (TimeManager.asset m_TimeScale).
    [InitializeOnLoad]
    internal static class TimeScaleTool
    {
        private const string PrefEnabledKey = "NekoLib:TimeScaleToolEnabled"; // existing preference toggle
        private const string PrefStoredValue = "NekoLib:TimeScaleValue";      // kept for backward compatibility (not authoritative anymore)
        private const float DefaultTimeScale = 1f;
        private const float MinTimeScale = 0f;
        private const float MaxTimeScale = 5f;

        private static bool installed;
        private static VisualElement rootContainer;
        private static Slider timeSlider;
        private static Button resetButton;
        private static Label valueLabel;
        private static Label titleLabel;
        private static float lastAppliedTimeScale = 1f;

        // TimeManager sync
        private static SerializedObject timeManagerSO;
        private static SerializedProperty timeScaleProp;
        private static double lastPollTime; // throttle external polling slightly

        static TimeScaleTool()
        {
            EditorApplication.delayCall += EnsureInstall;
            EditorApplication.update += UpdateExternalSync;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static bool Enabled => EditorPrefs.GetBool(PrefEnabledKey, true);

        private static void EnsureInstall()
        {
            if (!Enabled || installed) return;
            var toolbarRoot = GetToolbarRoot();
            if (toolbarRoot == null)
            {
                EditorApplication.delayCall += EnsureInstall; // try again next tick
                return;
            }
            LoadTimeManager();
            // Authoritative value from TimeManager first
            if (timeScaleProp != null) lastAppliedTimeScale = Mathf.Clamp(timeScaleProp.floatValue, MinTimeScale, MaxTimeScale);
            else lastAppliedTimeScale = Mathf.Clamp(EditorPrefs.GetFloat(PrefStoredValue, DefaultTimeScale), MinTimeScale, MaxTimeScale);
            BuildUI(toolbarRoot);
            ApplyTimeScaleRuntimeOnly(); // ensure runtime matches if already in play (domain reload)
            installed = true;
        }

        private static void LoadTimeManager()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset");
            if (assets != null && assets.Length > 0)
            {
                timeManagerSO = new SerializedObject(assets[0]);
                timeScaleProp = timeManagerSO.FindProperty("m_TimeScale");
            }
        }

        private static void BuildUI(VisualElement toolbarRoot)
        {
            rootContainer = new VisualElement { name = "NekoLibTimeScaleContainer" };
            rootContainer.style.position = Position.Absolute;
            rootContainer.style.flexDirection = FlexDirection.Row;
            rootContainer.style.alignItems = Align.Center;
            rootContainer.style.paddingLeft = 4;
            rootContainer.style.paddingRight = 4;
            rootContainer.style.height = 20;
            rootContainer.style.backgroundColor = new Color(0f, 0f, 0f, 0.08f);
            ApplyRoundedStyling(rootContainer);

            timeSlider = new Slider(MinTimeScale, MaxTimeScale)
            {
                value = lastAppliedTimeScale,
                name = "NekoLibTimeScaleSlider",
                showInputField = false
            };
            timeSlider.style.minWidth = 130;
            timeSlider.style.maxWidth = 130;
            timeSlider.style.marginLeft = 1;
            timeSlider.style.marginRight = 2;
            timeSlider.lowValue = MinTimeScale;
            timeSlider.highValue = MaxTimeScale;
            timeSlider.RegisterValueChangedCallback(e => OnSliderChanged(e.newValue));
            // Remove persistent focus ring / blue outline on knob after interaction
#if UNITY_2020_1_OR_NEWER
            timeSlider.focusable = false;
            timeSlider.RegisterCallback<FocusInEvent>(_ => timeSlider.Blur());
            // Try also disabling focus on internal dragger element if present
            var dragger = timeSlider.Q<VisualElement>(className: "unity-dragger");
            if (dragger != null)
            {
                dragger.focusable = false;
                dragger.RegisterCallback<FocusInEvent>(_ => timeSlider.Blur());
            }
#endif

            valueLabel = new Label(FormatValue(lastAppliedTimeScale)) { name = "NekoLibTimeScaleValue" };
            valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            valueLabel.style.minWidth = 44;
            valueLabel.style.maxWidth = 44;
            valueLabel.style.fontSize = 12;
            valueLabel.style.marginRight = 1;

#if UNITY_2020_1_OR_NEWER
            resetButton = new ToolbarButton(ResetTimeScale)
            {
                text = "↺",
                tooltip = "Reset Time Scale (1.0)",
                name = "NekoLibTimeScaleReset"
            };
            resetButton.AddToClassList("unity-toolbar-button");
#else
            resetButton = new Button(ResetTimeScale)
            {
                text = "↺",
                tooltip = "Reset Time Scale (1.0)",
                name = "NekoLibTimeScaleReset"
            };
#endif
            resetButton.style.width = 26;
            resetButton.style.height = 22;
            resetButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            resetButton.style.paddingLeft = 0;
            resetButton.style.paddingRight = 0;
            resetButton.style.marginLeft = 0;
            resetButton.style.marginRight = 1;
            resetButton.style.fontSize = 11;
            resetButton.style.backgroundColor = StyleKeyword.Null;
            resetButton.focusable = false;
#if UNITY_2020_1_OR_NEWER
            resetButton.RegisterCallback<FocusInEvent>(_ => resetButton.Blur());
#endif
            resetButton.style.flexGrow = 0;
            resetButton.style.flexShrink = 0;

            titleLabel = new Label("Time Scale") { name = "NekoLibTimeScaleTitle" };
            titleLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            titleLabel.style.fontSize = 12;
            titleLabel.style.marginLeft = 1;
            titleLabel.style.marginRight = 3;
            titleLabel.style.minWidth = 72;
            titleLabel.style.maxWidth = 100;

            rootContainer.Add(titleLabel);
            rootContainer.Add(timeSlider);
            rootContainer.Add(valueLabel);
            rootContainer.Add(resetButton);

            toolbarRoot.Add(rootContainer);
            PositionContainer(toolbarRoot, rootContainer);
            rootContainer.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                PositionContainer(toolbarRoot, rootContainer);
                var h = rootContainer.resolvedStyle.height;
                if (h > 0)
                {
                    float buttonH = h;
                    float innerH = Mathf.Max(14, h - 6);
                    timeSlider.style.height = innerH;
                    valueLabel.style.height = innerH;
                    resetButton.style.minHeight = buttonH;
                    resetButton.style.height = buttonH;
                    resetButton.style.maxHeight = buttonH;
                    resetButton.style.minWidth = buttonH * 2 - 5;
                    resetButton.style.width = buttonH * 2 - 5;
                    resetButton.style.maxWidth = buttonH * 3;
#if UNITY_2022_1_OR_NEWER
                    int br = 5;
                    resetButton.style.borderTopLeftRadius = br;
                    resetButton.style.borderTopRightRadius = br;
                    resetButton.style.borderBottomLeftRadius = br;
                    resetButton.style.borderBottomRightRadius = br;
#endif
                }
            });
        }

        private static void OnSliderChanged(float v)
        {
            v = Mathf.Clamp(v, MinTimeScale, MaxTimeScale);
            if (Mathf.Approximately(v, lastAppliedTimeScale)) return;
            lastAppliedTimeScale = v;
            valueLabel.text = FormatValue(v);
            PushValueToTimeManager();
            ApplyTimeScaleRuntimeOnly();
        }

        private static void ResetTimeScale()
        {
            lastAppliedTimeScale = DefaultTimeScale;
            if (timeSlider != null) timeSlider.SetValueWithoutNotify(lastAppliedTimeScale);
            if (valueLabel != null) valueLabel.text = FormatValue(lastAppliedTimeScale);
            PushValueToTimeManager();
            ApplyTimeScaleRuntimeOnly();
        }

        // Apply only to runtime (not editor ProjectSettings value) – value already stored in asset by PushValueToTimeManager.
        private static void ApplyTimeScaleRuntimeOnly()
        {
            if (Application.isPlaying) Time.timeScale = lastAppliedTimeScale;
        }

        private static void PushValueToTimeManager()
        {
            if (timeManagerSO == null || timeScaleProp == null) LoadTimeManager();
            if (timeManagerSO != null && timeScaleProp != null)
            {
                timeManagerSO.Update();
                timeScaleProp.floatValue = lastAppliedTimeScale;
                timeManagerSO.ApplyModifiedPropertiesWithoutUndo();
            }
            // Legacy persistence (non-authoritative now)
            EditorPrefs.SetFloat(PrefStoredValue, lastAppliedTimeScale);
        }

        private static void UpdateExternalSync()
        {
            if (!installed) return;
            // Poll at ~10Hz to avoid unnecessary allocations
            if (EditorApplication.timeSinceStartup - lastPollTime < 0.1d) return;
            lastPollTime = EditorApplication.timeSinceStartup;
            if (timeManagerSO == null || timeScaleProp == null) LoadTimeManager();
            if (timeManagerSO == null || timeScaleProp == null) return;
            timeManagerSO.Update();
            float ext = Mathf.Clamp(timeScaleProp.floatValue, MinTimeScale, MaxTimeScale);
            if (!Mathf.Approximately(ext, lastAppliedTimeScale))
            {
                lastAppliedTimeScale = ext;
                if (timeSlider != null) timeSlider.SetValueWithoutNotify(ext);
                if (valueLabel != null) valueLabel.text = FormatValue(ext);
                ApplyTimeScaleRuntimeOnly();
            }
        }

        private static string FormatValue(float v)
        {
            if (v < 1f) return v.ToString("0.00");
            if (v < 10f) return v.ToString("0.00");
            return v.ToString("0.0");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Re-apply in case value changed while not playing
                ApplyTimeScaleRuntimeOnly();
            }
        }

        private static VisualElement GetToolbarRoot()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return null;
            var instances = Resources.FindObjectsOfTypeAll(toolbarType);
            if (instances == null || instances.Length == 0) return null;
            object toolbarInstance = instances[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance) ?? toolbarType.GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance);
            return rootField?.GetValue(toolbarInstance) as VisualElement;
        }

        private static VisualElement FindPlayControlsCluster(VisualElement root) => FindCandidateRecursive(root, 0);
        private static VisualElement FindCandidateRecursive(VisualElement ve, int depth)
        {
            if (depth > 6) return null;
            int buttons = 0;
            for (int i = 0; i < ve.childCount; i++) if (LooksLikeToolbarButton(ve[i])) buttons++;
            if (buttons >= 3) return ve.childCount > 0 ? ve[0] : ve; // left-most in cluster
            for (int i = 0; i < ve.childCount; i++)
            {
                var found = FindCandidateRecursive(ve[i], depth + 1);
                if (found != null) return found;
            }
            return null;
        }
        private static bool LooksLikeToolbarButton(VisualElement ve)
        {
            var r = ve.layout;
            return r.width >= 14 && r.width <= 55 && r.height >= 14 && r.height <= 40;
        }

        private static float GetWorldX(VisualElement target, VisualElement root)
        {
            float x = 0f; var c = target;
            while (c != null && c != root) { x += c.layout.x; c = c.parent; }
            return x;
        }
        private static float GetWorldY(VisualElement target, VisualElement root)
        {
            float y = 0f; var c = target;
            while (c != null && c != root) { y += c.layout.y; c = c.parent; }
            return y;
        }

        private static void PositionContainer(VisualElement toolbarRoot, VisualElement container)
        {
            float containerWidth = container.layout.width > 0 ? container.layout.width : 200f;
            var playCluster = FindPlayControlsCluster(toolbarRoot);
            float top = 0f; float height = 18f; float left;
            if (playCluster != null)
            {
                float playX = GetWorldX(playCluster, toolbarRoot);
                left = playX - 20f - containerWidth;
                if (left < 4) left = 4;
                height = playCluster.layout.height > 0 ? playCluster.layout.height : 22f;
                top = GetWorldY(playCluster, toolbarRoot) + (height - container.resolvedStyle.height) * 0.5f;
            }
            else
            {
                left = 120f;
            }
            container.style.left = left;
            container.style.top = top;
            container.style.height = height;
        }

        internal static void ApplyPreferenceChange(bool enabled)
        {
            if (enabled)
            {
                if (!installed) EnsureInstall();
            }
            else
            {
                if (installed && rootContainer != null && rootContainer.parent != null)
                {
                    rootContainer.parent.Remove(rootContainer);
                }
                installed = false;
                rootContainer = null;
                timeSlider = null;
                resetButton = null;
                valueLabel = null;
                titleLabel = null;
                timeManagerSO = null; timeScaleProp = null;
            }
        }

        private static void ApplyRoundedStyling(VisualElement ve)
        {
#if UNITY_2022_1_OR_NEWER
            int r = 6;
            ve.style.borderTopLeftRadius = r;
            ve.style.borderTopRightRadius = r;
            ve.style.borderBottomLeftRadius = r;
            ve.style.borderBottomRightRadius = r;
            ve.style.paddingLeft = 4;
            ve.style.paddingRight = 4;
#else
            ve.style.paddingLeft = 3;
            ve.style.paddingRight = 3;
#endif
        }
    }
}
#endif
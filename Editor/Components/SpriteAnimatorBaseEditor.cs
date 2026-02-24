#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

namespace NekoLib.Components
{
    [CustomEditor(typeof(SpriteAnimatorBase), true)]
    public class SpriteAnimatorEditorBase :
#if ODIN_INSPECTOR
        OdinEditor
#else
        Editor
#endif
    {
        private const string TabSessionKey = "NekoLib.SpriteAnimatorEditorBase.Tab";
        private SerializedProperty _sprites;
        private SerializedProperty _frameRate;
        private SerializedProperty _loopMode;
        private SerializedProperty _playOnAwake;
        private SerializedProperty _speedMultiplier;
        private SerializedProperty _useUnscaledTime;
        private SerializedProperty _startAtRandomFrame;
        private SerializedProperty _frameEvents;
        private SerializedProperty _onAnimationComplete;
        private SerializedProperty _onLoopComplete;

        private Sprite[] _previousSprites;
        private int _selectedFrameToAdd = 0;
        private List<bool> _eventFoldouts = new();
        private SpriteAnimatorBase.LoopMode _previousLoopMode;
        private int _selectedTab = 0; // 0: Settings, 1: Events
        private static GUIStyle _foldoutNoFocusStyle;

#if ODIN_INSPECTOR
        private PropertyTree _tree;
#endif


#if ODIN_INSPECTOR
        protected override void OnEnable()
        {
            base.OnEnable();
#else
    protected virtual void OnEnable()
    {
#endif
            _sprites = serializedObject.FindProperty("_sprites");
            _frameRate = serializedObject.FindProperty("_frameRate");
            _loopMode = serializedObject.FindProperty("_loopMode");
            _playOnAwake = serializedObject.FindProperty("_playOnAwake");
            _speedMultiplier = serializedObject.FindProperty("_speedMultiplier");
            _useUnscaledTime = serializedObject.FindProperty("_useUnscaledTime");
            _startAtRandomFrame = serializedObject.FindProperty("_startAtRandomFrame");
            _frameEvents = serializedObject.FindProperty("_frameEvents");
            _onAnimationComplete = serializedObject.FindProperty("_onAnimationComplete");
            _onLoopComplete = serializedObject.FindProperty("_onLoopComplete");

            _selectedTab = SessionState.GetInt(TabSessionKey, 0);
            CacheCurrentSprites();
            CacheCurrentLoopMode();
            SyncEventFoldoutsSize();

#if ODIN_INSPECTOR
            _tree?.Dispose();
            _tree = PropertyTree.Create(serializedObject);
            _tree.DrawMonoScriptObjectField = false;
#endif
        }

#if ODIN_INSPECTOR
        protected override void OnDisable()
        {
            _tree?.Dispose();
            _tree = null;
            base.OnDisable();
        }

        protected void DrawOdinUnityProperty(string unityPath)
        {
            if (_tree == null)
            {
                _tree = PropertyTree.Create(serializedObject);
                _tree.DrawMonoScriptObjectField = false;
            }

            var property = _tree.GetPropertyAtUnityPath(unityPath);
            property?.Draw();
        }

        protected void DrawOdinUnityPropertyPath(string unityPropertyPath)
        {
            if (_tree == null)
            {
                _tree = PropertyTree.Create(serializedObject);
                _tree.DrawMonoScriptObjectField = false;
            }

            var property = _tree.GetPropertyAtUnityPath(unityPropertyPath);
            property?.Draw();
        }
#endif

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

#if ODIN_INSPECTOR
            if (_tree == null)
            {
                _tree = PropertyTree.Create(serializedObject);
                _tree.DrawMonoScriptObjectField = false;
            }
            _tree.UpdateTree();
#endif

            // Improve hover responsiveness by repainting on mouse move
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }

            // Tabs: Settings | Events
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                int eventCount = _frameEvents != null ? _frameEvents.arraySize : 0;
                string frameEventsLabel = eventCount > 0 ? $"Frame Events ({eventCount})" : "Frame Events";
                GUIContent[] tabs =
                {
                    new("Settings"),
                    new(frameEventsLabel)
                };
                int newTab = GUILayout.Toolbar(_selectedTab, tabs);
                if (newTab != _selectedTab)
                {
                    _selectedTab = newTab;
                    SessionState.SetInt(TabSessionKey, _selectedTab);
                }
            }
            EditorGUILayout.Space();

            if (_selectedTab == 0)
            {
                DrawAnimationSettings();
                EditorGUILayout.Space();
                DrawAdditionalProperties();
                EditorGUILayout.Space();
                // Normal Events now live under Settings tab
                DrawEvents();
                EditorGUILayout.Space();
            }
            else
            {
                // Frame Events tab only shows per-frame events
                DrawFrameEvents();
                EditorGUILayout.Space();
            }

            // Preview removed by request

            if (serializedObject.hasModifiedProperties)
            {
#if ODIN_INSPECTOR
                _tree.ApplyChanges();
                _tree.InvokeDelayedActions();
#else
                serializedObject.ApplyModifiedProperties();
#endif
                CheckForSpriteChanges();
                CheckForLoopModeChanges();
            }
        }

        private void DrawAnimationSettings()
        {

#if ODIN_INSPECTOR
            DrawOdinUnityProperty("_sprites");
            DrawOdinUnityProperty("_frameRate");
#else
            EditorGUILayout.PropertyField(_sprites);
            EditorGUILayout.PropertyField(_frameRate);
#endif

            // Check for loop mode changes before drawing the property
            SpriteAnimatorBase.LoopMode currentLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;
            EditorGUI.BeginChangeCheck();
#if ODIN_INSPECTOR
            DrawOdinUnityProperty("_loopMode");
#else
            EditorGUILayout.PropertyField(_loopMode);
#endif
            if (EditorGUI.EndChangeCheck())
            {
                // Loop mode changed, check if we need to clear events
                SpriteAnimatorBase.LoopMode newLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;
                if ((currentLoopMode == SpriteAnimatorBase.LoopMode.Loop ||
                     currentLoopMode == SpriteAnimatorBase.LoopMode.PingPong) &&
                    newLoopMode == SpriteAnimatorBase.LoopMode.Once)
                {
                    ClearUnityEvent(_onLoopComplete);

                }
                _previousLoopMode = newLoopMode;
            }

#if ODIN_INSPECTOR
            DrawOdinUnityProperty("_playOnAwake");
            DrawOdinUnityProperty("_speedMultiplier");
            DrawOdinUnityProperty("_useUnscaledTime");
            DrawOdinUnityProperty("_startAtRandomFrame");
#else
            EditorGUILayout.PropertyField(_playOnAwake);
            EditorGUILayout.PropertyField(_speedMultiplier);
            EditorGUILayout.PropertyField(_useUnscaledTime);
            EditorGUILayout.PropertyField(_startAtRandomFrame);
#endif
        }

        protected virtual void DrawAdditionalProperties()
        {
            // Override in derived editors for component-specific properties
        }

        private void DrawFrameEvents()
        {
            // When no events exist, show an instructional info box; otherwise no title/header
            if (_frameEvents.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No frame events yet. Select a frame below and click 'Add Event' to create one.", MessageType.Info);
            }

            EditorGUI.indentLevel++;

            if (_sprites.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Add sprites to enable frame events", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            // Ensure foldout list size matches events
            SyncEventFoldoutsSize();

            // Create frame name options
            string[] frameOptions = new string[_sprites.arraySize];
            for (int i = 0; i < _sprites.arraySize; i++)
            {
                var sprite = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                frameOptions[i] = sprite != null ? $"Frame {i}: {sprite.name}" : $"Frame {i}: (null)";
            }

            // Get currently used frame indices
            var usedFrames = new HashSet<int>();
            for (int i = 0; i < _frameEvents.arraySize; i++)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                usedFrames.Add(frameIndex.intValue);
            }

            // Check for duplicate frames and display warnings
            bool hasDuplicates = false;
            var duplicateFrames = new HashSet<int>();
            var frameCount = new Dictionary<int, int>();

            for (int i = 0; i < _frameEvents.arraySize; i++)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                int frame = frameIndex.intValue;

                if (frameCount.ContainsKey(frame))
                {
                    frameCount[frame]++;
                    duplicateFrames.Add(frame);
                    hasDuplicates = true;
                }
                else
                {
                    frameCount[frame] = 1;
                }
            }

            if (hasDuplicates)
            {
                string duplicateList = string.Join(", ", duplicateFrames);
                EditorGUILayout.HelpBox($"Duplicate frame events detected for frame(s): {duplicateList}. Each frame should only have one event.", MessageType.Error);
            }

            // Display existing frame events (each with its own foldout)
            for (int i = 0; i < _frameEvents.arraySize; i++)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                SerializedProperty unityEvent = frameEvent.FindPropertyRelative("_onFrame");

                // Foldout header per event with fixed height and aligned remove button
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                float headerH = EditorGUIUtility.singleLineHeight + 4f;
                Rect headerRect = EditorGUILayout.GetControlRect(false, headerH);

                bool isDuplicate = duplicateFrames.Contains(frameIndex.intValue);
                string spriteName = "(null)";
                var si = frameIndex.intValue;
                if (si >= 0 && si < _sprites.arraySize)
                {
                    var sprop = _sprites.GetArrayElementAtIndex(si).objectReferenceValue as Sprite;
                    if (sprop != null) spriteName = sprop.name;
                }

                string foldLabel = $"Frame {frameIndex.intValue}: {spriteName}";

                // Compute rects
                Rect closeRect = new Rect(headerRect.xMax - 22f, headerRect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
                Rect foldRect = new Rect(headerRect.x + 4f, headerRect.y + 2f, headerRect.width - 30f, EditorGUIUtility.singleLineHeight);

                // Duplicate background tint
                var hover = headerRect.Contains(Event.current.mousePosition);
                if (isDuplicate)
                {
                    var tint = new Color(1f, 0.6f, 0.6f, 0.18f);
                    EditorGUI.DrawRect(headerRect, tint);
                }
                else if (hover)
                {
                    var baseTint = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.05f) : new Color(0f, 0f, 0f, 0.05f);
                    EditorGUI.DrawRect(headerRect, baseTint);
                }

                // Use custom foldout style that doesn't turn blue on focus (fallback to default if unavailable)
                EnsureStyles();
                if (_foldoutNoFocusStyle != null)
                {
                    _eventFoldouts[i] = EditorGUI.Foldout(foldRect, _eventFoldouts[i], foldLabel, true, _foldoutNoFocusStyle);
                }
                else
                {
                    _eventFoldouts[i] = EditorGUI.Foldout(foldRect, _eventFoldouts[i], foldLabel, true);
                }

                if (GUI.Button(closeRect, new GUIContent("×"), EditorStyles.miniButton))
                {
                    _frameEvents.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndVertical();
                    break; // Exit after modification
                }

                if (_eventFoldouts[i])
                {
                    int prevIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 0; // minimize left padding for cleaner look
                    // Unity Event
#if ODIN_INSPECTOR
                    DrawOdinUnityPropertyPath(unityEvent.propertyPath);
#else
                    EditorGUILayout.PropertyField(unityEvent, new GUIContent("OnFrame"));
#endif
                    EditorGUI.indentLevel = prevIndent;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(6);
            }

            // Add new frame event section (outside bordered boxes)
            // Use a style with no margins/padding so the popup can align flush left
            EditorGUILayout.BeginVertical(GUIStyle.none);

            // Create available frame options (excluding already used frames)
            var availableFrames = new List<int>();
            var availableFrameOptions = new List<string>();

            for (int i = 0; i < _sprites.arraySize; i++)
            {
                if (!usedFrames.Contains(i))
                {
                    availableFrames.Add(i);
                    var sprite = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                    availableFrameOptions.Add(sprite != null ? $"Frame {i}: {sprite.name}" : $"Frame {i}: (null)");
                }
            }

            if (availableFrames.Count > 0)
            {
                // Ensure selected frame index is valid
                if (_selectedFrameToAdd >= availableFrames.Count)
                {
                    _selectedFrameToAdd = 0;
                }

                // Remove left padding for the add row by drawing at indent level 0
                int prevIndentForAdd = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // A little breathing room above the add row
                EditorGUILayout.Space(4);

                // Full-width dropdown on the left, Add button on the right
                float btnH = EditorGUIUtility.singleLineHeight + 2f;
                Rect rowRect = EditorGUILayout.GetControlRect(false, btnH);
                const float buttonW = 110f;
                const float spacing = 6f;
                Rect buttonRect = new Rect(rowRect.xMax - buttonW, rowRect.y, buttonW, btnH);
                Rect dropdownRect = new Rect(rowRect.x, rowRect.y, rowRect.width - buttonW - spacing, btnH);

                _selectedFrameToAdd = EditorGUI.Popup(dropdownRect, _selectedFrameToAdd, availableFrameOptions.ToArray());

                if (GUI.Button(buttonRect, "Add Event"))
                {
                    _frameEvents.arraySize++;
                    SerializedProperty newEvent = _frameEvents.GetArrayElementAtIndex(_frameEvents.arraySize - 1);
                    newEvent.FindPropertyRelative("_frameIndex").intValue = availableFrames[_selectedFrameToAdd];

                    // Reset selection to first available frame after adding
                    _selectedFrameToAdd = 0;
                }

                // Restore previous indent
                EditorGUI.indentLevel = prevIndentForAdd;
            }
            else
            {
                // Draw the info box without left padding as well
                int prevIndentForInfo = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                EditorGUILayout.HelpBox("All frames already have events assigned.", MessageType.Info);
                EditorGUI.indentLevel = prevIndentForInfo;
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void EnsureStyles()
        {
            if (_foldoutNoFocusStyle != null) return;
            try
            {
                var baseStyle = EditorStyles.foldout;
                if (baseStyle == null) return; // EditorStyles not ready yet
                _foldoutNoFocusStyle = new GUIStyle(baseStyle);
                // Align text colors to regular label to avoid blue-ish focus tint
                Color c = EditorStyles.label.normal.textColor;
                _foldoutNoFocusStyle.normal.textColor = c;
                _foldoutNoFocusStyle.onNormal.textColor = c;
                _foldoutNoFocusStyle.active.textColor = c;
                _foldoutNoFocusStyle.onActive.textColor = c;
                _foldoutNoFocusStyle.focused.textColor = c;
                _foldoutNoFocusStyle.onFocused.textColor = c;
                _foldoutNoFocusStyle.hover.textColor = c;
                _foldoutNoFocusStyle.onHover.textColor = c;
            }
            catch
            {
                // In case EditorStyles is not initialized; will try again later during OnGUI
                _foldoutNoFocusStyle = null;
            }
        }

        private void SyncEventFoldoutsSize()
        {
            int size = _frameEvents != null ? _frameEvents.arraySize : 0;
            if (_eventFoldouts == null)
            {
                _eventFoldouts = new List<bool>(size);
            }
            while (_eventFoldouts.Count < size) _eventFoldouts.Add(true);
            while (_eventFoldouts.Count > size) _eventFoldouts.RemoveAt(_eventFoldouts.Count - 1);
        }

        private void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            SpriteAnimatorBase.LoopMode currentLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;

            // Show OnAnimationComplete only for Once; warn if misconfigured otherwise
            if (currentLoopMode == SpriteAnimatorBase.LoopMode.Once)
            {
#if ODIN_INSPECTOR
                DrawOdinUnityPropertyPath(_onAnimationComplete.propertyPath);
#else
                EditorGUILayout.PropertyField(_onAnimationComplete);
#endif
            }
            else
            {
                SerializedProperty acCalls = _onAnimationComplete.FindPropertyRelative("m_PersistentCalls.m_Calls");
                bool hasAC = acCalls != null && acCalls.arraySize > 0;
                if (hasAC)
                {
                    EditorGUILayout.HelpBox("OnAnimationComplete events detected but won't be called unless LoopMode is 'Once'. Consider removing them.", MessageType.Warning);
#if ODIN_INSPECTOR
                    DrawOdinUnityPropertyPath(_onAnimationComplete.propertyPath);
#else
                    EditorGUILayout.PropertyField(_onAnimationComplete);
#endif
                    if (GUILayout.Button("Clear OnAnimationComplete Events"))
                    {
                        ClearUnityEvent(_onAnimationComplete);

                    }
                }
            }

            // Show onLoopComplete with context
            if (currentLoopMode == SpriteAnimatorBase.LoopMode.Loop ||
                currentLoopMode == SpriteAnimatorBase.LoopMode.PingPong)
            {
#if ODIN_INSPECTOR
                DrawOdinUnityPropertyPath(_onLoopComplete.propertyPath);
#else
                EditorGUILayout.PropertyField(_onLoopComplete);
#endif
            }
            else if (currentLoopMode == SpriteAnimatorBase.LoopMode.Once)
            {
                // Check if OnLoopComplete has any events
                SerializedProperty persistentCalls = _onLoopComplete.FindPropertyRelative("m_PersistentCalls.m_Calls");
                bool hasEvents = persistentCalls != null && persistentCalls.arraySize > 0;

                if (hasEvents)
                {
                    EditorGUILayout.HelpBox("OnLoopComplete events detected but won't be called in 'Once' mode. Consider removing them.", MessageType.Warning);
#if ODIN_INSPECTOR
                    DrawOdinUnityPropertyPath(_onLoopComplete.propertyPath);
#else
                    EditorGUILayout.PropertyField(_onLoopComplete);
#endif

                    if (GUILayout.Button("Clear OnLoopComplete Events"))
                    {
                        ClearUnityEvent(_onLoopComplete);

                    }
                }
            }
        }



        private void CacheCurrentSprites()
        {
            if (_sprites.arraySize > 0)
            {
                _previousSprites = new Sprite[_sprites.arraySize];
                for (int i = 0; i < _sprites.arraySize; i++)
                {
                    _previousSprites[i] = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                }
            }
            else
            {
                _previousSprites = null;
            }
        }

        private void CheckForSpriteChanges()
        {
            bool spritesChanged = false;

            // Check if array size changed
            if (_previousSprites == null && _sprites.arraySize > 0)
            {
                spritesChanged = true;
            }
            else if (_previousSprites != null && _sprites.arraySize != _previousSprites.Length)
            {
                spritesChanged = true;
            }
            else if (_previousSprites != null)
            {
                // Check if any sprite references changed
                for (int i = 0; i < _sprites.arraySize; i++)
                {
                    var currentSprite = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                    if (currentSprite != _previousSprites[i])
                    {
                        spritesChanged = true;
                        break;
                    }
                }
            }

            if (spritesChanged)
            {
                // Clear invalid frame events
                ClearInvalidFrameEvents();
                CacheCurrentSprites();
            }
        }

        private void ClearInvalidFrameEvents()
        {
            if (_frameEvents.arraySize == 0) return;

            // Remove invalid frame events by iterating backwards
            for (int i = _frameEvents.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");

                if (frameIndex.intValue < 0 || frameIndex.intValue >= _sprites.arraySize)
                {
                    _frameEvents.DeleteArrayElementAtIndex(i);
                }
            }
        }

        private void CacheCurrentLoopMode()
        {
            _previousLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;
        }

        private void CheckForLoopModeChanges()
        {
            SpriteAnimatorBase.LoopMode currentLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;

            if (currentLoopMode != _previousLoopMode)
            {
                // Clear OnLoopComplete events when changing from Loop/PingPong to Once
                if ((_previousLoopMode == SpriteAnimatorBase.LoopMode.Loop ||
                     _previousLoopMode == SpriteAnimatorBase.LoopMode.PingPong) &&
                    currentLoopMode == SpriteAnimatorBase.LoopMode.Once)
                {
                    ClearUnityEvent(_onLoopComplete);

                }

                // Cache the new loop mode
                _previousLoopMode = currentLoopMode;
            }
        }

        private void ClearUnityEvent(SerializedProperty unityEventProperty)
        {
            // Clear all persistent calls from the UnityEvent
            SerializedProperty persistentCalls = unityEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (persistentCalls != null)
            {
                persistentCalls.arraySize = 0;
            }
        }
    }
}
#endif
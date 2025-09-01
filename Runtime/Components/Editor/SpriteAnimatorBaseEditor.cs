#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NekoLib.Components
{
    [CustomEditor(typeof(SpriteAnimatorBase), true)]
    public class SpriteAnimatorEditorBase : Editor
    {
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
        private SerializedProperty _currentFrame;
        private SerializedProperty _isPlaying;
        private SerializedProperty _isReversed;

        private Sprite[] _previousSprites;
        private int _selectedFrameToAdd = 0;
        private bool _frameEventsFoldout = true;
        private SpriteAnimatorBase.LoopMode _previousLoopMode;

        protected virtual void OnEnable()
        {
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
            _currentFrame = serializedObject.FindProperty("_currentFrame");
            _isPlaying = serializedObject.FindProperty("_isPlaying");
            _isReversed = serializedObject.FindProperty("_isReversed");

            CacheCurrentSprites();
            CacheCurrentLoopMode();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawAnimationSettings();
            EditorGUILayout.Space();
            DrawFrameEvents();
            EditorGUILayout.Space();
            DrawEvents();
            EditorGUILayout.Space();
            DrawAdditionalProperties();
            EditorGUILayout.Space();
            DrawRuntimeAndControls();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                CheckForSpriteChanges();
                CheckForLoopModeChanges();
            }
        }

        private void DrawAnimationSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sprites);
            EditorGUILayout.PropertyField(_frameRate);

            // Check for loop mode changes before drawing the property
            SpriteAnimatorBase.LoopMode currentLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_loopMode);
            if (EditorGUI.EndChangeCheck())
            {
                // Loop mode changed, check if we need to clear events
                SpriteAnimatorBase.LoopMode newLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;
                if ((currentLoopMode == SpriteAnimatorBase.LoopMode.Loop ||
                     currentLoopMode == SpriteAnimatorBase.LoopMode.PingPong) &&
                    newLoopMode == SpriteAnimatorBase.LoopMode.Once)
                {
                    ClearUnityEvent(_onLoopComplete);
                    // Debug.Log("Cleared OnLoopComplete events because LoopMode changed to Once");
                }
                _previousLoopMode = newLoopMode;
            }

            EditorGUILayout.PropertyField(_playOnAwake);
            EditorGUILayout.PropertyField(_speedMultiplier);
            EditorGUILayout.PropertyField(_useUnscaledTime);
            EditorGUILayout.PropertyField(_startAtRandomFrame);
        }

        protected virtual void DrawAdditionalProperties()
        {
            // Override in derived editors for component-specific properties
        }

        private void DrawFrameEvents()
        {
            // Create foldout header with event count
            string headerText = _frameEvents.arraySize > 0 ? $"Frame Events ({_frameEvents.arraySize})" : "Frame Events";
            _frameEventsFoldout = EditorGUILayout.Foldout(_frameEventsFoldout, headerText, true, EditorStyles.foldoutHeader);

            if (!_frameEventsFoldout)
                return;

            EditorGUI.indentLevel++;

            if (_sprites.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Add sprites to enable frame events", MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            // Create frame name options
            string[] frameOptions = new string[_sprites.arraySize];
            for (int i = 0; i < _sprites.arraySize; i++)
            {
                var sprite = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                frameOptions[i] = sprite != null ? $"Frame {i}: {sprite.name}" : $"Frame {i}: (null)";
            }

            // Get currently used frame indices
            var usedFrames = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < _frameEvents.arraySize; i++)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                usedFrames.Add(frameIndex.intValue);
            }

            // Check for duplicate frames and display warnings
            bool hasDuplicates = false;
            var duplicateFrames = new System.Collections.Generic.HashSet<int>();
            var frameCount = new System.Collections.Generic.Dictionary<int, int>();

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

            // Display existing frame events
            for (int i = 0; i < _frameEvents.arraySize; i++)
            {
                SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                SerializedProperty unityEvent = frameEvent.FindPropertyRelative("_onFrame");

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                // Frame selection dropdown with warning color for duplicates
                bool isDuplicate = duplicateFrames.Contains(frameIndex.intValue);
                if (isDuplicate)
                {
                    GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // Light red background
                }

                int selectedFrame = EditorGUILayout.Popup($"Event Frame {i}", frameIndex.intValue, frameOptions);
                frameIndex.intValue = selectedFrame;

                if (isDuplicate)
                {
                    GUI.backgroundColor = Color.white; // Reset background color
                }

                // Remove button for this specific event
                if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(18)))
                {
                    _frameEvents.DeleteArrayElementAtIndex(i);
                    break; // Exit the loop since we modified the array
                }

                EditorGUILayout.EndHorizontal();

                // Unity Event
                EditorGUILayout.PropertyField(unityEvent, new GUIContent("OnFrame"));

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            // Add new frame event section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add New Frame Event", EditorStyles.boldLabel);

            // Create available frame options (excluding already used frames)
            var availableFrames = new System.Collections.Generic.List<int>();
            var availableFrameOptions = new System.Collections.Generic.List<string>();

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

                EditorGUILayout.BeginHorizontal();
                _selectedFrameToAdd = EditorGUILayout.Popup("Frame", _selectedFrameToAdd, availableFrameOptions.ToArray());

                if (GUILayout.Button("Add Event", GUILayout.Width(80)))
                {
                    _frameEvents.arraySize++;
                    SerializedProperty newEvent = _frameEvents.GetArrayElementAtIndex(_frameEvents.arraySize - 1);
                    newEvent.FindPropertyRelative("_frameIndex").intValue = availableFrames[_selectedFrameToAdd];

                    // Reset selection to first available frame after adding
                    _selectedFrameToAdd = 0;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("All frames already have events assigned.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            SpriteAnimatorBase.LoopMode currentLoopMode = (SpriteAnimatorBase.LoopMode)_loopMode.enumValueIndex;

            // Always show onAnimationComplete
            EditorGUILayout.PropertyField(_onAnimationComplete);

            // Show onLoopComplete with context
            if (currentLoopMode == SpriteAnimatorBase.LoopMode.Loop ||
                currentLoopMode == SpriteAnimatorBase.LoopMode.PingPong)
            {
                EditorGUILayout.PropertyField(_onLoopComplete);
            }
            else if (currentLoopMode == SpriteAnimatorBase.LoopMode.Once)
            {
                // Check if OnLoopComplete has any events
                SerializedProperty persistentCalls = _onLoopComplete.FindPropertyRelative("m_PersistentCalls.m_Calls");
                bool hasEvents = persistentCalls != null && persistentCalls.arraySize > 0;

                if (hasEvents)
                {
                    EditorGUILayout.HelpBox("OnLoopComplete events detected but won't be called in 'Once' mode. Consider removing them.", MessageType.Warning);
                    EditorGUILayout.PropertyField(_onLoopComplete);

                    if (GUILayout.Button("Clear OnLoopComplete Events"))
                    {
                        ClearUnityEvent(_onLoopComplete);
                        Debug.Log("Manually cleared OnLoopComplete events");
                    }
                }
            }
        }

        private void DrawRuntimeAndControls()
        {
            EditorGUILayout.LabelField("Runtime & Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            // Frame info
            int totalFrames = _sprites.arraySize;
            string frameInfo = totalFrames > 0 ? $"Frame: {_currentFrame.intValue} / {totalFrames - 1}" : "Frame: 0 / 0";
            EditorGUILayout.LabelField(frameInfo);

            EditorGUILayout.Space(5);

            // Progress bar showing animation state
            Rect progressRect = EditorGUILayout.GetControlRect(false, 6);

            // Calculate progress (0-1)
            float progress = 0f;
            if (totalFrames > 1)
            {
                progress = (float)_currentFrame.intValue / (totalFrames - 1);
            }

            // Draw progress bar background
            EditorGUI.DrawRect(progressRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));

            // Draw progress bar fill if playing
            if (_isPlaying.boolValue)
            {
                Rect fillRect = new(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
                Color progressColor = _isReversed.boolValue ? new Color(0.3f, 0.7f, 1f, 0.8f) : new Color(0.3f, 0.8f, 0.3f, 0.8f);
                EditorGUI.DrawRect(fillRect, progressColor);
            }

            EditorGUILayout.Space(10);

            // Control buttons
            GUILayout.BeginHorizontal();

            SpriteAnimatorBase animator = (SpriteAnimatorBase)target;

            // Play button
            if (GUILayout.Button("Play", GUILayout.Height(30)))
            {
                animator.Play();
            }

            // Play Reverse button
            if (GUILayout.Button("Play Reverse", GUILayout.Height(30)))
            {
                animator.PlayReverse();
            }

            // Stop button
            if (GUILayout.Button("Stop", GUILayout.Height(30)))
            {
                animator.Stop();
            }

            // Restart button
            if (GUILayout.Button("Restart", GUILayout.Height(30)))
            {
                animator.Restart();
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
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
                    // Debug.Log("Cleared OnLoopComplete events because LoopMode changed to Once");
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
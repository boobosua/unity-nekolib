#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace NekoLib.Components
{
    [CustomEditor(typeof(BaseSpriteAnimator), true)]
    public class BaseSpriteAnimatorEditor : Editor
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
            }
        }

        private void DrawAnimationSettings()
        {
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_sprites);
            EditorGUILayout.PropertyField(_frameRate);
            EditorGUILayout.PropertyField(_loopMode);
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
            if (_sprites.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Add sprites to enable frame events", MessageType.Info);
                return;
            }

            // Create frame name options
            string[] frameOptions = new string[_sprites.arraySize];
            for (int i = 0; i < _sprites.arraySize; i++)
            {
                var sprite = _sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
                frameOptions[i] = sprite != null ? $"Frame {i}: {sprite.name}" : $"Frame {i}: (null)";
            }

            EditorGUILayout.PropertyField(_frameEvents);

            // Custom drawer for frame events
            if (_frameEvents.isExpanded)
            {
                EditorGUI.indentLevel++;

                for (int i = 0; i < _frameEvents.arraySize; i++)
                {
                    SerializedProperty frameEvent = _frameEvents.GetArrayElementAtIndex(i);
                    SerializedProperty frameIndex = frameEvent.FindPropertyRelative("_frameIndex");
                    SerializedProperty unityEvent = frameEvent.FindPropertyRelative("_event");

                    EditorGUILayout.BeginVertical("box");

                    // Frame selection dropdown
                    int selectedFrame = EditorGUILayout.Popup($"Event {i} Frame", frameIndex.intValue, frameOptions);
                    frameIndex.intValue = selectedFrame;

                    // Unity Event
                    EditorGUILayout.PropertyField(unityEvent, new GUIContent("Event"));

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawEvents()
        {
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

            BaseSpriteAnimator.LoopMode currentLoopMode = (BaseSpriteAnimator.LoopMode)_loopMode.enumValueIndex;

            // Always show onAnimationComplete
            EditorGUILayout.PropertyField(_onAnimationComplete);

            // Only show onLoopComplete for Loop and PingPong modes
            if (currentLoopMode == BaseSpriteAnimator.LoopMode.Loop ||
                currentLoopMode == BaseSpriteAnimator.LoopMode.PingPong)
            {
                EditorGUILayout.PropertyField(_onLoopComplete);
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

            BaseSpriteAnimator animator = (BaseSpriteAnimator)target;

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
    }
}
#endif
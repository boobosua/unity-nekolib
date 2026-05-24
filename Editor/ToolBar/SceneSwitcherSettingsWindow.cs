#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TRnK.Toolkit
{
    internal class SceneSwitcherSettingsWindow : EditorWindow
    {
        private const int PageSize = 20;

        private int _page;

        // New-group inline state
        private bool _addingGroup;
        private string _pendingGroupName = string.Empty;
        private string _pendingGroupError = string.Empty;

        // Rename inline state
        private int _renamingGroupIndex = -1;
        private string _pendingRenameLabel = string.Empty;
        private string _renameError = string.Empty;

        private SceneSwitcherSettings _settings;
        private EditorBuildSettingsScene[] _buildScenes = Array.Empty<EditorBuildSettingsScene>();

        [MenuItem("Tools/TRnK/Scene Switcher Groups")]
        internal static void Open()
        {
            var win = GetWindow<SceneSwitcherSettingsWindow>("Scene Switcher Settings");
            win.minSize = new Vector2(480, 420);
            win.Show();
        }

        private void OnEnable()
        {
            _settings = SceneSwitcherSettings.GetOrCreate();
            RefreshBuildScenes();
            EditorBuildSettings.sceneListChanged += OnSceneListChanged;
        }

        private void OnDisable()
        {
            EditorBuildSettings.sceneListChanged -= OnSceneListChanged;
        }

        private void OnSceneListChanged()
        {
            RefreshBuildScenes();
            Repaint();
        }

        private void RefreshBuildScenes()
        {
            _buildScenes = EditorBuildSettings.scenes;
            _page = Mathf.Clamp(_page, 0, TotalPages() - 1);
        }

        private int TotalPages() => Mathf.Max(1, Mathf.CeilToInt(_buildScenes.Length / (float)PageSize));

        private void OnGUI()
        {
            if (_settings == null) _settings = SceneSwitcherSettings.GetOrCreate();
            DrawSceneTable();
            EditorGUILayout.Space(10);
            DrawGroupManagement();
        }

        // ── Scene table ────────────────────────────────────────────────────────

        private void DrawSceneTable()
        {
            EditorGUILayout.LabelField("Scenes", EditorStyles.boldLabel);

            if (_buildScenes.Length == 0)
            {
                EditorGUILayout.HelpBox("No scenes in Build Settings.", MessageType.Info);
                return;
            }

            var groupOptions = BuildGroupOptions();

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Scene", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField("Group", EditorStyles.miniLabel, GUILayout.Width(130));
            }

            int start = _page * PageSize;
            int end = Mathf.Min(start + PageSize, _buildScenes.Length);

            for (int i = start; i < end; i++)
                DrawSceneRow(_buildScenes[i], groupOptions);

            DrawPagination();
        }

        private void DrawSceneRow(EditorBuildSettingsScene buildScene, string[] groupOptions)
        {
            string scenePath = buildScene.path ?? string.Empty;
            string sceneName = string.IsNullOrEmpty(scenePath)
                ? "(Unnamed)"
                : Path.GetFileNameWithoutExtension(scenePath);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(sceneName, GUILayout.ExpandWidth(true));

                int currentIdx = 0;
                for (int g = 0; g < _settings.groups.Count; g++)
                {
                    if (_settings.groups[g].scenePaths.Contains(scenePath))
                    {
                        currentIdx = g + 1;
                        break;
                    }
                }

                int selected = EditorGUILayout.Popup(currentIdx, groupOptions, GUILayout.Width(130));
                if (selected != currentIdx)
                    AssignSceneToGroup(scenePath, selected);
            }
        }

        private void AssignSceneToGroup(string scenePath, int groupOptionIndex)
        {
            foreach (var grp in _settings.groups)
                grp.scenePaths.Remove(scenePath);
            if (groupOptionIndex > 0)
                _settings.groups[groupOptionIndex - 1].scenePaths.Add(scenePath);
            Save();
        }

        private void DrawPagination()
        {
            int total = TotalPages();
            if (total <= 1) return;

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_page == 0))
                    if (GUILayout.Button("◀ Prev", GUILayout.Width(72))) _page--;
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Page {_page + 1} / {total}");
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(_page >= total - 1))
                    if (GUILayout.Button("Next ▶", GUILayout.Width(72))) _page++;
            }
        }

        // ── Group management ───────────────────────────────────────────────────

        private void DrawGroupManagement()
        {
            EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);

            if (_settings.groups.Count == 0 && !_addingGroup)
            {
                EditorGUILayout.LabelField("No groups defined. All scenes appear ungrouped.", EditorStyles.miniLabel);
            }

            int removeAt = -1;
            for (int g = 0; g < _settings.groups.Count; g++)
            {
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    if (_renamingGroupIndex == g)
                        DrawGroupRenameRow(g, _settings.groups[g]);
                    else
                        DrawGroupRow(g, _settings.groups[g], ref removeAt);
                }
            }

            if (removeAt >= 0)
                RemoveGroup(removeAt);

            if (_addingGroup)
            {
                DrawNewGroupRow();
            }
            else
            {
                EditorGUILayout.Space(2);
                if (GUILayout.Button("+ New Group"))
                {
                    _addingGroup = true;
                    _pendingGroupName = string.Empty;
                    _pendingGroupError = string.Empty;
                    _renamingGroupIndex = -1;
                }
            }
        }

        private void DrawGroupRow(int index, SceneGroup group, ref int removeAt)
        {
            int count = CountScenesInGroup(group);
            EditorGUILayout.LabelField(group.label, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField($"{count} scene{(count != 1 ? "s" : "")}", EditorStyles.miniLabel, GUILayout.Width(56));
            if (GUILayout.Button("Rename", GUILayout.Width(58)))
            {
                _renamingGroupIndex = index;
                _pendingRenameLabel = group.label;
                _renameError = string.Empty;
                _addingGroup = false;
            }
            if (GUILayout.Button("Remove", GUILayout.Width(58)))
                removeAt = index;
        }

        private void DrawGroupRenameRow(int index, SceneGroup group)
        {
            _pendingRenameLabel = EditorGUILayout.TextField(_pendingRenameLabel, GUILayout.ExpandWidth(true));
            if (!string.IsNullOrEmpty(_renameError))
                EditorGUILayout.LabelField(_renameError, EditorStyles.miniLabel, GUILayout.Width(110));
            if (GUILayout.Button("Confirm", GUILayout.Width(58)))
            {
                string trimmed = _pendingRenameLabel.Trim();
                if (string.IsNullOrEmpty(trimmed)) { _renameError = "Cannot be empty."; return; }
                if (IsDuplicateLabel(trimmed, index)) { _renameError = "Name already exists."; return; }
                group.label = trimmed;
                Save();
                _renamingGroupIndex = -1;
                _renameError = string.Empty;
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(52)))
            {
                _renamingGroupIndex = -1;
                _renameError = string.Empty;
            }
        }

        private void DrawNewGroupRow()
        {
            EditorGUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
                _pendingGroupName = EditorGUILayout.TextField(_pendingGroupName, GUILayout.ExpandWidth(true));
                if (!string.IsNullOrEmpty(_pendingGroupError))
                    EditorGUILayout.LabelField(_pendingGroupError, EditorStyles.miniLabel, GUILayout.Width(110));
                if (GUILayout.Button("Confirm", GUILayout.Width(58)))
                {
                    string trimmed = _pendingGroupName.Trim();
                    if (string.IsNullOrEmpty(trimmed)) { _pendingGroupError = "Cannot be empty."; return; }
                    if (IsDuplicateLabel(trimmed, -1)) { _pendingGroupError = "Name already exists."; return; }
                    _settings.groups.Add(new SceneGroup { label = trimmed });
                    Save();
                    _addingGroup = false;
                    _pendingGroupName = string.Empty;
                    _pendingGroupError = string.Empty;
                }
                if (GUILayout.Button("Cancel", GUILayout.Width(52)))
                {
                    _addingGroup = false;
                    _pendingGroupName = string.Empty;
                    _pendingGroupError = string.Empty;
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private void RemoveGroup(int index)
        {
            _settings.groups.RemoveAt(index);
            if (_renamingGroupIndex == index) _renamingGroupIndex = -1;
            else if (_renamingGroupIndex > index) _renamingGroupIndex--;
            Save();
        }

        private string[] BuildGroupOptions()
        {
            var options = new string[_settings.groups.Count + 1];
            options[0] = "None";
            for (int i = 0; i < _settings.groups.Count; i++)
                options[i + 1] = _settings.groups[i].label;
            return options;
        }

        private bool IsDuplicateLabel(string label, int excludeIndex)
        {
            for (int i = 0; i < _settings.groups.Count; i++)
            {
                if (i == excludeIndex) continue;
                if (string.Equals(_settings.groups[i].label, label, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private int CountScenesInGroup(SceneGroup group)
        {
            int count = 0;
            foreach (var path in group.scenePaths)
                if (!string.IsNullOrEmpty(path)) count++;
            return count;
        }

        private void Save()
        {
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
            SceneSwitcherToolbar.RefreshSceneList();
        }
    }
}
#endif

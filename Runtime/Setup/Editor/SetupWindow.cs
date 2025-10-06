#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NekoLib
{
    public class SetupWindow : EditorWindow
    {
        private const string WindowTitle = "Project Setup";
        private static readonly string[] Tabs = new[] { "Folders", "Packages", "Settings" };
        private int _tabIndex;

        // settings
        private static SetupFoldersSettings _settings;

        // Folders tab state (runtime selection, doesn't overwrite defaults unless saved in Settings)
        private List<SetupFoldersSettings.FolderOption> _sessionSelection;
        private Vector2 _scroll;
        private Vector2 _settingsScroll;

        [MenuItem("Tools/Neko Indie/Startup/Project Setup", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupWindow>(false, WindowTitle, true);
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        private void OnEnable()
        {
            _settings = SetupFoldersSettings.LoadOrCreate();
            EnsureSessionSelectionFromSettings();
            Repaint();
        }

        // Repaint when the project changes (folders created/deleted/moved)
        private void OnProjectChange()
        {
            Repaint();
        }

        // Also refresh when the window gains focus
        private void OnFocus()
        {
            Repaint();
        }

        private void EnsureSessionSelectionFromSettings()
        {
            _sessionSelection = _settings.Folders
                .Select(f => new SetupFoldersSettings.FolderOption { name = f.name, enabled = f.enabled })
                .ToList();
        }

        private void OnGUI()
        {
            DrawHeader();

            GUILayout.Space(2);
            _tabIndex = GUILayout.Toolbar(_tabIndex, Tabs);
            GUILayout.Space(4);

            switch (_tabIndex)
            {
                case 0: DrawFoldersTab(); break;
                case 1: DrawPackagesTab(); break;
                case 2: DrawSettingsTab(); break;
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Neko Indie • Project Setup", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("A quick starter to create your preferred project folder layout.", EditorStyles.miniLabel);
            }
        }

        private void DrawFoldersTab()
        {
            if (_settings == null) _settings = SetupFoldersSettings.LoadOrCreate();
            if (_sessionSelection == null) EnsureSessionSelectionFromSettings();
            else
            {
                // Auto-sync when folders added/removed or renamed in Settings
                var sessionNames = _sessionSelection.Select(s => s.name).ToList();
                var settingsNames = _settings.Folders.Select(s => s.name).ToList();
                if (sessionNames.Count != settingsNames.Count || !sessionNames.SequenceEqual(settingsNames))
                {
                    EnsureSessionSelectionFromSettings();
                }
            }

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Root", GUILayout.Width(60));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(_settings.RootPath);
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.Space(2);

                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
                for (int i = 0; i < _sessionSelection.Count; i++)
                {
                    var opt = _sessionSelection[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        opt.enabled = EditorGUILayout.Toggle(opt.enabled, GUILayout.Width(18));
                        string path = SetupFoldersTool.CombineUnityPath(_settings.RootPath, opt.name);
                        bool exists = AssetDatabase.IsValidFolder(path);
                        // Color name green if folder exists, otherwise default color
                        var oldColor = GUI.contentColor;
                        if (exists) GUI.contentColor = new Color(0.45f, 0.95f, 0.45f);
                        EditorGUILayout.LabelField(opt.name, GUILayout.Width(130));
                        GUI.contentColor = oldColor;
                        GUILayout.Space(6);
                        EditorGUI.BeginDisabledGroup(true);
                        if (exists) GUI.contentColor = new Color(0.45f, 0.95f, 0.45f);
                        EditorGUILayout.TextField(path, GUILayout.MinWidth(200), GUILayout.ExpandWidth(true));
                        GUI.contentColor = oldColor;
                        EditorGUI.EndDisabledGroup();
                    }
                }
                EditorGUILayout.EndScrollView();

                // Bottom primary action: full width with side padding, default styling
                GUILayout.Space(6);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(4);
                    if (GUILayout.Button("Create Folders", GUILayout.Height(32), GUILayout.ExpandWidth(true)))
                    {
                        var toCreate = _sessionSelection.Where(f => f.enabled).Select(f => f.name).ToList();
                        SetupFoldersTool.CreateFolders(_settings, toCreate);
                    }
                    GUILayout.Space(2);
                }
            }
        }

        private void DrawPackagesTab()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Setup Packages", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Packages setup will be added here. For now, this section is empty.", MessageType.Info);
            }
        }

        private void DrawSettingsTab()
        {
            if (_settings == null) _settings = SetupFoldersSettings.LoadOrCreate();

            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Root", GUILayout.Width(60));
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField(_settings.RootPath);
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button("Choose…", GUILayout.Width(90)))
                    {
                        string abs = EditorUtility.OpenFolderPanel("Select Root Folder (under Assets)", Application.dataPath, "");
                        if (!string.IsNullOrEmpty(abs))
                        {
                            string rel = SetupFoldersTool.AbsoluteToAssetsRelative(abs);
                            if (!string.IsNullOrEmpty(rel))
                            {
                                Undo.RecordObject(_settings, "Change Setup Root Path");
                                _settings.RootPath = rel;
                                EditorUtility.SetDirty(_settings);
                                AssetDatabase.SaveAssets();
                            }
                            else EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder inside the project's Assets directory.", "OK");
                        }
                    }
                }

                GUILayout.Space(4);
                _settingsScroll = EditorGUILayout.BeginScrollView(_settingsScroll, GUILayout.ExpandHeight(true));

                bool listChangedByButtons = false;
                EditorGUI.BeginChangeCheck();
                // Draw list items
                for (int i = 0; i < _settings.Folders.Count; i++)
                {
                    var f = _settings.Folders[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        f.name = EditorGUILayout.TextField(f.name);
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Width(120)))
                        {
                            if (GUILayout.Button("Up", GUILayout.Width(36)) && i > 0)
                            {
                                (_settings.Folders[i - 1], _settings.Folders[i]) = (_settings.Folders[i], _settings.Folders[i - 1]);
                                GUI.FocusControl(null);
                                listChangedByButtons = true;
                            }
                            if (GUILayout.Button("Down", GUILayout.Width(48)) && i < _settings.Folders.Count - 1)
                            {
                                (_settings.Folders[i + 1], _settings.Folders[i]) = (_settings.Folders[i], _settings.Folders[i + 1]);
                                GUI.FocusControl(null);
                                listChangedByButtons = true;
                            }
                            if (GUILayout.Button("X", GUILayout.Width(24)))
                            {
                                _settings.Folders.RemoveAt(i);
                                i--;
                                listChangedByButtons = true;
                                continue;
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Folder"))
                    {
                        _settings.Folders.Add(new SetupFoldersSettings.FolderOption { name = "NewFolder", enabled = true });
                        listChangedByButtons = true;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Reset Defaults", GUILayout.Width(120)))
                    {
                        _settings.SetDefaults();
                        EnsureSessionSelectionFromSettings();
                        listChangedByButtons = true;
                    }
                }
                EditorGUILayout.EndScrollView();

                if (EditorGUI.EndChangeCheck() || listChangedByButtons)
                {
                    Undo.RecordObject(_settings, "Edit Setup Folders Settings");
                    EditorUtility.SetDirty(_settings);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}

#endif
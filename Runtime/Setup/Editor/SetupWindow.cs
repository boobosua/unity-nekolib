#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
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
            _pkgSettings = SetupPackagesSettings.LoadOrCreate();
            // Seed from manifest immediately for first paint
            if (SetupPackagesTool.TryBuildInstalledGitMapFromManifest(out var manifestMap))
            {
                _gitInstalledMap = manifestMap;
                // ensure we repaint immediately with manifest data
                Repaint();
            }
            BeginRefreshUpmGitCache();
            // Refresh when UPM packages change externally
            Events.registeredPackages += OnPackagesChanged;
            Repaint();
        }

        // Repaint when the project changes (folders created/deleted/moved)
        private void OnProjectChange()
        {
            BeginRefreshUpmGitCache();
            Repaint();
        }

        // Also refresh when the window gains focus
        private void OnFocus()
        {
            BeginRefreshUpmGitCache();
            Repaint();
        }

        private void OnDisable()
        {
            Events.registeredPackages -= OnPackagesChanged;
        }

        private void OnPackagesChanged(PackageRegistrationEventArgs args)
        {
            BeginRefreshUpmGitCache();
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

        private SetupPackagesSettings _pkgSettings;
        private Vector2 _pkgScroll;
        private Dictionary<string, string> _gitInstalledMap; // normalized git url -> package name
        private ListRequest _gitListRequest;
        private bool _gitListPending;
        private Dictionary<string, AddRequest> _pendingAdds = new Dictionary<string, AddRequest>(); // normalized git url -> request
        private bool _pollingAdds;

        private void BeginRefreshUpmGitCache()
        {
            if (_gitListPending) return;
            _gitListPending = true;
            _gitListRequest = Client.List(true, true);
            EditorApplication.update += PollUpmGitCache;
        }

        private void PollUpmGitCache()
        {
            if (_gitListRequest == null) { EditorApplication.update -= PollUpmGitCache; _gitListPending = false; return; }
            if (!_gitListRequest.IsCompleted) return;
            EditorApplication.update -= PollUpmGitCache;
            _gitListPending = false;
            var map = new Dictionary<string, string>();
            if (_gitListRequest.Status == StatusCode.Success && _gitListRequest.Result != null)
            {
                foreach (var p in _gitListRequest.Result)
                {
                    if (p == null || p.source != PackageSource.Git) continue;
                    var url = SetupPackagesTool.ExtractGitUrlFromPackageId(p.packageId);
                    if (string.IsNullOrEmpty(url)) continue;
                    map[SetupPackagesTool.NormalizeGitUrl(url)] = p.name;
                }
            }
            // If UPM returns nothing (edge cases), keep manifest-based map
            if (map.Count == 0 && _gitInstalledMap != null && _gitInstalledMap.Count > 0)
            {
                // keep existing
            }
            else
            {
                _gitInstalledMap = map;
            }
            _gitListRequest = null;
            Repaint();
        }

        private void BeginAddPackage(string url)
        {
            var req = SetupPackagesTool.AddPackage(url);
            if (req == null) return;
            string key = SetupPackagesTool.NormalizeGitUrl(url);
            _pendingAdds[key] = req;
            if (!_pollingAdds)
            {
                _pollingAdds = true;
                EditorApplication.update += PollAddRequests;
            }
        }

        private void PollAddRequests()
        {
            if (_pendingAdds == null || _pendingAdds.Count == 0)
            {
                EditorApplication.update -= PollAddRequests;
                _pollingAdds = false;
                return;
            }
            var keys = new List<string>(_pendingAdds.Keys);
            foreach (var k in keys)
            {
                var r = _pendingAdds[k];
                if (r == null || !r.IsCompleted) continue;
                if (r.Status == StatusCode.Success)
                {
                    Debug.Log($"Install succeeded: {k}");
                }
                else
                {
                    Debug.LogError($"Install failed: {k} => {r.Error?.message}");
                }
                _pendingAdds.Remove(k);
                // ensure UPM state reflects latest
                BeginRefreshUpmGitCache();
                Repaint();
            }
        }

        private void DrawPackagesTab()
        {
            if (_pkgSettings == null) _pkgSettings = SetupPackagesSettings.LoadOrCreate();
            if (_gitInstalledMap == null && !_gitListPending) BeginRefreshUpmGitCache();

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Git Packages", EditorStyles.boldLabel);
                _pkgScroll = EditorGUILayout.BeginScrollView(_pkgScroll, GUILayout.ExpandHeight(true));
                if (_pkgSettings.Packages.Count == 0)
                {
                    EditorGUILayout.HelpBox("No Git packages configured.", MessageType.Info);
                }
                else
                {
                    foreach (var pkg in _pkgSettings.Packages)
                    {
                        if (pkg == null) continue;
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // Determine installed state from cached list FIRST
                            string normalized = SetupPackagesTool.NormalizeGitUrl(pkg.url);
                            string installedName = null;
                            if (_gitInstalledMap != null)
                                _gitInstalledMap.TryGetValue(normalized, out installedName);
                            bool installed = !string.IsNullOrEmpty(installedName);
                            bool isInstalling = _pendingAdds.ContainsKey(normalized);

                            // Tint URL like folders tab when installed
                            var oldContent = GUI.contentColor;
                            if (installed) GUI.contentColor = new Color(0.45f, 0.95f, 0.45f);
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.TextField(pkg.url);
                            EditorGUI.EndDisabledGroup();
                            GUI.contentColor = oldContent;

                            using (new EditorGUI.DisabledScope((_gitInstalledMap == null && _gitListPending)))
                            {
                                if (installed)
                                {
                                    // Show a green checkmark icon rather than a button
                                    var old = GUI.contentColor;
                                    GUI.contentColor = new Color(0.45f, 0.95f, 0.45f);
                                    var check = EditorGUIUtility.IconContent("TestPassed");
                                    if (check == null || check.image == null) check = new GUIContent("Installed");
                                    GUILayout.Label(check, GUILayout.Width(90));
                                    GUI.contentColor = old;
                                }
                                else
                                {
                                    // Show Install button, but prevent spamming while installing
                                    var oldEn = GUI.enabled;
                                    GUI.enabled = !isInstalling;
                                    string label = isInstalling ? "Installing..." : "Install";
                                    if (GUILayout.Button(label, GUILayout.Width(90)))
                                    {
                                        if (SetupPackagesTool.ValidatePackageIdentifier(pkg.url, out bool isGit, out string err) && isGit)
                                        {
                                            BeginAddPackage(pkg.url);
                                            Debug.Log($"Install requested: {pkg.url}");
                                            // UI disables via isInstalling
                                        }
                                        else
                                        {
                                            Debug.LogError(string.IsNullOrEmpty(err) ? "Configured URL is not a valid Git URL." : err);
                                        }
                                    }
                                    GUI.enabled = oldEn;
                                }
                            }
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
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
                // Draw folder list items
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
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
#if UNITY_2020_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace NekoLib
{
    /// <summary>
    /// Adds a scene switching dropdown / menu to the main editor toolbar.
    /// Uses ToolbarMenu when available (Unity 2020.1+ UI Toolkit) which stays visually consistent across
    /// Unity versions including Unity 6 (6000.x). Falls back to a DropdownField if needed.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSwitcherToolbar
    {
        #region Fields
#if UNITY_2020_1_OR_NEWER
        private static ToolbarMenu toolbarMenu;          // Preferred control (auto‑styled)
#endif
        private static DropdownField fallbackDropdown;   // Fallback or primary in older versions
        private static Image sceneIcon;
        private static string[] sceneNames = Array.Empty<string>();
        private static string[] scenePaths = Array.Empty<string>();
        private static Dictionary<string, int> duplicateNameCounts = new Dictionary<string, int>();
        private const string NoScenesLabel = "No Scenes (Build Settings)";
        private static bool initialized;
        private static VisualElement containerRef;

        // Startup scene override (integrated formerly separate tool)
        private const string PrefStartupPath = "NekoLib:StartupScenePath";
        private static string startupScenePath;            // stored path for marked startup scene
        private static bool playSwitched;                  // did we switch on play enter
        private static string originalSceneBeforePlay;     // path to restore after play
        private const string SessionOriginalSceneKey = "NekoLib:OriginalSceneBeforePlay";
        private const string SessionPlaySwitchedKey = "NekoLib:PlaySwitchedFlag";

        private const string PrefKey = "NekoLib:SceneSwitcherEnabled"; // mirror key used in preferences
        private const string PrefActivateLoadedAdditive = "NekoLib:ActivateLoadedAdditiveOnSelect"; // new preference
        #endregion

        private static bool IsEnabledPreference()
        {
            return EditorPrefs.GetBool(PrefKey, true);
        }

        static SceneSwitcherToolbar()
        {
            #region Initialization
            EditorApplication.delayCall += () =>
            {
                RefreshSceneList();
                if (IsEnabledPreference())
                    EditorApplication.update += TryInstall;
                LoadStartupPrefs();
                // Recover persisted session info (domain reload safety)
                if (string.IsNullOrEmpty(originalSceneBeforePlay))
                {
                    var stored = SessionState.GetString(SessionOriginalSceneKey, string.Empty);
                    if (!string.IsNullOrEmpty(stored)) originalSceneBeforePlay = stored;
                }
                if (!playSwitched)
                {
                    playSwitched = SessionState.GetBool(SessionPlaySwitchedKey, false);
                }
            };
            EditorBuildSettings.sceneListChanged += RefreshSceneList;
            EditorSceneManager.sceneOpened += (_, __) => UpdateSelectionVisual();
            SceneManager.activeSceneChanged += (_, __) => UpdateSelectionVisual();
#if UNITY_2021_1_OR_NEWER
            EditorSceneManager.activeSceneChangedInEditMode += (_, __) => UpdateSelectionVisual();
#endif
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endregion
        }

        private static void TryInstall()
        {
            if (!IsEnabledPreference()) return; // preference disabled
            if (initialized) return;
            var root = GetToolbarRoot();
            if (root == null) return;
            initialized = true;
            EditorApplication.update -= TryInstall;

            var container = new VisualElement { name = "NekoLibSceneSwitcherContainer" };
            containerRef = container;
            container.style.position = Position.Absolute;
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            PositionContainer(root, container);

#if UNITY_2020_1_OR_NEWER
            toolbarMenu = new ToolbarMenu
            {
                name = "NekoLibSceneSwitcher",
                tooltip = "Switch active scene (Build Settings)"
            };
            ApplyControlSizing(toolbarMenu);
            MatchControlHeightToContainer(container, toolbarMenu);
            ApplyRoundedStyling(toolbarMenu);
            EnsureSceneIcon(container);
            container.Add(toolbarMenu);
            root.Add(container);
            if (sceneNames.Length == 0)
            {
                toolbarMenu.text = TruncateDisplayName(NoScenesLabel);
                toolbarMenu.SetEnabled(true);
            }
            else
            {
                PopulateToolbarMenu();
            }
            container.RegisterCallback<GeometryChangedEvent>(_ => MatchControlHeightToContainer(container, toolbarMenu));
            toolbarMenu.RegisterCallback<GeometryChangedEvent>(_ => { if (string.IsNullOrEmpty(toolbarMenu.text)) InstallFallbackDropdown(container); });
#else
            InstallFallbackDropdown(container);
            root.Add(container);
#endif
        }

        private static void Uninstall()
        {
            if (!initialized) return;
            initialized = false;
#if UNITY_2020_1_OR_NEWER
            toolbarMenu = null;
#endif
            fallbackDropdown = null;
            sceneIcon = null;
            if (containerRef != null && containerRef.parent != null)
            {
                containerRef.parent.Remove(containerRef);
            }
            containerRef = null;
            // Re-register install attempt if preference re-enabled later
            if (IsEnabledPreference())
            {
                EditorApplication.update += TryInstall;
            }
        }

        internal static void ApplyPreferenceChange(bool enabled)
        {
            if (enabled)
            {
                if (!initialized)
                    EditorApplication.update += TryInstall;
            }
            else
            {
                Uninstall();
            }
        }

        private static void InstallFallbackDropdown(VisualElement container)
        {
            if (fallbackDropdown != null) return;
            #region FallbackControl
            fallbackDropdown = new DropdownField
            {
                name = "NekoLibSceneSwitcherFallback",
                tooltip = "Switch active scene (Build Settings)",
                choices = new List<string>(sceneNames)
            };
            ApplyControlSizing(fallbackDropdown);
            MatchControlHeightToContainer(container, fallbackDropdown);
            fallbackDropdown.style.marginLeft = 4;
            fallbackDropdown.RegisterValueChangedCallback(e => SwitchToScene(e.newValue));
            ApplyRoundedStyling(fallbackDropdown);
            EnsureSceneIcon(container, before: fallbackDropdown);
            container.Add(fallbackDropdown);

            if (sceneNames.Length == 0)
            {
                fallbackDropdown.choices = new List<string> { NoScenesLabel };
                fallbackDropdown.value = NoScenesLabel;
                fallbackDropdown.SetEnabled(false);
            }
            else
            {
                string current = SceneManager.GetActiveScene().name;
                if (Array.IndexOf(sceneNames, current) < 0) current = sceneNames[0];
                fallbackDropdown.value = current;
            }
            container.RegisterCallback<GeometryChangedEvent>(_ => MatchControlHeightToContainer(container, fallbackDropdown));
            #endregion
        }

#if UNITY_2020_1_OR_NEWER
        private static void PopulateToolbarMenu()
        {
            if (toolbarMenu == null) return;
            toolbarMenu.menu.MenuItems().Clear();
            string current = SceneManager.GetActiveScene().name;
            var activeScene = SceneManager.GetActiveScene();
            bool hasScenes = sceneNames.Length > 0;
            if (!hasScenes)
            {
                toolbarMenu.text = TruncateDisplayName(NoScenesLabel);
                toolbarMenu.SetEnabled(true);
                toolbarMenu.menu.AppendAction("Open Build Settings...", _ => OpenBuildSettings(), _ => DropdownMenuAction.Status.Normal);
                toolbarMenu.menu.AppendAction("Add Active Scene To Build Settings", _ => AddActiveSceneToBuild(), _ => CanAddActiveScene() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                toolbarMenu.menu.AppendSeparator("");
                AppendStartupMarkItem();
                toolbarMenu.menu.AppendSeparator("");
                toolbarMenu.menu.AppendAction("Refresh", _ => RefreshSceneList(), _ => DropdownMenuAction.Status.Normal);
                return;
            }
            bool currentInList = Array.IndexOf(sceneNames, current) >= 0;
            if (!currentInList && sceneNames.Length > 0)
            {
                string baseDisplay = string.IsNullOrEmpty(current) ? sceneNames[0] : current + " *";
                if (HasStartupScene() && activeScene.IsValid() && ScenePathMatchesStartup(activeScene.path))
                    baseDisplay += " ★"; // append startup star suffix
                toolbarMenu.text = TruncateDisplayName(baseDisplay);
                toolbarMenu.menu.AppendAction("Add Active Scene To Build Settings", _ => AddActiveSceneToBuild(), _ => CanAddActiveScene() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                toolbarMenu.menu.AppendSeparator("");
            }
            else
            {
                string baseDisplay = currentInList ? current : sceneNames[0];
                if (HasStartupScene() && activeScene.IsValid() && ScenePathMatchesStartup(activeScene.path))
                    baseDisplay += " ★";
                toolbarMenu.text = TruncateDisplayName(baseDisplay);
            }
            string startupDisplay = StartupSceneName();
            for (int i = 0; i < sceneNames.Length; i++)
            {
                string display = sceneNames[i];
                if (ScenePathMatchesStartup(scenePaths[i])) display = display + " ★"; // star as suffix to avoid clashing with left checkmark
                string captured = sceneNames[i];
                toolbarMenu.menu.AppendAction(display, a => SwitchToScene(captured), a => captured == current ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
            toolbarMenu.menu.AppendSeparator("");
            toolbarMenu.menu.AppendAction("Open Build Settings...", _ => OpenBuildSettings(), _ => DropdownMenuAction.Status.Normal);
            AppendStartupMarkItem();
            toolbarMenu.menu.AppendAction("Refresh", _ => RefreshSceneList(), _ => DropdownMenuAction.Status.Normal);
        }
#endif

        private static bool CanAddActiveScene()
        {
            var active = SceneManager.GetActiveScene();
            if (!active.IsValid() || string.IsNullOrEmpty(active.path)) return false;
            if (Array.IndexOf(scenePaths, active.path) >= 0) return false;
            return true;
        }

        private static void AddActiveSceneToBuild()
        {
            if (!CanAddActiveScene()) return;
            var active = SceneManager.GetActiveScene();
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes) { new EditorBuildSettingsScene(active.path, true) };
            EditorBuildSettings.scenes = list.ToArray();
            RefreshSceneList();
        }

        private static void OpenBuildSettings() => EditorWindow.GetWindow(typeof(BuildPlayerWindow));

        private static void RefreshSceneList()
        {
            #region RefreshList
            var buildScenes = EditorBuildSettings.scenes;
            int count = buildScenes.Length;
            sceneNames = new string[count];
            scenePaths = new string[count];
            duplicateNameCounts.Clear();
            for (int i = 0; i < count; i++)
            {
                string path = buildScenes[i].path;
                scenePaths[i] = path;
                string baseName = string.IsNullOrEmpty(path) ? "Unnamed" : System.IO.Path.GetFileNameWithoutExtension(path);
                if (duplicateNameCounts.ContainsKey(baseName)) duplicateNameCounts[baseName]++; else duplicateNameCounts[baseName] = 1;
                sceneNames[i] = baseName;
            }
            // Disambiguate duplicates by appending (index)
            for (int i = 0; i < count; i++)
            {
                string bn = sceneNames[i];
                if (duplicateNameCounts.TryGetValue(bn, out var c) && c > 1)
                {
                    sceneNames[i] = bn + " (" + (i + 1) + ")";
                }
            }
#if UNITY_2020_1_OR_NEWER
            PopulateToolbarMenu();
#endif
            if (fallbackDropdown != null)
            {
                fallbackDropdown.choices = new List<string>(sceneNames.Length > 0 ? sceneNames : new[] { NoScenesLabel });
                if (sceneNames.Length == 0)
                {
                    fallbackDropdown.value = NoScenesLabel;
                    fallbackDropdown.SetEnabled(false);
                }
                else
                {
                    fallbackDropdown.SetEnabled(true);
                    string current = SceneManager.GetActiveScene().name;
                    if (Array.IndexOf(sceneNames, current) < 0) current = sceneNames[0];
                    fallbackDropdown.value = current;
                }
            }
            #endregion
        }

        private static void UpdateSelectionVisual()
        {
#if UNITY_2020_1_OR_NEWER
            PopulateToolbarMenu();
#endif
            if (fallbackDropdown != null)
            {
                string active = SceneManager.GetActiveScene().name;
                if (Array.IndexOf(sceneNames, active) >= 0)
                    fallbackDropdown.value = active;
            }
        }

        private static void SwitchToScene(string sceneName)
        {
            if (sceneName == NoScenesLabel) return;
            int index = Array.IndexOf(sceneNames, sceneName);
            if (index < 0 || index >= scenePaths.Length) return;
            string path = scenePaths[index];
            if (string.IsNullOrEmpty(path)) return;
            // Optional behavior: if target scene already loaded additively, just set active instead of reopening.
            bool activateLoadedAdditive = EditorPrefs.GetBool(PrefActivateLoadedAdditive, false);
            if (activateLoadedAdditive)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (string.Equals(s.path, path, StringComparison.OrdinalIgnoreCase))
                    {
                        if (s != SceneManager.GetActiveScene())
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                SceneManager.SetActiveScene(s);
                                UpdateSelectionVisual();
                            }
                        }
                        else
                        {
                            UpdateSelectionVisual();
                        }
                        return; // done
                    }
                }
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
            else
            {
                UpdateSelectionVisual();
            }
        }

        private static VisualElement GetToolbarRoot()
        {
            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            if (toolbarType == null) return null;
            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars == null || toolbars.Length == 0) return null;
            object toolbarInstance = toolbars[0];
            var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance) ?? toolbarType.GetField("m_RootVisualElement", BindingFlags.NonPublic | BindingFlags.Instance);
            if (rootField == null) return null;
            return rootField.GetValue(toolbarInstance) as VisualElement;
        }

        private static VisualElement FindPlayControlsRightMost(VisualElement root) => FindCandidateRecursive(root, 0);

        private static VisualElement FindCandidateRecursive(VisualElement ve, int depth)
        {
            if (depth > 6) return null;
            int buttons = 0;
            for (int i = 0; i < ve.childCount; i++) if (LooksLikeToolbarButton(ve[i])) buttons++;
            if (buttons >= 3) return ve.childCount > 0 ? ve[ve.childCount - 1] : ve;
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

        private static float GetWorldXWithinToolbar(VisualElement target, VisualElement toolbarRoot)
        {
            float x = 0f;
            var current = target;
            while (current != null && current != toolbarRoot) { x += current.layout.x; current = current.parent; }
            return x;
        }

        private static float GetWorldYWithinToolbar(VisualElement target, VisualElement toolbarRoot)
        {
            float y = 0f;
            var current = target;
            while (current != null && current != toolbarRoot) { y += current.layout.y; current = current.parent; }
            return y;
        }

        private static void PositionContainer(VisualElement toolbarRoot, VisualElement container)
        {
            var anchor = FindPlayControlsRightMost(toolbarRoot);
            float left = 220f, top = 0f, height = 0f;
            if (anchor != null)
            {
                left = GetWorldXWithinToolbar(anchor, toolbarRoot) + anchor.layout.width + 18f;
                top = GetWorldYWithinToolbar(anchor, toolbarRoot);
                height = anchor.layout.height;
            }
            container.style.left = left;
            container.style.top = top;
            if (height > 0) container.style.height = height;
        }

        private static void MatchControlHeightToContainer(VisualElement container, VisualElement control)
        {
            var h = container.resolvedStyle.height;
            if (h > 0) { control.style.height = h; control.style.unityTextAlign = TextAnchor.MiddleLeft; }
        }

        private static void ApplyRoundedStyling(VisualElement ve)
        {
#if UNITY_2022_1_OR_NEWER
            var radius = 6;
            ve.style.borderTopLeftRadius = radius;
            ve.style.borderTopRightRadius = radius;
            ve.style.borderBottomLeftRadius = radius;
            ve.style.borderBottomRightRadius = radius;
            ve.style.paddingLeft = 4;
            ve.style.paddingRight = 4;
#else
            ve.style.paddingLeft = 3;
            ve.style.paddingRight = 3;
#endif
        }

        private static void ApplyControlSizing(VisualElement ve)
        {
            ve.style.minWidth = 110;
            ve.style.maxWidth = 160;
        }

        // Truncate while preserving a trailing star indicator (space + star) if present
        private static string TruncateDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            const int max = 22; // total allowed characters including ellipsis & star
            bool hasStar = name.EndsWith(" ★", StringComparison.Ordinal);
            if (name.Length <= max) return name;
            if (hasStar)
            {
                // Reserve 2 chars for space+star and 1 for ellipsis
                int coreLen = max - 3; // remaining for visible core before ellipsis
                if (coreLen < 1) return "★"; // degenerate fallback
                string core = name.Substring(0, coreLen) + "…";
                return core + " ★";
            }
            // Normal truncation (no star)
            return name.Substring(0, max - 1) + "…";
        }

        private static void EnsureSceneIcon(VisualElement container, VisualElement before = null)
        {
            if (sceneIcon != null) return;
            var tex = EditorGUIUtility.IconContent("SceneAsset Icon").image as Texture2D;
            sceneIcon = new Image { image = tex, scaleMode = ScaleMode.ScaleToFit, name = "NekoLibSceneIcon" };
            sceneIcon.style.width = 16;
            sceneIcon.style.height = 16;
            sceneIcon.style.marginRight = 4;
            if (before != null) container.Insert(container.IndexOf(before), sceneIcon); else container.Add(sceneIcon);
        }

        // --- Startup Scene Override Logic ---
        private static void LoadStartupPrefs()
        {
            startupScenePath = EditorPrefs.GetString(PrefStartupPath, string.Empty);
            if (!string.IsNullOrEmpty(startupScenePath) && !System.IO.File.Exists(startupScenePath))
            {
                // scene was removed
                startupScenePath = string.Empty;
                EditorPrefs.DeleteKey(PrefStartupPath);
            }
        }

        private static bool HasStartupScene() => !string.IsNullOrEmpty(startupScenePath);

        private static void ToggleStartupScene()
        {
            #region ToggleStartup
            var active = SceneManager.GetActiveScene();
            if (!HasStartupScene())
            {
                if (active.IsValid() && !string.IsNullOrEmpty(active.path))
                {
                    startupScenePath = active.path;
                    EditorPrefs.SetString(PrefStartupPath, startupScenePath);
                }
            }
            else
            {
                if (active.IsValid() && !string.IsNullOrEmpty(active.path) && !string.Equals(active.path, startupScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Switch mark to current scene instead of unmarking
                    startupScenePath = active.path;
                    EditorPrefs.SetString(PrefStartupPath, startupScenePath);
                }
                else
                {
                    startupScenePath = string.Empty;
                    EditorPrefs.DeleteKey(PrefStartupPath);
                }
            }
#if UNITY_2020_1_OR_NEWER
            PopulateToolbarMenu();
#endif
            #endregion
        }

        private static string StartupSceneName()
        {
            if (!HasStartupScene()) return string.Empty;
            string baseName = System.IO.Path.GetFileNameWithoutExtension(startupScenePath);
            // Find index to append if duplicate
            for (int i = 0; i < scenePaths.Length; i++)
            {
                if (string.Equals(scenePaths[i], startupScenePath, StringComparison.OrdinalIgnoreCase))
                {
                    if (duplicateNameCounts.TryGetValue(baseName, out var c) && c > 1)
                        return baseName + " (" + (i + 1) + ")";
                    return baseName;
                }
            }
            return baseName;
        }

        private static bool ScenePathMatchesStartup(string path)
        {
            if (!HasStartupScene()) return false;
            return string.Equals(path, startupScenePath, StringComparison.OrdinalIgnoreCase);
        }

#if UNITY_2020_1_OR_NEWER
        private static void AppendStartupMarkItem()
        {
            string label;
            var active = SceneManager.GetActiveScene();
            bool activeValid = active.IsValid() && !string.IsNullOrEmpty(active.path);
            // Helper: disambiguate active scene name if duplicates exist in build list
            string ActiveSceneDisplayName()
            {
                if (!activeValid) return "(Invalid)";
                string baseName = active.name;
                // attempt to find index among build scenes to match duplicate formatting
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    if (string.Equals(scenePaths[i], active.path, StringComparison.OrdinalIgnoreCase))
                    {
                        if (duplicateNameCounts.TryGetValue(baseName, out var c) && c > 1)
                            return baseName + " (" + (i + 1) + ")";
                        return baseName;
                    }
                }
                return baseName;
            }

            if (!HasStartupScene())
            {
                label = "Mark Active Scene As Startup";
                toolbarMenu.menu.AppendAction(label, _ => ToggleStartupScene(), _ => activeValid ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            }
            else
            {
                bool activeIsStartup = activeValid && ScenePathMatchesStartup(active.path);
                if (activeIsStartup)
                {
                    var name = StartupSceneName();
                    label = string.IsNullOrEmpty(name) ? "Unmark Startup Scene" : $"Unmark Startup Scene ({name})";
                    toolbarMenu.menu.AppendAction(label, _ => ToggleStartupScene(), _ => DropdownMenuAction.Status.Normal);
                }
                else
                {
                    var oldName = StartupSceneName();
                    var newName = ActiveSceneDisplayName();
                    label = $"Switch Startup Scene from ({oldName}) to ({newName})";
                    toolbarMenu.menu.AppendAction(label, _ => ToggleStartupScene(), _ => activeValid ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                }
            }
        }
#endif

        private static bool CanMarkActive()
        {
            var active = SceneManager.GetActiveScene();
            return active.IsValid() && !string.IsNullOrEmpty(active.path);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            #region PlayModeHook
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    if (!HasStartupScene()) { playSwitched = false; SessionState.SetBool(SessionPlaySwitchedKey, false); return; }
                    if (!System.IO.File.Exists(startupScenePath)) { startupScenePath = string.Empty; EditorPrefs.DeleteKey(PrefStartupPath); playSwitched = false; SessionState.SetBool(SessionPlaySwitchedKey, false); return; }
                    var active = SceneManager.GetActiveScene();
                    if (string.Equals(active.path, startupScenePath, StringComparison.OrdinalIgnoreCase)) { playSwitched = false; SessionState.SetBool(SessionPlaySwitchedKey, false); return; }
                    originalSceneBeforePlay = active.path;
                    SessionState.SetString(SessionOriginalSceneKey, originalSceneBeforePlay);
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorApplication.isPlaying = false; playSwitched = false; originalSceneBeforePlay = null; SessionState.EraseString(SessionOriginalSceneKey); SessionState.SetBool(SessionPlaySwitchedKey, false); return;
                    }
                    var opened = EditorSceneManager.OpenScene(startupScenePath);
                    SceneManager.SetActiveScene(opened);
                    playSwitched = true; SessionState.SetBool(SessionPlaySwitchedKey, true);
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    if ((playSwitched || SessionState.GetBool(SessionPlaySwitchedKey, false)) && !string.IsNullOrEmpty(originalSceneBeforePlay) && System.IO.File.Exists(originalSceneBeforePlay))
                    {
                        var current = SceneManager.GetActiveScene();
                        if (!string.Equals(current.path, originalSceneBeforePlay, StringComparison.OrdinalIgnoreCase))
                        {
                            var restored = EditorSceneManager.OpenScene(originalSceneBeforePlay);
                            SceneManager.SetActiveScene(restored);
                        }
                    }
                    playSwitched = false; SessionState.SetBool(SessionPlaySwitchedKey, false);
                    originalSceneBeforePlay = null; SessionState.EraseString(SessionOriginalSceneKey);
                    break;
            }
            #endregion
        }
    }
}
#endif
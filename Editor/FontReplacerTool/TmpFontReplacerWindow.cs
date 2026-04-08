#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NekoLib
{
    internal class TmpFontReplacerWindow : EditorWindow
    {
        [MenuItem("Tools/Neko Framework/TMP Font Replacer")]
        public static void Open() => GetWindow<TmpFontReplacerWindow>("TMP Font Replacer");

        // --- Font ---
        private TMP_FontAsset _targetFont;
        private Material[] _sdfMaterials = System.Array.Empty<Material>();
        private string[] _sdfMatNames = System.Array.Empty<string>();
        private int _sdfMatIndex;

        // --- Asset lists ---
        private int _tab;
        private Vector2 _sceneScroll;
        private Vector2 _prefabScroll;
        private List<AssetEntry> _scenes = new();
        private List<AssetEntry> _prefabs = new();
        private int _selectedSceneCount;
        private int _selectedPrefabCount;

        // --- Exclusions ---
        private List<DefaultAsset> _excludedFolders = new();
        private Vector2 _exclusionScroll;
        private bool _exclusionFoldout = true;

        // Row tint textures — built once in OnGUI, safe because they don't
        // depend on any GUIStyle or skin.
        private Texture2D _rowEvenTex;
        private Texture2D _rowOddTex;

        // -------------------------------------------------------------------------

        private class AssetEntry
        {
            public string Path;
            public string Name;
            public bool Selected;
        }

        // -------------------------------------------------------------------------

        private void OnEnable()
        {
            Scan();
            RebuildMaterialList();
        }

        // Row textures are plain Color→Texture2D; safe to build any time.
        private void EnsureRowTextures()
        {
            if (_rowEvenTex != null) return;
            _rowEvenTex = MakeTex(EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.88f, 0.88f, 0.88f));
            _rowOddTex = MakeTex(EditorGUIUtility.isProSkin
                ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.84f, 0.84f, 0.84f));
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        // Row styles are built inline each call — GUIStyle(GUIStyle.none) with
        // a background tex is safe inside OnGUI; no EditorStyles dependency.
        private GUIStyle RowStyle(int index)
        {
            return new GUIStyle(GUIStyle.none)
            {
                normal = { background = index % 2 == 0 ? _rowEvenTex : _rowOddTex },
                padding = new RectOffset(4, 4, 1, 1),
                margin = new RectOffset(0, 0, 0, 0),
                fixedHeight = 20,
            };
        }

        private static GUIStyle IconBtnStyle() => new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(2, 2, 1, 1),
            fixedWidth = 20,
            fixedHeight = 18,
        };

        // -------------------------------------------------------------------------

        private void Scan()
        {
            var excludedPaths = _excludedFolders
                .Where(f => f != null)
                .Select(f => AssetDatabase.GetAssetPath(f))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet();

            bool IsExcluded(string path)
                => excludedPaths.Any(ex => path.StartsWith(ex + "/") || path == ex);

            _scenes = AssetDatabase
                .FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !IsExcluded(p))
                .Select(p => new AssetEntry { Path = p, Name = System.IO.Path.GetFileName(p) })
                .OrderBy(e => e.Path)
                .ToList();

            _prefabs = AssetDatabase
                .FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !IsExcluded(p))
                .Select(p => new AssetEntry { Path = p, Name = System.IO.Path.GetFileName(p) })
                .OrderBy(e => e.Path)
                .ToList();

            _selectedSceneCount = 0;
            _selectedPrefabCount = 0;
        }

        private void RebuildMaterialList()
        {
            if (_targetFont == null)
            {
                _sdfMaterials = System.Array.Empty<Material>();
                _sdfMatNames = System.Array.Empty<string>();
                _sdfMatIndex = 0;
                return;
            }

            Texture2D atlas = _targetFont.atlasTexture;

            _sdfMaterials = AssetDatabase
                .FindAssets("t:Material")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(p => AssetDatabase.LoadAssetAtPath<Material>(p))
                .Where(m => m != null &&
                            m.HasProperty("_MainTex") &&
                            m.GetTexture("_MainTex") == atlas)
                .OrderBy(m => m.name)
                .ToArray();

            _sdfMatNames = _sdfMaterials.Select(m => m.name).ToArray();

            _sdfMatIndex = System.Array.IndexOf(_sdfMaterials, _targetFont.material);
            if (_sdfMatIndex < 0) _sdfMatIndex = 0;
        }

        private Material SelectedMaterial
            => _sdfMaterials.Length > 0 ? _sdfMaterials[_sdfMatIndex] : null;

        private void SetSelected(AssetEntry entry, List<AssetEntry> list, bool value)
        {
            if (entry.Selected == value) return;
            entry.Selected = value;
            if (list == _scenes) _selectedSceneCount += value ? 1 : -1;
            else _selectedPrefabCount += value ? 1 : -1;
        }

        private void SetAllSelected(List<AssetEntry> list, bool value)
        {
            foreach (var e in list) e.Selected = value;
            if (list == _scenes) _selectedSceneCount = value ? list.Count : 0;
            else _selectedPrefabCount = value ? list.Count : 0;
        }

        // -------------------------------------------------------------------------
        // OnGUI
        // -------------------------------------------------------------------------

        private void OnGUI()
        {
            // Row textures are the only thing we lazily init here — no GUIStyle
            // references, so safe at any point during OnGUI.
            EnsureRowTextures();

            DrawFontSection();
            DrawSeparator();
            DrawExclusionSection();
            DrawSeparator();
            DrawAssetSection();
            GUILayout.FlexibleSpace();
            DrawSeparator();
            DrawFooter();
        }

        // -------------------------------------------------------------------------
        // Shared drawing helpers
        // -------------------------------------------------------------------------

        private static void DrawSectionHeader(string title)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.76f, 0.76f, 0.76f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height),
                new Color(0.27f, 0.55f, 0.98f));
            // EditorStyles.boldLabel is safe here — we are inside OnGUI.
            EditorGUI.LabelField(
                new Rect(rect.x + 10f, rect.y, rect.width - 10f, rect.height),
                title, EditorStyles.boldLabel);
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space(2);
            EditorGUI.DrawRect(
                GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true)),
                EditorGUIUtility.isProSkin
                    ? new Color(0.13f, 0.13f, 0.13f)
                    : new Color(0.6f, 0.6f, 0.6f));
            EditorGUILayout.Space(2);
        }

        // -------------------------------------------------------------------------
        // Section 1 — Font Settings
        // -------------------------------------------------------------------------

        private void DrawFontSection()
        {
            DrawSectionHeader("Font Settings");
            EditorGUILayout.Space(4);

            EditorGUI.BeginChangeCheck();
            var newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
                "Font Asset", _targetFont, typeof(TMP_FontAsset), allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck())
            {
                _targetFont = newFont;
                RebuildMaterialList();
            }

            EditorGUILayout.Space(2);

            if (_targetFont != null)
            {
                if (_sdfMaterials.Length > 0)
                {
                    _sdfMatIndex = EditorGUILayout.Popup(
                        "SDF Material", _sdfMatIndex, _sdfMatNames);
                }
                else
                {
                    EditorGUILayout.LabelField("SDF Material", "No presets found",
                        EditorStyles.miniLabel);
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.Popup("SDF Material", 0, new[] { "-" });
            }

            EditorGUILayout.Space(4);
        }

        // -------------------------------------------------------------------------
        // Section 2 — Excluded Folders
        // -------------------------------------------------------------------------

        private void DrawExclusionSection()
        {
            Rect headerRect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(headerRect, EditorGUIUtility.isProSkin
                ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.76f, 0.76f, 0.76f));
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.y, 3f, headerRect.height),
                new Color(0.27f, 0.55f, 0.98f));

            _exclusionFoldout = EditorGUI.Foldout(
                new Rect(headerRect.x + 6f, headerRect.y,
                    headerRect.width - 80f, headerRect.height),
                _exclusionFoldout, "Excluded Folders", true, EditorStyles.boldLabel);

            if (GUI.Button(
                    new Rect(headerRect.xMax - 72f, headerRect.y + 3f, 68f, 16f),
                    "Rescan", EditorStyles.miniButton))
                Scan();

            if (!_exclusionFoldout) return;

            EditorGUILayout.Space(2);

            if (_excludedFolders.Count > 0)
            {
                float maxH = 20f * Mathf.Min(_excludedFolders.Count, 4);
                using (var sv = new EditorGUILayout.ScrollViewScope(
                    _exclusionScroll, GUILayout.Height(maxH)))
                {
                    _exclusionScroll = sv.scrollPosition;
                    for (int i = _excludedFolders.Count - 1; i >= 0; i--)
                    {
                        using (new EditorGUILayout.HorizontalScope(RowStyle(i)))
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(
                                _excludedFolders[i], typeof(DefaultAsset),
                                allowSceneObjects: false, GUILayout.Height(16));
                            EditorGUI.EndDisabledGroup();

                            if (GUILayout.Button("✕", IconBtnStyle()))
                            {
                                _excludedFolders.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

            // Silent drop zone — tooltip only, no visible label.
            Rect dropRect = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
            GUI.Box(dropRect,
                new GUIContent("", "Drag folders from the Project window to exclude them"),
                EditorStyles.helpBox);
            HandleFolderDrop(dropRect);

            EditorGUILayout.Space(2);
        }

        private void HandleFolderDrop(Rect dropRect)
        {
            Event evt = Event.current;
            if (!dropRect.Contains(evt.mousePosition)) return;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;

            bool anyFolder = DragAndDrop.objectReferences.Any(
                obj => obj is DefaultAsset da &&
                       AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(da)));

            DragAndDrop.visualMode = anyFolder
                ? DragAndDropVisualMode.Copy
                : DragAndDropVisualMode.Rejected;

            if (evt.type == EventType.DragPerform && anyFolder)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is not DefaultAsset da) continue;
                    if (!AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(da))) continue;
                    if (!_excludedFolders.Contains(da)) _excludedFolders.Add(da);
                }
            }

            evt.Use();
        }

        // -------------------------------------------------------------------------
        // Section 3 — Asset Selection
        // -------------------------------------------------------------------------

        private void DrawAssetSection()
        {
            DrawSectionHeader("Assets to Process");
            EditorGUILayout.Space(2);

            var scenes = _scenes;
            var prefabs = _prefabs;

            // Toolbar row — tabs + count + All / None.
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Toggle(_tab == 0,
                        $"Scenes ({scenes.Count})", EditorStyles.toolbarButton))
                    _tab = 0;
                if (GUILayout.Toggle(_tab == 1,
                        $"Prefabs ({prefabs.Count})", EditorStyles.toolbarButton))
                    _tab = 1;

                GUILayout.FlexibleSpace();

                var currentList = _tab == 0 ? scenes : prefabs;
                int selectedCount = _tab == 0 ? _selectedSceneCount : _selectedPrefabCount;

                GUILayout.Label($"{selectedCount} / {currentList.Count}",
                    EditorStyles.toolbarButton, GUILayout.Width(56));

                if (GUILayout.Button("All", EditorStyles.toolbarButton, GUILayout.Width(36)))
                    SetAllSelected(currentList, true);
                if (GUILayout.Button("None", EditorStyles.toolbarButton, GUILayout.Width(42)))
                    SetAllSelected(currentList, false);
            }

            // Re-read after possible tab change.
            var list = _tab == 0 ? scenes : prefabs;
            float rowH = EditorGUIUtility.singleLineHeight + 2f;
            float listHeight = Mathf.Clamp(list.Count * rowH, 60f, 280f);
            Vector2 scroll = _tab == 0 ? _sceneScroll : _prefabScroll;

            using (var sv = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(listHeight)))
            {
                scroll = sv.scrollPosition;

                if (list.Count == 0)
                {
                    EditorGUILayout.Space(8);
                    EditorGUILayout.LabelField("No assets found.",
                        EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var entry = list[i];
                        using (new EditorGUILayout.HorizontalScope(RowStyle(i)))
                        {
                            bool next = EditorGUILayout.ToggleLeft(
                                new GUIContent(entry.Name, entry.Path),
                                entry.Selected, GUILayout.Height(16));

                            if (next != entry.Selected)
                                SetSelected(entry, list, next);

                            if (GUILayout.Button(
                                    new GUIContent("→", "Ping in Project window"),
                                    IconBtnStyle()))
                                EditorGUIUtility.PingObject(
                                    AssetDatabase.LoadAssetAtPath<Object>(entry.Path));
                        }
                    }
                }
            }

            if (_tab == 0) _sceneScroll = scroll;
            else _prefabScroll = scroll;
        }

        // -------------------------------------------------------------------------
        // Section 4 — Footer
        // -------------------------------------------------------------------------

        private void DrawFooter()
        {
            EditorGUILayout.Space(4);

            int totalSelected = _selectedSceneCount + _selectedPrefabCount;
            bool canRun = _targetFont != null && totalSelected > 0;

            EditorGUILayout.LabelField(
                $"{_selectedSceneCount} scene(s)  ·  {_selectedPrefabCount} prefab(s) selected",
                EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(!canRun))
            {
                if (GUILayout.Button("Replace Fonts", GUILayout.Height(28)))
                    RunReplacement();
            }

            if (!canRun)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField(
                    _targetFont == null
                        ? "Assign a Font Asset to continue."
                        : "Select at least one scene or prefab.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.Space(4);
        }

        // -------------------------------------------------------------------------
        // Replacement
        // -------------------------------------------------------------------------

        private void RunReplacement()
        {
            Material mat = SelectedMaterial;

            bool confirm = EditorUtility.DisplayDialog(
                "Replace TMP Fonts",
                $"Font: {_targetFont.name}\nMaterial: {(mat != null ? mat.name : "none")}" +
                "\n\nCommit or stash before proceeding.",
                "Replace", "Cancel");

            if (!confirm) return;

            int totalFound = 0;
            int totalReplaced = 0;

            void ProcessText(TMP_Text text)
            {
                totalFound++;
                bool changed = false;

                if (text.font != _targetFont)
                {
                    text.font = _targetFont;
                    changed = true;
                }

                if (mat != null && text.fontSharedMaterial != mat)
                {
                    text.fontSharedMaterial = mat;
                    changed = true;
                }

                if (changed) totalReplaced++;
            }

            foreach (var entry in _prefabs.Where(e => e.Selected))
            {
                using var scope = new PrefabUtility.EditPrefabContentsScope(entry.Path);
                foreach (var text in scope.prefabContentsRoot
                    .GetComponentsInChildren<TMP_Text>(true))
                    ProcessText(text);
            }

            string activeScenePath = SceneManager.GetActiveScene().path;
            foreach (var entry in _scenes.Where(e => e.Selected))
            {
                var scene = EditorSceneManager.OpenScene(entry.Path, OpenSceneMode.Additive);
                bool dirty = false;

                foreach (var root in scene.GetRootGameObjects())
                    foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    {
                        int before = totalReplaced;
                        ProcessText(text);
                        if (totalReplaced > before) dirty = true;
                    }

                if (dirty) EditorSceneManager.SaveScene(scene);
                if (entry.Path != activeScenePath)
                    EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.SaveAssets();

            string message = totalFound == 0
                ? "No TMP_Text components found in the selected assets."
                : totalReplaced == 0
                    ? $"Found {totalFound} TMP_Text component(s) — all already up to date. Nothing changed."
                    : $"Found {totalFound} TMP_Text component(s). Updated {totalReplaced}, {totalFound - totalReplaced} already correct.";

            EditorUtility.DisplayDialog("Done", message, "OK");
        }
    }
}
#endif
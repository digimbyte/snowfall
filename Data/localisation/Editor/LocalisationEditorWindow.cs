#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Core.Localisation.Editor
{
    public class LocalisationEditorWindow : EditorWindow
    {
        private const string Folder = "Assets/Core/Data/localisation";
        private const string LangPrefix = "lang_";

        [SerializeField]
        private LocalisationClientConfig config;

        private Vector2 scroll;

        [MenuItem("Core/Localisation/Editor")]
        public static void Open()
        {
            var win = GetWindow<LocalisationEditorWindow>("Localisation");
            win.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6);

            config = (LocalisationClientConfig)EditorGUILayout.ObjectField("Client Config", config, typeof(LocalisationClientConfig), false);

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate defs + langs", GUILayout.Height(28)))
            {
                LocalisationCodegen.RegenerateAll();
            }

            if (GUILayout.Button("Sync Config Languages", GUILayout.Height(28)))
            {
                if (config == null)
                {
                    Debug.LogWarning("[LocalisationEditorWindow] Assign a LocalisationClientConfig first.");
                }
                else
                {
                    // Invoke the context menu method directly.
                    var mi = typeof(LocalisationClientConfig).GetMethod("Editor_SyncLanguageYamlAssets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    mi?.Invoke(config, null);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Discovered lang_<id>.yaml files", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var info in FindLangYamlAssets())
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(info.id, GUILayout.Width(120));
                EditorGUILayout.LabelField(info.assetPath);
                if (GUILayout.Button("Ping", GUILayout.Width(60)))
                {
                    var ta = AssetDatabase.LoadAssetAtPath<TextAsset>(info.assetPath);
                    if (ta != null) EditorGUIUtility.PingObject(ta);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Keys live in YAML (lang_<id>.yaml). defs/langs are generated for IDE-safe access.\n" +
                "Missing keys return $<path>. Nodes can have self-text via a reserved '$' key (defs.<node>.self).",
                MessageType.Info);
        }

        private static List<(string id, string assetPath)> FindLangYamlAssets()
        {
            var result = new List<(string id, string assetPath)>();
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { Folder });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var file = Path.GetFileName(path);
                if (file == null) continue;

                if (!file.StartsWith(LangPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!file.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) &&
                    !file.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                    continue;

                var id = Path.GetFileNameWithoutExtension(file).Substring(LangPrefix.Length);
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                result.Add((id.Trim(), path));
            }

            result.Sort((a, b) => string.Compare(a.id, b.id, StringComparison.OrdinalIgnoreCase));
            return result;
        }
    }
}
#endif

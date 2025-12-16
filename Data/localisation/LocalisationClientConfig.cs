using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
#endif

namespace Core.Localisation
{
    /// <summary>
    /// Runtime config for localisation. The editor tool owns keeping this in sync with lang_<id>.yaml files.
    /// This is not intended as the main authoring surface for strings.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalisationClientConfig", menuName = "Core/Data/Localisation Client Config")]
    public class LocalisationClientConfig : ScriptableObject
    {
        [Serializable]
        public class LanguageFile
        {
            [Tooltip("Language id (matches lang_<id>.yaml)")]
            public string id;

            [Tooltip("YAML file for this language.")]
            public TextAsset yaml;
        }

        [Tooltip("Default language id to fall back to.")]
        public string defaultLanguageId = "en";

        [Tooltip("Language files available to the client.")]
        public List<LanguageFile> languages = new List<LanguageFile>();

        public TextAsset GetYaml(string languageId)
        {
            if (languages == null || string.IsNullOrEmpty(languageId))
                return null;

            for (int i = 0; i < languages.Count; i++)
            {
                var lf = languages[i];
                if (lf == null) continue;
                if (string.Equals(lf.id, languageId, StringComparison.OrdinalIgnoreCase))
                    return lf.yaml;
            }

            return null;
        }

#if UNITY_EDITOR
        private const string LangFolder = "Assets/Core/Data/localisation";
        private const string LangPrefix = "lang_";

        [ContextMenu("Sync Language YAMLs (Assets/Core/Data/localisation)")]
        private void Editor_SyncLanguageYamlAssets()
        {
            if (languages == null)
                languages = new List<LanguageFile>();

            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { LangFolder });
            var found = new List<LanguageFile>();

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

                var yaml = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (yaml == null)
                    continue;

                found.Add(new LanguageFile { id = id.Trim(), yaml = yaml });
            }

            // Stable order.
            found = found.OrderBy(x => x.id, StringComparer.OrdinalIgnoreCase).ToList();

            languages.Clear();
            languages.AddRange(found);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"[LocalisationClientConfig] Synced {languages.Count} language YAML assets.");
        }
#endif
    }
}

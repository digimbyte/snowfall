using UnityEngine;

namespace Core.Localisation
{
    /// <summary>
    /// Callsite API:
    ///     client.set_config(config);
    ///     client.use(langs.en);
    ///     var text = localise.get(defs.UI.menu.start);
    /// </summary>
    public static class localise
    {
        public static string get(LocalKey key) => client.get(key);
    }

    public static class client
    {
        private static LocalisationClientConfig _config;

        public static void set_config(LocalisationClientConfig config)
        {
            _config = config;
        }

        public static LocalLanguage current { get; private set; }

        public static void use(LocalLanguage language)
        {
            current = language;

            if (_config == null)
            {
                Debug.LogWarning("[Localisation.client] No LocalisationClientConfig assigned. Localisation will return $-prefixed keys.");
                Localisation.Clear();
                return;
            }

            var yaml = _config.GetYaml(language.Id);
            if (yaml == null)
            {
                // fallback to default language file if available
                yaml = _config.GetYaml(_config.defaultLanguageId);
            }

            if (yaml == null)
            {
                Debug.LogWarning($"[Localisation.client] No YAML found for '{language.Id}' (or default '{_config.defaultLanguageId}').");
                Localisation.Clear();
                return;
            }

            var table = YamlLocalisationTable.Parse(yaml.text);
            Localisation.SetTable(table);
        }

        public static string get(LocalKey key)
        {
            // TODO (Phase B): route to parsed YAML table.
            return Localisation.Get(key);
        }
    }
}

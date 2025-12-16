using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Localisation
{
    /// <summary>
    /// Runtime localisation store.
    ///
    /// Callsites should go through:
    ///     localise.get(defs...)
    /// which routes into:
    ///     Localisation.Get(LocalKey)
    ///
    /// Storage is YAML-first: the active language loads a flattened table
    /// (path -> text) into this store.
    /// </summary>
    public static class Localisation
    {
        private static IReadOnlyDictionary<string, string> _table;
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Replace the current language table.
        /// </summary>
        public static void SetTable(IReadOnlyDictionary<string, string> table)
        {
            _table = table;
            _cache.Clear();
        }

        public static void Clear()
        {
            SetTable(null);
        }

        /// <summary>
        /// Get the localised string for a strongly-typed LocalKey.
        /// Never accepts raw strings at callsites.
        /// </summary>
        public static string Get(LocalKey key)
        {
            string path = key.Path;
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            if (_cache.TryGetValue(path, out var cached))
                return cached;

            string resolved = Resolve(path, key.Ref);
            _cache[path] = resolved;
            return resolved;
        }

        private static string Resolve(string path, string fallbackRef)
        {
            if (_table == null)
                return fallbackRef;

            if (_table.TryGetValue(path, out var value) && !string.IsNullOrEmpty(value))
                return value;

            return fallbackRef;
        }
    }
}

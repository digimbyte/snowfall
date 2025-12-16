using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace Core.Localisation
{
    /// <summary>
    /// Flattens nested YAML mappings into a table of:
    ///     "UI.menu.start" -> "Start Game"
    ///
    /// Supports nodes that have both text and children via a reserved key:
    ///     $: "node text"
    /// which maps to the node's own path:
    ///     "UI.menu" -> "node text"
    /// </summary>
    public static class YamlLocalisationTable
    {
        public const string SelfTextKey = "$";

        public static Dictionary<string, string> Parse(string yamlText)
        {
            var table = new Dictionary<string, string>(StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(yamlText))
                return table;

            var yaml = new YamlStream();
            using (var reader = new StringReader(yamlText))
            {
                yaml.Load(reader);
            }

            if (yaml.Documents.Count == 0)
                return table;

            var root = yaml.Documents[0].RootNode;
            Walk(root, new List<string>(), table);

            return table;
        }

        private static void Walk(YamlNode node, List<string> segments, Dictionary<string, string> table)
        {
            if (node is YamlMappingNode map)
            {
                // Node self text: "$: ..." maps to the node's own path.
                foreach (var kv in map.Children)
                {
                    if (kv.Key is not YamlScalarNode keyScalar)
                        continue;

                    string rawKey = keyScalar.Value ?? string.Empty;
                    if (!string.Equals(rawKey, SelfTextKey, StringComparison.Ordinal))
                        continue;

                    if (kv.Value is YamlScalarNode selfScalar)
                    {
                        string path = JoinPath(segments);
                        if (!string.IsNullOrEmpty(path))
                        {
                            table[path] = selfScalar.Value ?? string.Empty;
                        }
                    }
                }

                // Children.
                foreach (var kv in map.Children)
                {
                    if (kv.Key is not YamlScalarNode keyScalar)
                        continue;

                    string rawKey = keyScalar.Value ?? string.Empty;
                    if (string.Equals(rawKey, SelfTextKey, StringComparison.Ordinal))
                        continue;

                    foreach (var childSeg in ExpandSegments(rawKey))
                    {
                        // ExpandSegments returns a sequence (possibly multiple segments when '.' appears).
                        // We apply them one by one.
                        segments.Add(childSeg);
                    }

                    if (kv.Value is YamlScalarNode valueScalar)
                    {
                        string path = JoinPath(segments);
                        if (!string.IsNullOrEmpty(path))
                        {
                            table[path] = valueScalar.Value ?? string.Empty;
                        }
                    }
                    else
                    {
                        Walk(kv.Value, segments, table);
                    }

                    // Pop the segments we added for this key.
                    int countToRemove = CountExpandedSegments(rawKey);
                    if (countToRemove > 0 && segments.Count >= countToRemove)
                    {
                        segments.RemoveRange(segments.Count - countToRemove, countToRemove);
                    }
                }

                return;
            }

            if (node is YamlSequenceNode seq)
            {
                for (int i = 0; i < seq.Children.Count; i++)
                {
                    segments.Add(i.ToString());
                    Walk(seq.Children[i], segments, table);
                    segments.RemoveAt(segments.Count - 1);
                }

                return;
            }

            // Root scalar is ignored; leaf scalars are handled by parent mapping.
        }

        private static IEnumerable<string> ExpandSegments(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                yield break;

            // Safety: normalise whitespace.
            rawKey = rawKey.Trim();

            // Support legacy dotted keys by splitting, but keep it visible.
            // Editor tooling should prevent dotted keys in YAML headers.
            var parts = rawKey.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                Debug.LogWarning($"[YamlLocalisationTable] Key '{rawKey}' contains '.'. Prefer nested YAML instead of dotted keys.");
            }

            foreach (var p in parts)
            {
                var seg = NormalizeSegment(p);
                if (!string.IsNullOrEmpty(seg))
                    yield return seg;
            }
        }

        private static int CountExpandedSegments(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                return 0;

            return rawKey.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static string NormalizeSegment(string seg)
        {
            if (string.IsNullOrWhiteSpace(seg))
                return string.Empty;

            // IDs should be stable (snake_case recommended). We at least normalise spaces.
            return seg.Trim().Replace(' ', '_');
        }

        private static string JoinPath(List<string> segments)
        {
            if (segments == null || segments.Count == 0)
                return string.Empty;

            return string.Join(".", segments);
        }
    }
}

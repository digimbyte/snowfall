using System;
using UnityEngine;

namespace Core.Localisation
{
    /// <summary>
    /// Strongly-typed localisation key, conceptually similar to SerialEnum
    /// but dedicated to localisation/dialog trees.
    ///
    /// It serialises as a single underlying string path like
    /// "local.campaign.3.poison.44", but user code never deals with raw
    /// strings directly – keys are provided via generated nested classes.
    /// </summary>
    [Serializable]
    public struct LocalKey : IEquatable<LocalKey>
    {
        [SerializeField]
        private string path;

        internal LocalKey(string path)
        {
            this.path = path;
        }

        /// <summary>
        /// Underlying path (e.g. "UI.menu.start_menu").
        /// </summary>
        public string Path => path ?? string.Empty;

        /// <summary>
        /// $-prefixed reference form (e.g. "$UI.menu.start_menu").
        /// Used as a safe missing-key placeholder.
        /// </summary>
        public string Ref => "$" + Path;

        public override string ToString() => Path;

        public bool Equals(LocalKey other) => string.Equals(Path, other.Path, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is LocalKey other && Equals(other);

        public override int GetHashCode() => Path != null ? StringComparer.Ordinal.GetHashCode(Path) : 0;

        public static bool operator ==(LocalKey left, LocalKey right) => left.Equals(right);
        public static bool operator !=(LocalKey left, LocalKey right) => !left.Equals(right);

    }
}

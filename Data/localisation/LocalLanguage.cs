using System;
using UnityEngine;

namespace Core.Localisation
{
    /// <summary>
    /// Strongly-typed language id (e.g. "en").
    /// Intended to be used via generated `langs` constants (IDE-safe).
    /// </summary>
    [Serializable]
    public struct LocalLanguage : IEquatable<LocalLanguage>
    {
        [SerializeField]
        private string id;

        internal LocalLanguage(string id)
        {
            this.id = id;
        }

        public string Id => id ?? string.Empty;

        public override string ToString() => Id;

        public bool Equals(LocalLanguage other) => string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj) => obj is LocalLanguage other && Equals(other);

        public override int GetHashCode() => Id != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Id) : 0;

        public static bool operator ==(LocalLanguage left, LocalLanguage right) => left.Equals(right);
        public static bool operator !=(LocalLanguage left, LocalLanguage right) => !left.Equals(right);
    }
}

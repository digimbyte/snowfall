using System;
using UnityEngine;

namespace Core.Enums
{
    /// <summary>
    /// Serializable string-backed enum-like value.
    /// In generated code you will use it like: SerialEnum ammoKey = ammo.medium;
    /// and compare with: if (ammoKey == ammo.medium) { ... }
    /// </summary>
    [Serializable]
    public struct SerialEnum : IEquatable<SerialEnum>
    {
        [SerializeField]
        private string key;

        public SerialEnum(string key)
        {
            this.key = key;
        }

        /// <summary>
        /// Underlying string key, e.g. "ammo.medium".
        /// </summary>
        public string Key => key ?? string.Empty;

        public override string ToString() => Key;

        // Equality by key (ordinal)
        public bool Equals(SerialEnum other) => string.Equals(Key, other.Key, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is SerialEnum other && Equals(other);

        public override int GetHashCode() => Key != null ? StringComparer.Ordinal.GetHashCode(Key) : 0;

        public static bool operator ==(SerialEnum left, SerialEnum right) => left.Equals(right);
        public static bool operator !=(SerialEnum left, SerialEnum right) => !left.Equals(right);

        // Convenience implicit conversion to string when needed.
        public static implicit operator string(SerialEnum value) => value.Key;
    }
}

using System;
using System.Security;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;

namespace Fb2.Document.Models.Base
{
    public class Fb2Attribute
    {
        public string Key { get; }
        public string Value { get; set; }
        public string NamespaceName { get; } = null;

        public Fb2Attribute(string key, string value, string namespaceName = null)
        {
            var escapedKey = SecurityElement.Escape(key);
            if (string.IsNullOrWhiteSpace(escapedKey))
                throw new InvalidAttributeException(key);

            // because value can actually be empty string ))
            var escapedValue = SecurityElement.Escape(value) ?? string.Empty;

            Key = escapedKey;
            Value = escapedValue;

            if (!string.IsNullOrWhiteSpace(namespaceName))
                NamespaceName = namespaceName;
        }

        public override bool Equals(object obj) =>
            obj != null &&
            obj is Fb2Attribute attribute &&
            (ReferenceEquals(this, attribute) ||
            (Key.EqualsIgnoreCase(attribute.Key) &&
            Value.Equals(attribute.Value, StringComparison.InvariantCulture) &&
            NamespaceName == attribute.NamespaceName));

        public override int GetHashCode() => HashCode.Combine(Key, Value, NamespaceName);
    }
}

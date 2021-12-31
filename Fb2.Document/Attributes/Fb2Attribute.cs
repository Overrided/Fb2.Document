using System;
using System.Security;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;

namespace Fb2.Document.Attributes
{
    public class Fb2Attribute
    {
        public string Key { get; }
        public string Value { get; set; }
        public string? NamespaceName { get; } = null;

        public Fb2Attribute(string key, string value, string? namespaceName = null)
        {
            var escapedKey = SecurityElement.Escape(key);
            if (string.IsNullOrWhiteSpace(escapedKey))
                throw new InvalidAttributeException(nameof(key));

            var escapedValue = SecurityElement.Escape(value);
            if (string.IsNullOrWhiteSpace(escapedValue))
                throw new InvalidAttributeException(nameof(value));

            Key = escapedKey;
            Value = escapedValue;

            if (!string.IsNullOrWhiteSpace(namespaceName))
                NamespaceName = namespaceName;
        }

        public override bool Equals(object? obj) =>
            obj != null &&
            obj is Fb2Attribute attribute &&
            Key.EqualsInvariant(attribute.Key) &&
            Value.Equals(attribute.Value, StringComparison.InvariantCulture) &&
            NamespaceName == attribute.NamespaceName;

        public override int GetHashCode() => HashCode.Combine(Key, Value, NamespaceName);
    }
}

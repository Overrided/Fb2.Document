using System;
using System.Security;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;

namespace Fb2.Document.Models.Base;

/// <summary>
/// Represents Fb2 Attribute key-value pair.
/// </summary>
public class Fb2Attribute
{
    /// <summary>
    /// Attribute Name/Key. For list of all standard Fb2 attributes please see <see cref="Constants.AttributeNames"/>.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Attribute value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Metadata part, points to attribute XML Namespace if any. Used for serialization.
    /// </summary>
    public string? NamespaceName { get; }

    /// <summary>
    /// Creates new instance of <see cref="Fb2Attribute"/>.
    /// </summary>
    /// <param name="key">Attribute Name/Key. For list of all standard Fb2 attributes please see <see cref="Constants.AttributeNames"/>.</param>
    /// <param name="value">Attribute value.</param>
    /// <param name="namespaceName">Metadata part, points to attribute XML Namespace if any. Used for serialization. This parameter is optional.</param>
    /// <exception cref="InvalidAttributeException"></exception>
    public Fb2Attribute(string key, string value, string? namespaceName = null)
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

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="other"><see cref="Fb2Attribute"/> instance to be copied.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Attribute(Fb2Attribute other)
    {
        ArgumentNullException.ThrowIfNull(other, nameof(other));

        Key = other.Key;
        Value = other.Value;
        NamespaceName = other.NamespaceName;
    }

    public override bool Equals(object? obj) =>
        obj != null &&
        obj is Fb2Attribute attribute &&
        (ReferenceEquals(this, attribute) ||
        (Key.EqualsIgnoreCase(attribute.Key) &&
        Value.Equals(attribute.Value, StringComparison.InvariantCulture) &&
        NamespaceName == attribute.NamespaceName));

    public override int GetHashCode() => HashCode.Combine(Key, Value, NamespaceName);
}

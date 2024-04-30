using System;
using System.Security;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;

namespace Fb2.Document.Models.Base;

/// <summary>
/// 
/// </summary>
public class Fb2Attribute
{
    /// <summary>
    /// 
    /// </summary>
    public string Key { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string? NamespaceName { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="namespaceName"></param>
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

    public override bool Equals(object? obj) =>
        obj != null &&
        obj is Fb2Attribute attribute &&
        (ReferenceEquals(this, attribute) ||
        (Key.EqualsIgnoreCase(attribute.Key) &&
        Value.Equals(attribute.Value, StringComparison.InvariantCulture) &&
        NamespaceName == attribute.NamespaceName));

    public override int GetHashCode() => HashCode.Combine(Key, Value, NamespaceName);
}

﻿using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown on attempt to create <see cref="Models.Base.Fb2Attribute"/> with null or empty <see cref="Models.Base.Fb2Attribute.Key"/>.
/// </summary>
public class InvalidAttributeException : Exception
{
    /// <summary>
    /// Invalid Key that was reason of an exception.
    /// </summary>
    public string AttributeKey { get; }

    /// <summary>
    /// Creates new instance of <see cref="InvalidAttributeException"/>.
    /// </summary>
    /// <param name="attributeKey">Attribute Name that caused exception.</param>
    public InvalidAttributeException(string attributeKey) :
        base("AttributeKey is null or empty string, or contains invalid characters.")
    {
        AttributeKey = attributeKey;
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Fb2.Document.Models.Base;

/// <summary>
/// Represents metadata used for serializing <see cref="Fb2Node"/> to XML.
/// </summary>
public class Fb2NodeMetadata
{
    /// <summary>
    /// Default Namespace of original <see cref="XNode"/>.
    /// </summary>
    public XNamespace? DefaultNamespace { get; private set; }

    /// <summary>
    /// Namespace Declaration attributes of original <see cref="XNode"/>.
    /// </summary>
    public List<XAttribute>? NamespaceDeclarations { get; private set; }

    /// <summary>
    /// Creates new instance of <see cref="Fb2NodeMetadata"/>.
    /// </summary>
    /// <param name="defaultNamespace"> Default Namespace of original <see cref="XNode"/>. Optional, <see langword="null"/> by default.</param>
    /// <param name="namespaceDeclarations">Set of Namespace Declaration Attributes of original <see cref="XNode"/>. </param>
    /// <exception cref="ArgumentException"></exception>
    public Fb2NodeMetadata(
        XNamespace? defaultNamespace = null,
        IEnumerable<XAttribute>? namespaceDeclarations = null)
    {
        if (defaultNamespace != null)
            DefaultNamespace = defaultNamespace;

        if (namespaceDeclarations != null && namespaceDeclarations.Any())
        {
            var namespaceDeclarationsOnly = namespaceDeclarations.All(attr => attr.IsNamespaceDeclaration);
            if (!namespaceDeclarationsOnly)
                throw new ArgumentException($"{nameof(namespaceDeclarations)} should contain Namespace Declarations attributes only.");

            NamespaceDeclarations = new(namespaceDeclarations);
        }
    }

    /// <summary>
    /// copy-constructor
    /// </summary>
    /// <param name="other">Metadata to copy</param>
    public Fb2NodeMetadata(Fb2NodeMetadata other)
    {
        DefaultNamespace = other.DefaultNamespace;

        NamespaceDeclarations = other.NamespaceDeclarations != null && other.NamespaceDeclarations.Any() ?
            new(other.NamespaceDeclarations) :
            null;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj is not Fb2NodeMetadata otherMetadata)
            return false;

        if (ReferenceEquals(this, otherMetadata))
            return true;

        if (DefaultNamespace == null &&
            NamespaceDeclarations == null &&
            otherMetadata.DefaultNamespace == null &&
            otherMetadata.NamespaceDeclarations == null)
            return true;

        var defaultNamespaceAreEqual = otherMetadata.DefaultNamespace == DefaultNamespace;
        if (!defaultNamespaceAreEqual)
            return false;

        var namespaceDeclarationsAreEqual =
            NamespaceDeclarations == null && otherMetadata.NamespaceDeclarations == null ||
            (NamespaceDeclarations?.Count == otherMetadata.NamespaceDeclarations?.Count &&
            NamespaceDeclarations != null && otherMetadata.NamespaceDeclarations != null &&
            NamespaceDeclarations.All(nd =>
                otherMetadata.NamespaceDeclarations!.Any(nd2 =>
                    nd2.IsNamespaceDeclaration == nd.IsNamespaceDeclaration &&
                    nd2.Name == nd.Name &&
                    nd2.Value.Equals(nd.Value)
            )));

        return namespaceDeclarationsAreEqual;
    }

    public override int GetHashCode() => HashCode.Combine(DefaultNamespace, NamespaceDeclarations);
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Fb2.Document.Models.Base
{
    public class Fb2NodeMetadata
    {
        public XNamespace? DefaultNamespace { get; private set; }

        public List<XAttribute>? NamespaceDeclarations { get; private set; }

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

                NamespaceDeclarations = new List<XAttribute>(namespaceDeclarations);
            }
        }

        /// <summary>
        /// copy-constructor
        /// </summary>
        /// <param name="other"></param>
        public Fb2NodeMetadata(Fb2NodeMetadata other)
        {
            DefaultNamespace = other.DefaultNamespace;

            NamespaceDeclarations = other.NamespaceDeclarations != null && other.NamespaceDeclarations.Any() ?
                new List<XAttribute>(other.NamespaceDeclarations) :
                null;
        }

        // maybe later
        //public static bool operator ==(Fb2NodeMetadata? left, Fb2NodeMetadata? right)
        //{
        //    // cant use == here lol
        //    var bothAreNull = left is null && right is null;
        //    var bothAreNotNull = !(left is null) && !(right is null);

        //    if (!bothAreNull && !bothAreNotNull) // one is null other one is not
        //        return false;

        //    if (bothAreNull)
        //        return true;

        //    if (bothAreNotNull)
        //        return left!.Equals(right!);

        //    return false;
        //}

        //public static bool operator !=(Fb2NodeMetadata? left, Fb2NodeMetadata? right)
        //{
        //    return !(left == right);
        //}

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Fb2NodeMetadata otherMetadata))
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
}

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
    }
}

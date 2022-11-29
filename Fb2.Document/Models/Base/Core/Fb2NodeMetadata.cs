﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Fb2.Document.Models.Base
{
    public class Fb2NodeMetadata
    {
        public XNamespace? DefaultNamespace { get; }

        public List<XAttribute>? NamespaceDeclarations { get; }

        public Fb2NodeMetadata(
            XNamespace? defaultNamespace = null,
            IEnumerable<XAttribute>? namespaceDeclarations = null)
        {
            DefaultNamespace = defaultNamespace;

            var namespaceDeclarationsArr = namespaceDeclarations?.ToArray();

            if (namespaceDeclarationsArr != null && namespaceDeclarationsArr.Length != 0)
            {
                var namespaceDeclarationsOnly = namespaceDeclarations.All(attr => attr.IsNamespaceDeclaration);
                if (!namespaceDeclarationsOnly)
                    throw new ArgumentException($"{nameof(namespaceDeclarations)} should contain Namespace Declarations attributes only.");

                NamespaceDeclarations = new List<XAttribute>(namespaceDeclarationsArr.Length);
                NamespaceDeclarations.AddRange(namespaceDeclarationsArr);
            }
        }
    }
}

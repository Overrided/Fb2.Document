﻿using System;
using System.Xml;
using System.Xml.Linq;

namespace Fb2.Document.Extensions
{
    internal static class XNodeExtension
    {
        internal static void Validate(this XNode node, string supposedName)
        {
            if (string.IsNullOrWhiteSpace(supposedName))
                throw new ArgumentNullException(nameof(supposedName));

            if (node.NodeType == XmlNodeType.Element)
            {
                var element = node as XElement;

                if (!element.Name.LocalName.EqualsInvariant(supposedName))
                    throw new ArgumentException($"Invalid element, local name {element.Name.LocalName}, supposed name {supposedName}");
            }
        }
    }
}

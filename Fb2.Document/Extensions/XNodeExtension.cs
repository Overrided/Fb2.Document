using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Fb2.Document.Extensions
{
    public static class XNodeExtension
    {
        public static bool GetAllAttributesOrDefault(this XNode node, out Dictionary<string, string> result)
        {
            var element = node as XElement;

            if (element == null || !element.Attributes().Any())
            {
                result = null;
                return false;
            }

            result = element.Attributes()
                            .ToDictionary(attr => attr.Name.LocalName, attr => attr.Value);
            return true;
        }

        public static void Validate(this XNode node, string supposedName)
        {
            if (string.IsNullOrWhiteSpace(supposedName))
                throw new ArgumentNullException($"{nameof(supposedName)} is null or empty string");

            if (node.NodeType == XmlNodeType.Element)
            {
                var element = node as XElement;

                if (!element.Name.LocalName.EqualsInvariant(supposedName))
                    throw new ArgumentException($"Invalid element, local name {element.Name.LocalName}, supposed name {supposedName}");
            }
        }

        //public static string GetNodeContent(this XNode node)
        //{
        //    return node.NodeType switch
        //    {
        //        XmlNodeType.Element => ((XElement)node).Value,
        //        XmlNodeType.Text => ((XText)node).Value,
        //        _ => throw new Exception($"Unsupported nodeType: {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
        //    };

        //    //var rawContent = string.Empty;

        //    //if (node.NodeType == XmlNodeType.Element)
        //    //    rawContent = ((XElement)node).Value;
        //    //else if (node.NodeType == XmlNodeType.Text)
        //    //    rawContent = ((XText)node).Value;
        //    //else
        //    //    throw new ArgumentOutOfRangeException($"Unsupported nodeType: {node.NodeType}");

        //    //return rawContent;
        //}

        public static string GetNodeContent(this XNode node)
            => node.NodeType switch
            {
                XmlNodeType.Element => ((XElement)node).Value,
                XmlNodeType.Text => ((XText)node).Value,
                _ => throw new Exception($"Unsupported nodeType: {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
            };
    }
}

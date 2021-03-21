using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Represents text Node of Fb2Document
    /// Any class derived from Fb2Element can contain text only
    /// </summary>
    public abstract class Fb2Element : Fb2Node
    {
        private static readonly Regex trimWhitespace = new Regex(@"\s+", RegexOptions.Multiline);

        private static readonly HashSet<char> conditionalChars = new HashSet<char> { '\n', '\r', '\t' };

        /// <summary>
        /// Content (value) of element. Available after Load() method call
        /// </summary>
        public string Content { get; protected set; }

        /// <summary>
        /// For text nodes Inline is true by default, however, some classes override this property
        /// Indicates if content of an element should be written from a new line
        /// </summary>
        public override bool IsInline { get; protected set; } = true;

        /// <summary>
        /// Text node loading mechanism - formatting text and removal of unwanted characters
        /// Note: original content of XNode is NOT preserved by default except for <seealso cref="Code" />
        /// </summary>
        /// <param name="node">Node to load as Fb2Element</param>
        /// <param name="preserveWhitespace">Indicates if whitespace chars (\t, \n, \r) should be preserved. By default `false`.</param>
        public override void Load([In] XNode node, bool preserveWhitespace = false)
        {
            base.Load(node, preserveWhitespace);

            var rawContent = GetNodeContent(node);

            if (!preserveWhitespace && rawContent.Any(rch => conditionalChars.Contains(rch)))
                Content = trimWhitespace.Replace(rawContent, " ");
            else
                Content = rawContent;
        }

        /// <summary>
        /// Converts Fb2Element to XElement with regards to all attributes
        /// Note: only formatted content is serialized. 
        /// Original symbols from string value of XNode passed to Load method can be replaced and/or removed
        /// </summary>
        /// <returns>XElement reflected from given Fb2Element</returns>
        public override XElement ToXml()
        {
            var element = base.ToXml();
            if (!string.IsNullOrWhiteSpace(Content))
                element.Value = Content;

            return element;
        }

        public override string ToString() => Content;

        private static string GetNodeContent([In] XNode node)
            => node.NodeType switch
            {
                XmlNodeType.Element => ((XElement)node).Value,
                XmlNodeType.Text => ((XText)node).Value,
                _ => throw new Exception($"Unsupported nodeType: {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
            };
    }
}

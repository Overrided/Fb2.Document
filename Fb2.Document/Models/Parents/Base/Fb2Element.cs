using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Represents text Node of Fb2Document
    /// Any class derived from Fb2Element can contain text only
    /// </summary>
    public abstract class Fb2Element : Fb2Node
    {
        const string Whitespace = " ";

        protected string content = string.Empty;

        /// <summary>
        /// Content (value) of element. Available after Load() method call
        /// </summary>
        public string Content() => string.Copy(content);

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
                content = trimWhitespace.Replace(rawContent, Whitespace);
            else
                content = rawContent;
        }

        public Fb2Element WithContent(Func<string> contentProvider,
            string separator = null,
            bool preserveWhitespace = false)
        {
            if (contentProvider == null)
                throw new ArgumentNullException($"{nameof(contentProvider)} is null.");

            var content = contentProvider();

            return WithContent(content, separator, preserveWhitespace);
        }

        public Fb2Element WithContent(string newContent,
            string separator = null,
            bool preserveWhitespace = false)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new ArgumentNullException($"{nameof(newContent)} is null or empty string.");

            if (Name == ElementNames.EmptyLine)
                return this; // no content injections in empty line )

            // todo: move to separate method "NormalizeString" or so?
            if (string.IsNullOrWhiteSpace(separator))
                separator = string.Empty;
            else
            {
                separator = SecurityElement.Escape(separator);
                separator = trimWhitespace.Replace(separator, Whitespace);
            }

            // preventing xml injections, hopefully
            var escapedContent = SecurityElement.Escape(newContent);

            if (!preserveWhitespace && escapedContent.Any(c => conditionalChars.Contains(c)))
                escapedContent = trimWhitespace.Replace(escapedContent, Whitespace);

            if (string.IsNullOrWhiteSpace(content))
                content = escapedContent;
            else
                content = string.Join(separator, content, escapedContent);

            return this;
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
            if (!string.IsNullOrWhiteSpace(content))
                element.Value = content;

            return element;
        }

        // TODO : double check this, backward-compatibility?
        public override string ToString() => string.Copy(content);

        private static string GetNodeContent([In] XNode node)
            => node.NodeType switch
            {
                XmlNodeType.Element => ((XElement)node).Value,
                XmlNodeType.Text => ((XText)node).Value,
                _ => throw new Exception($"Unsupported nodeType: {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
            };

        // TODO : double check implementation validity
        public override bool Equals(object obj)
        {
            return obj is Fb2Element element &&
                   base.Equals(obj) &&
                   content.Equals(element.content, StringComparison.InvariantCulture);
        }

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), content);

        public override object Clone()
        {
            var element = base.Clone() as Fb2Element;
            element.content = Content();

            return element;
        }
    }
}

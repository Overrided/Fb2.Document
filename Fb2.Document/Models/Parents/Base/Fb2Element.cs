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
                content = trimWhitespace.Replace(rawContent, " ");
            else
                content = rawContent;
        }

        //public virtual Fb2Element WithContent(Func<string> contentProvider, bool preserveWhitespace = false)
        public Fb2Element WithContent(Func<string> contentProvider, bool preserveWhitespace = false)
        {
            if (contentProvider == null)
                throw new ArgumentNullException($"{nameof(contentProvider)} is null.");

            var content = contentProvider();

            return WithContent(content, preserveWhitespace);
        }

        // TODO : add separator params?
        //public virtual Fb2Element WithContent(string content, bool preserveWhitespace = false)
        public Fb2Element WithContent(string content, bool preserveWhitespace = false)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException($"{nameof(content)} is null or empty string.");

            if (Name == ElementNames.EmptyLine)
                return this; // no content injections in empty line )

            // preventing xml injections, hopefully
            var escapedContent = SecurityElement.Escape(content);

            if (!preserveWhitespace && escapedContent.Any(c => conditionalChars.Contains(c)))
                escapedContent = trimWhitespace.Replace(escapedContent, " ");

            if (string.IsNullOrWhiteSpace(content))
                content = escapedContent;
            else
                // TODO: not sure it should be `string.Empty` as separator
                content = string.Join(string.Empty, content, content);

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

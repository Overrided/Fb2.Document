using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;

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
        /// Content (value) of element. Available after Load() method call.
        /// </summary>
        public string Content => content;

        /// <summary>
        /// For text nodes Inline is true by default, however, some classes override this property.
        /// Indicates if content of an element should be written from a new line.
        /// </summary>
        public override bool IsInline { get; protected set; } = true;

        public override bool IsEmpty => string.IsNullOrEmpty(content);

        /// <summary>
        /// Text node loading mechanism - formatting text and removal of unwanted characters.
        /// </summary>
        /// <param name="node">Node to load as Fb2Element.</param>
        /// <param name="preserveWhitespace">Indicates if whitespace chars (\t, \n, \r) should be preserved. By default <see langword="false"/>.</param>
        /// <param name="loadUnsafe"> Is ignored by Fb2Element loading.</param>
        /// <exception cref="Fb2NodeLoadingException"></exception>
        /// <remarks>Original content of XNode is NOT preserved by default except for <seealso cref="Code" />.</remarks>
        public override void Load(
            [In] XNode node,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            base.Load(node, preserveWhitespace);

            var rawContent = node.NodeType switch
            {
                XmlNodeType.Element => ((XElement)node).Value,
                XmlNodeType.Text => ((XText)node).Value,
                _ => throw new Fb2NodeLoadingException($"Unsupported nodeType: received {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
            };

            if (!preserveWhitespace && trimWhitespace.IsMatch(rawContent))
                content = trimWhitespace.Replace(rawContent, Whitespace);
            else
                content = rawContent;
        }

        /// <summary>
        /// Appends new plain text to <see cref="Content"/> using provider function.
        /// </summary>
        /// <param name="contentProvider">Content provider function.</param>
        /// <param name="separator">Separator to split text from rest of the content.</param>
        /// <returns>Current element.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Element AddContent(Func<string> contentProvider, string? separator = null)
        {
            if (contentProvider == null)
                throw new ArgumentNullException(nameof(contentProvider));

            var content = contentProvider();

            return AddContent(content, separator);
        }

        /// <summary>
        /// Appends new plain text to <see cref="Content"/>.
        /// </summary>
        /// <param name="newContent">Plain text to append.</param>
        /// <param name="separator">Separator string used to join new text with existing content.</param>
        /// <returns>Current element.</returns>
        /// <remarks>
        /// <para>If <paramref name="separator"/> contains <see cref="Environment.NewLine"/> - it will be replaced with " " (whitespace).</para>
        /// <para>To insert new line use <see cref="EmptyLine"/> Fb2Element instead.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual Fb2Element AddContent(string newContent, string? separator = null)
        {
            if (string.IsNullOrEmpty(newContent))
                throw new ArgumentNullException(nameof(newContent));

            separator = string.IsNullOrEmpty(separator) ?
                string.Empty :
                SecurityElement.Escape(separator.Replace(Environment.NewLine, Whitespace));

            newContent = newContent.Replace(Environment.NewLine, Whitespace);
            newContent = SecurityElement.Escape(newContent);

            content = string.Join(separator, content, newContent);

            return this;
        }

        /// <summary>
        /// Clears <see cref="Content"/>.
        /// </summary>
        /// <returns>Current element.</returns>
        public virtual Fb2Element ClearContent()
        {
            if (!IsEmpty)
                content = string.Empty;

            return this;
        }

        /// <summary>
        /// Converts Fb2Element to XElement with regards to all attributes.
        /// </summary>
        /// <returns>XElement reflected from given Fb2Element.</returns>
        /// <remarks>
        /// <para>Only formatted content is serialized.</para>
        /// <para>Original symbols from string value of XNode passed to Load method can be replaced and/or removed during <see cref="Fb2Element.Load(XNode, bool, bool)"/></para>
        /// </remarks>
        public override XElement ToXml()
        {
            var element = base.ToXml();
            if (!IsEmpty)
                element.Value = content;

            return element;
        }

        public override string ToString() => Content;

        public override bool Equals(object? other)
        {
            if (other == null)
                return false;

            if (!(other is Fb2Element otherElement))
                return false;

            if (!base.Equals(otherElement))
                return false;

            var result = content.Equals(otherElement.content, StringComparison.InvariantCulture);

            return result;
        }

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), content);

        public override object Clone()
        {
            var element = base.Clone() as Fb2Element;
            element.content = Content;

            return element;
        }
    }
}

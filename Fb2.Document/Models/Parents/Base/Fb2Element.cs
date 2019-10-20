using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Fb2.Document.Extensions;

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
        /// Note: original content of XNode is NOT preserved
        /// </summary>
        /// <param name="node">Node to load as Fb2Element</param>
        public override void Load(XNode node, bool preserveWhitespace = false)
        {
            base.Load(node, preserveWhitespace);

            var rawContent = node.GetNodeContent();

            if (!preserveWhitespace && rawContent.Any(rch => conditionalChars.Contains(rch)))
                this.Content = trimWhitespace.Replace(rawContent, " ");
            else
                this.Content = rawContent;
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
    }
}

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    // TODO : add override for string content - at least check for base64 validity?
    public class Image : Fb2Element
    {
        public override string Name => ElementNames.Image;

        public override bool IsInline { get; protected set; } = true;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(
                AttributeNames.Id,
                AttributeNames.Alt,
                AttributeNames.Title,
                AttributeNames.XHref,
                AttributeNames.Type);

        private readonly HashSet<string> InlineParentNodes = new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.StanzaV,
            ElementNames.SubTitle,
            ElementNames.TableHeader,
            ElementNames.TableCell,
            ElementNames.TextAuthor
        };

        private readonly HashSet<string> NotInlineParentNodes = new HashSet<string>
        {
            ElementNames.BookBody,
            ElementNames.BookBodySection,
            ElementNames.Coverpage
        };

        public override string ToString()
        {
            var formattedAttributeString = TryGetAttribute(AttributeNames.XHref, true, out var result) ? $" {result.Value}" : string.Empty;

            return $"{Name}{formattedAttributeString}";
        }

        public override void Load([In] XNode node, bool preserveWhitespace = false)
        {
            IsInline = GetInline(node?.Parent?.Name?.LocalName, node.NodeType);

            base.Load(node, preserveWhitespace);
        }

        private bool GetInline(string parentNodeName, XmlNodeType parentNodeType)
        {
            if (string.IsNullOrEmpty(parentNodeName) || parentNodeType != XmlNodeType.Element)
                return true;

            if (InlineParentNodes.Contains(parentNodeName))
                return true;
            else if (NotInlineParentNodes.Contains(parentNodeName))
                return false;

            return true;
        }
    }
}

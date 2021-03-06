using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Image : Fb2Element
    {
        public override string Name => ElementNames.Image;

        public override bool IsInline { get; protected set; } = true;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id,
            AttributeNames.Alt,
            AttributeNames.Title,
            AttributeNames.XHref,
            AttributeNames.Type
        };

        private HashSet<string> InlineParentNodes => new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.StanzaV,
            ElementNames.SubTitle,
            ElementNames.TableHeader,
            ElementNames.TableCell,
            ElementNames.TextAuthor
        };

        private HashSet<string> NotInlineParentNodes => new HashSet<string>
        {
            ElementNames.BookBody,
            ElementNames.BookBodySection,
            ElementNames.Coverpage
        };

        public override string ToString()
        {
            var attributeOrDefault =
                Attributes != null &&
                Attributes.Any() &&
                Attributes.ContainsKey(AttributeNames.XHref) ? Attributes[AttributeNames.XHref] : string.Empty;

            var formattedAttributeString = string.IsNullOrWhiteSpace(attributeOrDefault) ? string.Empty : " " + attributeOrDefault;

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

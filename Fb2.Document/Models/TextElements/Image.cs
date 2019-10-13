using System.Collections.Generic;
using System.Linq;
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
            ElementNames.TextAutor
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
                Attributes.ContainsKey(AttributeNames.XHref) ?
                    this.Attributes[AttributeNames.XHref] :
                    string.Empty;

            var formattedAttributeString = string.IsNullOrWhiteSpace(attributeOrDefault) ? string.Empty : " " + attributeOrDefault;

            return $"{this.Name}{formattedAttributeString}";
        }

        public override void Load(XNode node, bool trimWhitespaces = true)
        {
            this.IsInline = GetInline(node?.Parent?.Name?.LocalName, node.NodeType);

            base.Load(node, trimWhitespaces);
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

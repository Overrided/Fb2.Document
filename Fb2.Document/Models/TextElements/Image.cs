using System.Collections.Generic;
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
            var attributeOrDefault = Attributes.ContainsKey(AttributeNames.XHref) ?
                this.Attributes[AttributeNames.XHref] :
                string.Empty;

            return $"Image {attributeOrDefault}";
        }

        public override void Load(XNode node)
        {
            this.IsInline = GetInline(node?.Parent?.Name?.LocalName, node.NodeType);

            base.Load(node);
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

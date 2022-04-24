using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class BinaryImage : Fb2Element
    {
        public override string Name => ElementNames.BinaryImage;

        public override bool IsInline => false;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(AttributeNames.ContentType, AttributeNames.Id);

        public override void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = false,
            bool loadUnsafe = true,
            bool loadNamespaceMetadata = true)
        {
            base.Load(node, parentNode, false, loadUnsafe, loadNamespaceMetadata);

            if (trimWhitespace.IsMatch(content))
                content = trimWhitespace.Replace(content, string.Empty);
        }
    }
}

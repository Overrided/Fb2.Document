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

        public override void Load([In] XNode node, bool preserveWhitespace = false, bool loadUnsafe = true)
        {
            base.Load(node, false, loadUnsafe);

            if (trimWhitespace.IsMatch(content))
                content = trimWhitespace.Replace(content, string.Empty);
        }
    }
}

using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TextLink : TextContainerBase
    {
        public override string Name => ElementNames.TextLink;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(AttributeNames.XHref, AttributeNames.Type);
    }
}

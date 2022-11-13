using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class SequenceInfo : Fb2Element
    {
        public override string Name => ElementNames.Sequence;

        public override bool IsInline => false;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(AttributeNames.Name, AttributeNames.Number, AttributeNames.Language);

        public sealed override Fb2Element AddContent(string newContent, string? separator = null) => this;
    }
}

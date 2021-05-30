using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class MiddleName : Fb2Element
    {
        public override string Name => ElementNames.MiddleName;

        public override bool IsInline => false;

        public override ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet.Create(AttributeNames.Language);
    }
}

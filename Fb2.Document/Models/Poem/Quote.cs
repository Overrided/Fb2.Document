using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Quote : Fb2Container
    {
        public override string Name => ElementNames.Quote;

        public override bool CanContainText => false;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(AttributeNames.Id, AttributeNames.Language);

        public override ImmutableHashSet<string> AllowedElements =>
            ImmutableHashSet.Create(
                ElementNames.Paragraph,
                ElementNames.SubTitle,
                ElementNames.EmptyLine,
                ElementNames.Poem,
                ElementNames.Table,
                ElementNames.TextAuthor);
    }
}

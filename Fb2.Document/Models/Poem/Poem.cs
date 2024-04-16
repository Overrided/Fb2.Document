using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Poem : Fb2Container
{
    public override string Name => ElementNames.Poem;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes =>
        ImmutableHashSet.Create(AttributeNames.Id, AttributeNames.Language);

    public override ImmutableHashSet<string> AllowedElements =>
        ImmutableHashSet.Create(
            ElementNames.Title,
            ElementNames.Epigraph,
            ElementNames.Stanza,
            ElementNames.TextAuthor,
            ElementNames.Date);
}

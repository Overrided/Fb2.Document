using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Poem : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Id, AttributeNames.Language];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Title,
        ElementNames.Epigraph,
        ElementNames.Stanza,
        ElementNames.TextAuthor,
        ElementNames.Date
    ];

    public override string Name => ElementNames.Poem;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

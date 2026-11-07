using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Epigraph : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Id];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Paragraph,
        ElementNames.Poem,
        ElementNames.Quote,
        ElementNames.EmptyLine,
        ElementNames.TextAuthor
    ];

    public override string Name => ElementNames.Epigraph;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

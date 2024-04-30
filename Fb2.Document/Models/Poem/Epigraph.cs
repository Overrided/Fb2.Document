using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Epigraph : Fb2Container
{
    public override string Name => ElementNames.Epigraph;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.Id];

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.Paragraph,
        ElementNames.Poem,
        ElementNames.Quote,
        ElementNames.EmptyLine,
        ElementNames.TextAuthor
    ];
}

using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BookBody : Fb2Container
{
    public override string Name => ElementNames.BookBody;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.Name, AttributeNames.Language];

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.Image,
        ElementNames.Title,
        ElementNames.Epigraph,
        ElementNames.BookBodySection
    ];
}

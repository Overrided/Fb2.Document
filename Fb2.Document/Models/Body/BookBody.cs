using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BookBody : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [
        AttributeNames.Name,
        AttributeNames.Language
    ];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Image,
        ElementNames.Title,
        ElementNames.Epigraph,
        ElementNames.BookBodySection
    ];

    public override string Name => ElementNames.BookBody;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

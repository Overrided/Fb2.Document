using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class PublishInfo : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.BookName,
        ElementNames.Publisher,
        ElementNames.City,
        ElementNames.Year,
        ElementNames.ISBN,
        ElementNames.Sequence
    ];

    public override string Name => ElementNames.PublishInfo;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

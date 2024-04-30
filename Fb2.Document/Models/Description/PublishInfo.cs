using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class PublishInfo : Fb2Container
{
    public override string Name => ElementNames.PublishInfo;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.BookName,
        ElementNames.Publisher,
        ElementNames.City,
        ElementNames.Year,
        ElementNames.ISBN,
        ElementNames.Sequence
    ];
}

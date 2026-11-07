using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BookDescription : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.TitleInfo,
        ElementNames.SrcTitleInfo,
        ElementNames.DocumentInfo,
        ElementNames.PublishInfo,
        ElementNames.CustomInfo
    ];

    public override string Name => ElementNames.Description;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

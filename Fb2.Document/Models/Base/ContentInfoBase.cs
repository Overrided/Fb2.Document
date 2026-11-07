using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base;

public abstract class ContentInfoBase : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [
        AttributeNames.Id,
        AttributeNames.Language
    ];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Paragraph,
        ElementNames.Poem,
        ElementNames.Quote,
        ElementNames.SubTitle,
        ElementNames.EmptyLine,
        ElementNames.Table
    ];

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

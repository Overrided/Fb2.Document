using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base;

public abstract class TextContainerBase : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Strong,
        ElementNames.Emphasis,
        ElementNames.TextStyle,
        ElementNames.TextLink,
        ElementNames.Strikethrough,
        ElementNames.Subscript,
        ElementNames.Superscript,
        ElementNames.Code,
        ElementNames.Image
    ];

    public override bool IsInline => true;

    public override bool CanContainText => true;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

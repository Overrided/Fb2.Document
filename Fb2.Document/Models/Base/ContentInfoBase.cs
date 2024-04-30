using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base;

public abstract class ContentInfoBase : Fb2Container
{
    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.Id, AttributeNames.Language];

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.Paragraph,
        ElementNames.Poem,
        ElementNames.Quote,
        ElementNames.SubTitle,
        ElementNames.EmptyLine,
        ElementNames.Table
    ];
}

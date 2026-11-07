using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Title : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Language];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Paragraph,
        ElementNames.EmptyLine
    ];

    public override string Name => ElementNames.Title;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Quote : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Id, AttributeNames.Language];

    private static readonly ImmutableHashSet<string> allowedElements = [
        ElementNames.Paragraph,
        ElementNames.SubTitle,
        ElementNames.EmptyLine,
        ElementNames.Poem,
        ElementNames.Table,
        ElementNames.TextAuthor
    ];

    public override string Name => ElementNames.Quote;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}
using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BodySection : Fb2Container
{
    public override string Name => ElementNames.BookBodySection;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.Id, AttributeNames.Language];

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.Title,
        ElementNames.Epigraph,
        ElementNames.Image,
        ElementNames.BookBodySection,
        ElementNames.Paragraph,
        ElementNames.Poem,
        ElementNames.Quote,
        ElementNames.EmptyLine,
        ElementNames.SubTitle,
        ElementNames.Table,
        ElementNames.Annotation
    ];
}

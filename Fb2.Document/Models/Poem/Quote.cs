using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Quote : Fb2Container
{
    public override string Name => ElementNames.Quote;

    public override bool CanContainText => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Id, AttributeNames.Language];

    public override FrozenSet<string> AllowedElements =>
    [
        ElementNames.Paragraph,
        ElementNames.SubTitle,
        ElementNames.EmptyLine,
        ElementNames.Poem,
        ElementNames.Table,
        ElementNames.TextAuthor
    ];
}

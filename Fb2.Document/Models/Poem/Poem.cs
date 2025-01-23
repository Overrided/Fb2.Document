using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Poem : Fb2Container
{
    public override string Name => ElementNames.Poem;

    public override bool CanContainText => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Id, AttributeNames.Language];

    public override FrozenSet<string> AllowedElements =>
    [
        ElementNames.Title,
        ElementNames.Epigraph,
        ElementNames.Stanza,
        ElementNames.TextAuthor,
        ElementNames.Date
    ];
}

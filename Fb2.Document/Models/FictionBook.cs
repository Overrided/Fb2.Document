using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class FictionBook : Fb2Container
{
    public override string Name => ElementNames.FictionBook;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.BinaryImage,
        ElementNames.BookBody,
        ElementNames.Description,
        ElementNames.Stylesheet
    ];
}

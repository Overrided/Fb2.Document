using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base;

public abstract class TitleInfoBase : Fb2Container
{
    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements =>
    [
        ElementNames.Genre,
        ElementNames.Author,
        ElementNames.BookTitle,
        ElementNames.Annotation,
        ElementNames.Keywords,
        ElementNames.Coverpage,
        ElementNames.Translator,
        ElementNames.Sequence,
        ElementNames.SubTitle,
        ElementNames.Date,
        ElementNames.Lang,
        ElementNames.SrcLang
    ];
}

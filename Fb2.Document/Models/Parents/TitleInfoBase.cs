using System.Collections.Generic;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    public abstract class TitleInfoBase : Fb2Container
    {
        public override bool CanContainText => false;

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
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
        };
    }
}

using System.Collections.Generic;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    public abstract class ContentInfoBase : Fb2Container
    {
        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id,
            AttributeNames.Language
        };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.Poem,
            ElementNames.Quote,
            ElementNames.SubTitle,
            ElementNames.EmptyLine,
            ElementNames.Table
        };
    }
}

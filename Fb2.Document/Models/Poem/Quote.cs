using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Quote : Fb2Container
    {
        public override string Name => ElementNames.Quote;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id,
            AttributeNames.Language
        };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.SubTitle,
            ElementNames.EmptyLine,
            ElementNames.Poem,
            ElementNames.Table,
            ElementNames.TextAuthor
        };
    }
}

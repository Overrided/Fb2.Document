using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TextAutor : TextContainer
    {
        public override string Name => ElementNames.TextAuthor;

        public override HashSet<string> AllowedAttributes => new HashSet<string> { AttributeNames.Id };

        public override bool IsInline => false;
    }
}

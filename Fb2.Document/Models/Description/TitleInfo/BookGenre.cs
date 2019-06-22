using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class BookGenre : Fb2Element
    {
        public override string Name => ElementNames.Genre;

        public override HashSet<string> AllowedAttributes => new HashSet<string> { AttributeNames.Match };

        public override bool IsInline => false;
    }
}

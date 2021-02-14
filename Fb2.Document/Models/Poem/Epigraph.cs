using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Epigraph : Fb2Container
    {
        public override string Name => ElementNames.Epigraph;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string> { AttributeNames.Id };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.Poem,
            ElementNames.Quote,
            ElementNames.EmptyLine,
            ElementNames.TextAuthor
        };
    }
}

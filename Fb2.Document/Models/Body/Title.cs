using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Title : Fb2Container
    {
        public override string Name => ElementNames.Title;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Language
        };

        public override bool CanContainText => false;

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Paragraph,
            ElementNames.EmptyLine
        };
    }
}

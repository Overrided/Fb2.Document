using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class BookBody : Fb2Container
    {
        public override string Name => ElementNames.BookBody;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Name,
            AttributeNames.Language
        };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Image,
            ElementNames.Title,
            ElementNames.Epigraph,
            ElementNames.BookBodySection
        };
    }
}

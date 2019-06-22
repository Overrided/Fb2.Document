using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class BodySection : Fb2Container
    {
        public override string Name => ElementNames.BookBodySection;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id,
            AttributeNames.Language
        };

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Title,
            ElementNames.Epigraph,
            ElementNames.Image,
            ElementNames.BookBodySection,
            ElementNames.Paragraph,
            ElementNames.Poem,
            ElementNames.Quote,
            ElementNames.EmptyLine,
            ElementNames.SubTitle,
            ElementNames.Table,
            ElementNames.Annotation
        };
    }
}

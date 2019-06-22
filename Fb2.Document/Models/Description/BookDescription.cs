using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class BookDescription : Fb2Container
    {
        public override string Name => ElementNames.Description;

        public override bool CanContainText => false;

        public override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.TitleInfo,
            ElementNames.SrcTitleInfo,
            ElementNames.DocumentInfo,
            ElementNames.PublishInfo,
            ElementNames.CustomInfo
        };
    }
}

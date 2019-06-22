using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Publisher : CreatorBase
    {
        public override string Name => ElementNames.Publisher;

        public override HashSet<string> AllowedAttributes => new HashSet<string> { AttributeNames.Language };

        public override bool CanContainText => true;
    }
}

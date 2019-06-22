using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class SubTitle : TextContainer
    {
        public override string Name => ElementNames.SubTitle;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Id,
            AttributeNames.Language
        };

        public override bool IsInline => false;
    }
}

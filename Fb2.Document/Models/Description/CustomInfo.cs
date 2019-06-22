using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class CustomInfo : Fb2Element
    {
        public override string Name => ElementNames.CustomInfo;

        public override bool IsInline => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string>()
        {
            AttributeNames.InfoType
        };
    }
}

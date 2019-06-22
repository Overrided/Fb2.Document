using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class City : Fb2Element
    {
        public override string Name => ElementNames.City;

        public override bool IsInline => false;

        public override HashSet<string> AllowedAttributes => new HashSet<string> { AttributeNames.Language };
    }
}

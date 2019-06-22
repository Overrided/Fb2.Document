using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TextLink : TextContainer
    {
        public override string Name => ElementNames.TextLink;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.XHref,
            AttributeNames.Type
        };
    }
}

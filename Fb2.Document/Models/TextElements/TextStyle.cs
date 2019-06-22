using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class TextStyle : TextContainer
    {
        public override string Name => ElementNames.TextStyle;

        public override HashSet<string> AllowedAttributes => new HashSet<string>
        {
            AttributeNames.Name,
            AttributeNames.Language
        };
    }
}

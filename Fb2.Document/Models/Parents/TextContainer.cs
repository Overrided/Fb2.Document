using System.Collections.Generic;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    public abstract class TextContainer : Fb2Container
    {
        public override bool IsInline => true;

        public override bool CanContainText => true;

        public sealed override HashSet<string> AllowedElements => new HashSet<string>
        {
            ElementNames.Strong,
            ElementNames.Emphasis,
            ElementNames.TextStyle,
            ElementNames.TextLink,
            ElementNames.Strikethrough,
            ElementNames.Subscript,
            ElementNames.Superscript,
            ElementNames.Code,
            ElementNames.Image
        };
    }
}

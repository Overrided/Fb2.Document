using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class SrcLang : Fb2Element
    {
        public override string Name => ElementNames.SrcLang;

        public override bool IsInline => false;
    }
}

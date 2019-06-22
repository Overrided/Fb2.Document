using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Year : Fb2Element
    {
        public override string Name => ElementNames.Year;

        public override bool IsInline => false;
    }
}

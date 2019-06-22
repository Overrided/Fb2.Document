using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class ProgramUsed : Fb2Element
    {
        public override string Name => ElementNames.ProgramUsed;

        public override bool IsInline => false;
    }
}

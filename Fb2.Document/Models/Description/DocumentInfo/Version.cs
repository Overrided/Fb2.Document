using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Version : Fb2Element
    {
        public override string Name => ElementNames.Version;

        public override bool IsInline => false;
    }
}

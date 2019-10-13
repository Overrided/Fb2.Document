using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Code : TextContainer
    {
        public override string Name => ElementNames.Code;

        public override void Load(XNode node, bool trimWhitespaces = true)
        {
            base.Load(node, false);
        }
    }
}

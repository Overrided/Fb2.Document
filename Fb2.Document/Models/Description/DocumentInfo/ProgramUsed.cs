using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class ProgramUsed : Fb2Element
    {
        public override string Name => ElementNames.ProgramUsed;

        public override bool IsInline => false;

        public override void Load(
            [In] XNode node,
            bool preserveWhitespace = false,
            bool loadUnsafe = true) => base.Load(node, true, loadUnsafe);
    }
}

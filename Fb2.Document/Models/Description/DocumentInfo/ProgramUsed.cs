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

        /// <summary>
        /// Specific override to preserve original string content 
        /// with '\t', ' ', '\r\n' etc. formatting.
        /// </summary>
        public override void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = false,
            bool loadUnsafe = true) => base.Load(node, parentNode, true, loadUnsafe);
    }
}

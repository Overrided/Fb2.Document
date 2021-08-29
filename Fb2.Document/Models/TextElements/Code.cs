using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    // TODO : add comments on how parameters get ignored on Load call
    public class Code : TextContainerBase
    {
        public override string Name => ElementNames.Code;

        /// <summary>
        /// Specific override to preserve original string content 
        /// with all formatting done with '\t', ' ', '\r\n' etc.
        /// </summary>
        public override void Load(
            [In] XNode node,
            bool preserveWhitespace = true,
            bool loadUnsafe = true) => base.Load(node, true, loadUnsafe);
    }
}

using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Code : TextContainerBase
    {
        public override string Name => ElementNames.Code;

        /// <summary>
        /// Specific override to preserve original string content 
        /// with all formatting done with '\t', ' ', '\r\n' etc.
        /// </summary>
        public sealed override void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = true,
            bool loadUnsafe = true,
            bool loadNamespaceMetadata = true) => base.Load(node, parentNode, true, loadUnsafe, loadNamespaceMetadata);
    }
}

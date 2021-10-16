using Fb2.Document.Models.Base;

namespace Fb2.Document.Tests.Common
{
    // just for what? ...you've got it, lulz!)
    public class ImpostorNode : Fb2Node
    {
        private string nodeName = "Impostor";

        public ImpostorNode(string? nodeName = null)
        {
            if (!string.IsNullOrEmpty(nodeName))
                this.nodeName = nodeName;
        }

        public override string Name => nodeName;

        public override bool IsInline
        {
            get;
            protected set;
        }

        public override bool IsEmpty => false;
    }
}

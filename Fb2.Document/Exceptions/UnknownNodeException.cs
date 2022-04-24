using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Exceptions
{
    public class UnknownNodeException : Exception
    {
        public string NodeName { get; }

        public UnknownNodeException(string nodeName) : base($"'{nodeName}' is not known Fb2 node name.")
        {
            NodeName = nodeName;
        }

        public UnknownNodeException(Fb2Node node) : this(node.Name) { }
    }
}

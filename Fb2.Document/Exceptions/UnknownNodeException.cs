using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Exceptions
{
    public class UnknownNodeException : Exception
    {
        public UnknownNodeException(string nodeName) : base($"'{nodeName}' is not known Fb2 node name.") { }

        public UnknownNodeException(Fb2Node node) :
            base($"'{node.Name}' with type '{node.GetType().Name}' is not known Fb2 node.")
        { }
    }
}

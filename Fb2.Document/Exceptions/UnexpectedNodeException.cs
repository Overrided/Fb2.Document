using System;

namespace Fb2.Document.Exceptions
{
    public class UnexpectedNodeException : Exception
    {
        public string ParentNodeName { get; }

        public string ChildNodeName { get; }

        public UnexpectedNodeException(string parentNodeName, string childNodeName) :
            base($"Node '{parentNodeName}' can not contain '{childNodeName}'.")
        {
            ParentNodeName = parentNodeName;
            ChildNodeName = childNodeName;
        }
    }
}

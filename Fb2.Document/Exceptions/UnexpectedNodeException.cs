using System;

namespace Fb2.Document.Exceptions
{
    public class UnexpectedNodeException : Exception
    {
        public UnexpectedNodeException(string parentNodeName, string childNodeName) :
            base($"Node '{parentNodeName}' can not contain '{childNodeName}'.")
        { }
    }
}

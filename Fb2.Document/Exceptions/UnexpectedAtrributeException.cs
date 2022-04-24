using System;

namespace Fb2.Document.Exceptions
{
    public class UnexpectedAtrributeException : Exception
    {
        public string NodeName { get; }

        public string AttributeName { get; }

        public UnexpectedAtrributeException(string nodeName, string attributeName) :
            base($"Node '{nodeName}' can not have '{attributeName}' attribute.")
        {
            NodeName = nodeName;
            AttributeName = attributeName;
        }
    }
}

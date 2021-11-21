using System;

namespace Fb2.Document.Exceptions
{
    public class UnexpectedAtrributeException : Exception
    {
        public UnexpectedAtrributeException(string nodeName, string attributeName) :
            base($"Node '{nodeName}' can not have '{attributeName}' attribute.")
        { }
    }
}

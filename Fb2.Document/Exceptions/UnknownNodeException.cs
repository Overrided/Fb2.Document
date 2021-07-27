using System;

namespace Fb2.Document.Exceptions
{
    public class UnknownNodeException : Exception
    {
        public UnknownNodeException(string nodeName) : base($"'{nodeName}' is not valid Fb2 node.")
        { }
    }
}

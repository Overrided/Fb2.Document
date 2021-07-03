using System;

namespace Fb2.Document.Exceptions
{
    public class UnknownNodeNameException : Exception
    {
        public UnknownNodeNameException(string nodeName) : base($"'{nodeName}' is not valid Fb2 node name.")
        { }
    }
}

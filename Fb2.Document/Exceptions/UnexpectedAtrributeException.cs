using System;

namespace Fb2.Document.Exceptions
{
    public class UnexpectedAtrributeException : Exception
    {
        public UnexpectedAtrributeException(string message) : base(message) { }
    }
}

using System;

namespace Fb2.Document.Exceptions
{
    public class UnknownNodeNameException : Exception
    {
        public UnknownNodeNameException(string message) : base(message)
        {
        }
    }
}

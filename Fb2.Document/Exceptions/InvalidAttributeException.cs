using System;

namespace Fb2.Document.Exceptions
{
    public class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string message) : base(message) { }
    }
}

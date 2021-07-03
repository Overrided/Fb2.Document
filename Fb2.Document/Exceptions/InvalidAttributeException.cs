using System;

namespace Fb2.Document.Exceptions
{
    public class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string issueName) :
            base($"{issueName} is null or empty string, or contains invalid characters.")
        { }
    }
}

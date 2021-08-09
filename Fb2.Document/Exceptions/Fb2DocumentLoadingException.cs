using System;

namespace Fb2.Document.Exceptions
{
    public class Fb2DocumentLoadingException : Exception
    {
        public Fb2DocumentLoadingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

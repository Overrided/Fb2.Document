using System;

namespace Fb2.Document.Exceptions
{
    public class Fb2NodeLoadingException : Exception
    {
        public Fb2NodeLoadingException(string message) : base(message) { }
    }
}

using System;

namespace Fb2.Document.Exceptions
{
    public class NoAttributesAllowedException : Exception
    {
        public NoAttributesAllowedException(string message) : base(message) { }
    }
}

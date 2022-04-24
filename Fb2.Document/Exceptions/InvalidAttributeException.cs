using System;

namespace Fb2.Document.Exceptions
{
    public class InvalidAttributeException : Exception
    {
        public string AttributeKey { get; }

        public InvalidAttributeException(string attributeKey) :
            base($"AttributeKey '{attributeKey}' is null or empty string, or contains invalid characters.")
        {
            AttributeKey = attributeKey;
        }
    }
}

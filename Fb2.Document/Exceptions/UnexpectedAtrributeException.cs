using System;

namespace Fb2.Document.Exceptions
{
    /// <summary>
    /// Thrown when attempt to add Fb2Attribute that is not in Fb2Node's <seealso cref="Models.Base.Fb2Node.AllowedAttributes"/>.
    /// </summary>
    public class UnexpectedAtrributeException : Exception
    {
        /// <summary>
        /// The Name of the Fb2Node to which the attribute was attempted to be added.
        /// </summary>
        public string NodeName { get; }

        /// <summary>
        /// Name of attribute which was attempted to be added.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// Creates new instance of <seealso cref="UnexpectedAtrributeException"/>.
        /// </summary>
        /// <param name="nodeName">The Name of the Fb2Node to which the attribute was attempted to be added.</param>
        /// <param name="attributeName">Name of attribute which was attempted to be added.</param>
        public UnexpectedAtrributeException(string nodeName, string attributeName) :
            base($"Node '{nodeName}' can not have '{attributeName}' attribute.")
        {
            NodeName = nodeName;
            AttributeName = attributeName;
        }
    }
}

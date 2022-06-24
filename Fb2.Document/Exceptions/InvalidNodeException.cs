using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Exceptions
{
    /// <summary>
    /// Thrown on attempt to get or create Fb2Node using invalid node Name.
    /// </summary>
    public class InvalidNodeException : Exception
    {
        /// <summary>
        /// Invalid NodeName that caused exception to be thrown.
        /// </summary>
        public string NodeName { get; }

        public InvalidNodeException(string nodeName) : base($"'{nodeName}' is not known Fb2 node name.")
        {
            NodeName = nodeName;
        }

        public InvalidNodeException(Fb2Node node) : this(node.Name) { }
    }
}

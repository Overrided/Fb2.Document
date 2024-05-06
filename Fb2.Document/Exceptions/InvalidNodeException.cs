using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown on attempt to get or create <see cref="Fb2Node"/> using invalid node Name.
/// </summary>
public class InvalidNodeException : Exception
{
    /// <summary>
    /// Invalid NodeName that caused exception to be thrown.
    /// </summary>
    public string NodeName { get; }

    /// <summary>
    /// Creates new instance of <see cref="InvalidNodeException"/> using <paramref name="nodeName"/> parameter.
    /// </summary>
    /// <param name="nodeName"><see cref="Fb2Node.Name"/> that caused exception.</param>
    public InvalidNodeException(string nodeName) : base($"'{nodeName}' is not known Fb2 node name.")
    {
        NodeName = nodeName;
    }

    /// <summary>
    /// Creates new instance of <see cref="InvalidNodeException"/> using <paramref name="node"/> parameter.
    /// </summary>
    /// <param name="node"><see cref="Fb2Node"/> that caused exception.</param>
    public InvalidNodeException(Fb2Node node) : this(node.Name) { }
}

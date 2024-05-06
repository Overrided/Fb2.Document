using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown when attempt to add attribute(s) to <see cref="Models.Base.Fb2Node"/> without <see cref="Models.Base.Fb2Node.AllowedAttributes"/>.
/// </summary>
public class NoAttributesAllowedException : Exception
{
    /// <summary>
    /// The Name of the Fb2Node to which the attribute was attempted to be added.
    /// </summary>
    public string NodeName { get; }

    /// <summary>
    /// Creates new instance of <see cref="NoAttributesAllowedException"/>.
    /// </summary>
    /// <param name="nodeName">Name of Node that attribute(s) where attempted to be added to.</param>
    public NoAttributesAllowedException(string nodeName) : base($"Node '{nodeName}' has no allowed attributes.")
    {
        NodeName = nodeName;
    }
}

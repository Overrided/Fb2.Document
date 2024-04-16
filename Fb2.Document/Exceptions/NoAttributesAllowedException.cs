using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown when attempt to add attribute(s) no Fb2Node without <seealso cref="Models.Base.Fb2Node.AllowedAttributes"/>.
/// </summary>
public class NoAttributesAllowedException : Exception
{
    /// <summary>
    /// The Name of the Fb2Node to which the attribute was attempted to be added.
    /// </summary>
    public string NodeName { get; }

    public NoAttributesAllowedException(string nodeName) : base($"Node '{nodeName}' has no allowed attributes.")
    {
        NodeName = nodeName;
    }
}

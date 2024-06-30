using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown on attempt to add invalid Fb2Node to particular <see cref="Models.Base.Fb2Container.Content"/>.
/// </summary>
public class UnexpectedNodeException : Exception
{
    /// <summary>
    /// The Name of the Fb2Node which Content was attempted to be modified.
    /// </summary>
    public string ParentNodeName { get; }

    /// <summary>
    /// The Name of the Fb2Node which was attempted to be added.
    /// </summary>
    public string ChildNodeName { get; }

    /// <summary>
    /// Creates new instance of <see cref="UnexpectedNodeException"/>.
    /// </summary>
    /// <param name="parentNodeName">The Name of the Fb2Node to which the Node with name <paramref name="childNodeName"/> was attempted to be added.</param>
    /// <param name="childNodeName">The Name of the Fb2Node which was attempted to be added.</param>
    public UnexpectedNodeException(string parentNodeName, string childNodeName) :
        base($"Node '{parentNodeName}' can not contain '{childNodeName}'.")
    {
        ParentNodeName = parentNodeName;
        ChildNodeName = childNodeName;
    }
}

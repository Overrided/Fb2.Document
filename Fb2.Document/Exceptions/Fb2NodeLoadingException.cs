using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown if loading of a particular Fb2Node failed.
/// </summary>
public class Fb2NodeLoadingException : Exception
{
    public Fb2NodeLoadingException(string message) : base(message) { }
}

using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown if loading of a Fb2Document failed.
/// </summary>
public class Fb2DocumentLoadingException : Exception
{
    public Fb2DocumentLoadingException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

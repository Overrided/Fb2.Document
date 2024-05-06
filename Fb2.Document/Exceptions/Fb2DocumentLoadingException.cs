using System;

namespace Fb2.Document.Exceptions;

/// <summary>
/// Thrown if loading of a <see cref="Fb2Document"/> failed.
/// </summary>
public class Fb2DocumentLoadingException : Exception
{
    /// <summary>
    /// Creates new instance of <see cref="Fb2DocumentLoadingException"/>.
    /// </summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception. This parameter is optional.</param>
    public Fb2DocumentLoadingException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}

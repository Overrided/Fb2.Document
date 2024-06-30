namespace Fb2.Document.LoadingOptions;

/// <summary>
/// Specifies a set of options to use with <see cref="Fb2Document.Load(System.IO.Stream, Fb2StreamLoadingOptions?)"/> and <see cref="Fb2Document.LoadAsync(System.IO.Stream, Fb2StreamLoadingOptions?)"/>.
/// </summary>
public class Fb2StreamLoadingOptions : Fb2LoadingOptions
{
    /// <summary>
    /// Indicates if input stream should be closed after loading Fb2Document.
    /// </summary>
    public bool CloseInputStream { get; set; }

    /// <summary>
    /// Creates new instance of <see cref="Fb2StreamLoadingOptions"/>.
    /// </summary>
    /// <param name="loadUnsafeElements">Indicates if Unsafe elements should be loaded. Optional, <see langword="true"/> by default.</param>
    /// <param name="loadNamespaceMetadata">Indicates if <see cref="Models.Base.Fb2Node.NodeMetadata"/> should be loaded. Optional, <see langword="true"/> by default.</param>
    /// <param name="closeInputStream">Indicates if input stream should be closed after loading Fb2 document. Optional, <see langword="false"/> by default.</param>
    public Fb2StreamLoadingOptions(
        bool loadUnsafeElements = true,
        bool loadNamespaceMetadata = true,
        bool closeInputStream = false) : base(loadUnsafeElements, loadNamespaceMetadata)
    {
        CloseInputStream = closeInputStream;
    }
}

namespace Fb2.Document.LoadingOptions
{
    /// <summary>
    /// Specifies a set of option to use during <see cref="Fb2Document.Load(System.IO.Stream, Fb2StreamLoadingOptions?)"/> and <see cref="Fb2Document.LoadAsync(System.IO.Stream, Fb2StreamLoadingOptions?)"/>
    /// </summary>
    public class Fb2StreamLoadingOptions
    {
        /// <summary>
        /// Indicates if unsafe elements should be loaded.
        /// </summary>
        public bool LoadUnsafeElements { get; set; }

        /// <summary>
        /// Indicates if input stream should be closed after loading fb2 document.
        /// </summary>
        public bool CloseInputStream { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="Fb2StreamLoadingOptions"/>.
        /// </summary>
        /// <param name="loadUnsafeElements">Indicates if Unsafe elements should be loaded. Optional, <see langword="true"/> by default.</param>
        /// <param name="closeInputStream">Indicates if input stream should be closed after loading fb2 document. Optional, <see langword="false"/> by default.</param>
        public Fb2StreamLoadingOptions(bool loadUnsafeElements = true, bool closeInputStream = false)
        {
            LoadUnsafeElements = loadUnsafeElements;
            CloseInputStream = closeInputStream;
        }
    }
}

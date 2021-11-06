namespace Fb2.Document.LoadingOptions
{
    /// <summary>
    /// Specifies a set of option to use during <seealso cref="Fb2Document.Load(System.IO.Stream, Fb2StreamLoadingOptions?)"/> and <seealso cref="Fb2Document.LoadAsync(System.IO.Stream, Fb2StreamLoadingOptions?)"/>
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

        public Fb2StreamLoadingOptions(bool loadUnsafeElements = true, bool closeInputStream = false)
        {
            LoadUnsafeElements = loadUnsafeElements;
            CloseInputStream = closeInputStream;
        }
    }
}

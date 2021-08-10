namespace Fb2.Document.LoadingOptions
{
    public class Fb2StreamLoadingOptions
    {
        // indicates if unsafe elements should be loaded
        public bool LoadUnsafeElements { get; set; }

        // indicates if stream should be closed after loading fb2 document
        public bool CloseInputStream { get; set; }

        public Fb2StreamLoadingOptions(bool loadUnsafeElements = true, bool closeInputStream = false)
        {
            LoadUnsafeElements = loadUnsafeElements;
            CloseInputStream = closeInputStream;
        }
    }
}

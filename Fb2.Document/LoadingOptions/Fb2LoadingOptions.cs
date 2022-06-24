namespace Fb2.Document.LoadingOptions
{
    /// <summary>
    /// Specifies a set of option to use during 
    /// </summary>
    public class Fb2LoadingOptions
    {
        /// <summary>
        /// Indicates if unsafe elements should be loaded.
        /// </summary>
        public bool LoadUnsafeElements { get; set; }

        /// <summary>
        /// Indicates if <see cref="Models.Base.Fb2Node.NodeMetadata"/> should be loaded.
        /// </summary>
        public bool LoadNamespaceMetadata { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="Fb2LoadingOptions"/>.
        /// </summary>
        /// <param name="loadUnsafeElements">Indicates if Unsafe elements should be loaded. Optional, <see langword="true"/> by default.</param>
        /// <param name="loadNamespaceMetadata">Indicates if <see cref="Models.Base.Fb2Node.NodeMetadata"/> should be loaded. Optional, <see langword="true"/> by default.</param>
        public Fb2LoadingOptions(bool loadUnsafeElements = true, bool loadNamespaceMetadata = true)
        {
            LoadUnsafeElements = loadUnsafeElements;
            LoadNamespaceMetadata = loadNamespaceMetadata;
        }
    }
}

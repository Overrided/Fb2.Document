using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;
using Fb2.Document.LoadingOptions;
using Fb2.Document.Models;

namespace Fb2.Document
{
    /// <summary>
    /// Represents Fiction Book at file level.
    /// Provides API for loading, reading and serializing fb2 files to FictionBook model and vice versa.
    /// </summary>
    public sealed class Fb2Document
    {
        private const string defaultXmlVersion = "1.0";

        private BookDescription description = null;
        private TitleInfo title = null;
        private SrcTitleInfo sourceTitle = null;
        private DocumentInfo documentInfo = null;
        private PublishInfo publishInfo = null;
        private CustomInfo customInfo = null;
        private ImmutableList<BookBody> bodies = null;
        private ImmutableList<BinaryImage> binaryImages = null;

        private static readonly XDeclaration DefaultDeclaration = new XDeclaration(defaultXmlVersion, Encoding.UTF8.HeaderName, null);
        private static readonly XmlReaderSettings DefaultXmlReaderSettings = new XmlReaderSettings
        {
            Async = true,
            CheckCharacters = true,
            IgnoreWhitespace = true,
            ConformanceLevel = ConformanceLevel.Document
        };

        /// <summary>
        /// Represents <FictionBook> - root element of a file.
        /// </summary>
        public FictionBook Book { get; private set; }

        /// <summary>
        /// Represents Description element of a FictionBook
        /// </summary>
        public BookDescription BookDescription
        {
            get
            {
                if (!IsLoaded)
                    return null;

                if (description == null)
                    description = Book?.GetFirstChild<BookDescription>();

                return description;
            }
        }

        /// <summary>
        /// Shortcut property. Gets Title of Description from FictionBook
        /// </summary>
        public TitleInfo Title
        {
            get
            {
                if (title == null)
                    title = BookDescription?.GetFirstChild<TitleInfo>();

                return title;
            }
        }

        /// <summary>
        /// Shortcut property. Gets SrcTitle of Description from FictionBook
        /// Mainly exists if book is translated - and contains Title in original language
        /// </summary>
        public SrcTitleInfo SourceTitle
        {
            get
            {
                if (sourceTitle == null)
                    sourceTitle = BookDescription?.GetFirstChild<SrcTitleInfo>();

                return sourceTitle;
            }
        }

        /// <summary>
        /// Shortcut property. Gets DocumentInfo of Description from FictionBook
        /// </summary>
        public DocumentInfo DocumentInfo
        {
            get
            {
                if (documentInfo == null)
                    documentInfo = BookDescription?.GetFirstChild<DocumentInfo>();

                return documentInfo;
            }
        }

        /// <summary>
        /// Shortcut property. Gets PublishInfo of Description from FictionBook
        /// </summary>
        public PublishInfo PublishInfo
        {
            get
            {
                if (publishInfo == null)
                    publishInfo = BookDescription?.GetFirstChild<PublishInfo>();

                return publishInfo;
            }
        }

        /// <summary>
        /// Shortcut property. Gets CustomInfo of Description from FictionBook
        /// </summary>
        public CustomInfo CustomInfo
        {
            get
            {
                if (customInfo == null)
                    customInfo = BookDescription?.GetFirstChild<CustomInfo>();

                return customInfo;
            }
        }

        /// <summary>
        /// Shortcut property. Gets list of BookBody elements from FictionBook.
        /// </summary>
        public ImmutableList<BookBody> Bodies
        {
            get
            {
                if (!IsLoaded)
                    return null;

                if (bodies == null || !bodies.Any())
                    bodies = Book?.GetChildren<BookBody>()?.ToImmutableList();

                return bodies;
            }
        }

        /// <summary>
        /// Shortcut property. Gets list of BinaryImages elements from FictionBook.
        /// </summary>
        public ImmutableList<BinaryImage> BinaryImages
        {
            get
            {
                if (!IsLoaded)
                    return null;

                if (binaryImages == null || !binaryImages.Any())
                    binaryImages = Book?.GetChildren<BinaryImage>()?.ToImmutableList();

                return binaryImages;
            }
        }

        /// <summary>
        /// Indicates whether Fb2Document instance was loaded using Load or LoadAsync methods
        /// </summary>
        public bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Creates new, empty instance of Fb2Document with `IsLoaded` set to `true`.
        /// </summary>
        /// <param name="fictionBook">
        /// Optional parameter. Book to use with Fb2Document. If ommited, `Book` property of created document returns `null`.
        /// </param>
        /// <returns>New instance of Fb2Document.</returns>
        public static Fb2Document CreateDocument(FictionBook fictionBook = null)
        {
            var document = new Fb2Document();

            document.Book = fictionBook;
            document.IsLoaded = fictionBook != null;

            return document;
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from XDocument insance
        /// </summary>
        /// <param name="document">Content of a file read as xml</param>
        /// <param name="loadUnsafeElements">Idicates if "unsafe" elements should be loaded. `true` by default.</param>
        /// This method is not Encoding-safe 
        /// Loading will proceed with Encoding of XDocument received.
        public void Load([In] XDocument document, bool loadUnsafeElements = true)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            LoadHandled(() => Load(document.Root, loadUnsafeElements));
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from string
        /// </summary>
        /// <param name="fileContent">Content of a file read as string</param>
        /// <param name="loadUnsafeElements">Idicates if "unsafe" elements should be loaded. `true` by default.</param>
        /// This method is not Encoding-safe 
        /// Loading will proceed with Encoding of string received.
        public void Load([In] string fileContent, bool loadUnsafeElements = true)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentNullException(nameof(fileContent));

            LoadHandled(() =>
            {
                var document = XDocument.Parse(fileContent);
                Load(document.Root, loadUnsafeElements);
            });
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from string asynchronously
        /// </summary>
        /// <param name="fileContent">Content of a file read as string</param>
        /// <param name="loadUnsafeElements">Idicates if "unsafe" elements should be loaded. `true` by default.</param>
        /// this method exists mostly for lulz
        public async Task LoadAsync([In] string fileContent, bool loadUnsafeElements = true)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentNullException(nameof(fileContent));

            await LoadHandledAsync(async () =>
            {
                using (var reader = new StringReader(fileContent))
                {
                    var document = await XDocument
                        .LoadAsync(reader, LoadOptions.None, default)
                        .ConfigureAwait(false);

                    Load(document.Root, loadUnsafeElements);
                }
            });
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from stream.
        /// </summary>
        /// <param name="fileContent">Stream of file data, opened for read.</param>
        /// <param name="loadUnsafeElements">Idicates if "unsafe" elements should be loaded. `true` by default.</param>
        /// This method is Encoding-safe 
        /// Encoding for reading content will be determined during load process 
        /// or Encoding.Default will be used
        public void Load([In] Stream fileContent, Fb2StreamLoadingOptions loadingOptions = null)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            if (!fileContent.CanRead)
                throw new ArgumentException($"Can`t read file content : {nameof(fileContent)}.CanRead is {false}");

            var options = loadingOptions ?? new Fb2StreamLoadingOptions();
            var xmlReaderSetting = DefaultXmlReaderSettings.Clone();
            xmlReaderSetting.CloseInput = options.CloseInputStream;

            LoadHandled(() =>
            {
                using (var reader = XmlReader.Create(fileContent, xmlReaderSetting))
                {
                    var document = XDocument.Load(reader);
                    Load(document.Root, options.LoadUnsafeElements);
                }
            });
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from stream asynchronously.
        /// </summary>
        /// <param name="fileContent">Stream of file data, opened for read.</param>
        /// <param name="loadUnsafeElements">Idicates if "unsafe" elements should be loaded. `true` by default.</param>
        /// This method is Encoding-safe 
        /// Encoding for reading content will be determined during load process 
        /// or Encoding.Default will be used
        public async Task LoadAsync([In] Stream fileContent, Fb2StreamLoadingOptions loadingOptions = null)
        {
            if (fileContent == null)
                throw new ArgumentNullException(nameof(fileContent));

            if (!fileContent.CanRead)
                throw new ArgumentException($"Can`t read file content : {nameof(fileContent)}.CanRead is {false}");

            var options = loadingOptions ?? new Fb2StreamLoadingOptions();
            var xmlReaderSetting = DefaultXmlReaderSettings.Clone();
            xmlReaderSetting.CloseInput = options.CloseInputStream;

            await LoadHandledAsync(async () =>
            {
                using (var reader = XmlReader.Create(fileContent, xmlReaderSetting))
                {
                    var document = await XDocument
                        .LoadAsync(reader, LoadOptions.None, default)
                        .ConfigureAwait(false);

                    Load(document.Root, options.LoadUnsafeElements);
                }
            });
        }

        /// <summary>
        /// Generates XDocument using previously loaded FictionBook
        /// </summary>
        /// <returns>
        /// XDocument instance formatted accordingly to fb2 rules. 
        /// Returns null if <see cref="Book"/> is null or <see cref="IsLoaded"/> is <see langword="false"/>
        /// </returns>
        public XDocument ToXml()
        {
            if (Book == null || !IsLoaded)
                return null;

            var xmlRoot = Book.ToXml();
            var xmlDoc = new XDocument(DefaultDeclaration, xmlRoot);
            return xmlDoc;
        }

        /// <summary>
        /// Renders content of FictionBook as formatted xml string
        /// </summary>
        /// <returns>String content of a XDocument</returns>
        public string ToXmlString()
        {
            var document = ToXml();

            if (document == null)
                return null;

            return string.Join(Environment.NewLine, document.Declaration ?? DefaultDeclaration, document.ToString());
        }

        // wraps any exception with Fb2DocumentLoadingException
        private void LoadHandled(Action loadingAction)
        {
            try
            {
                loadingAction();
            }
            catch (Exception ex)
            {
                throw new Fb2DocumentLoadingException("Document loading failed.", ex);
            }
        }

        private async Task LoadHandledAsync(Func<Task> loadingAsync)
        {
            try
            {
                await loadingAsync();
            }
            catch (Exception ex)
            {
                throw new Fb2DocumentLoadingException("Document asynchronous loading failed.", ex);
            }
        }

        private void Load([In] XElement root, bool loadUnsafeElements)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            Book = new FictionBook();
            Book.Load(root, loadUnsafe: loadUnsafeElements);

            IsLoaded = true;
        }

        public override bool Equals(object obj) =>
            obj != null &&
            obj is Fb2Document other && // if `Book` is `null` and `other.Book` is also null - those are equal
            IsLoaded == other.IsLoaded &&
            (Book?.Equals(other.Book) ?? other.Book == null);

        public override int GetHashCode() => HashCode.Combine(Book?.GetHashCode() ?? 0, IsLoaded);
    }
}

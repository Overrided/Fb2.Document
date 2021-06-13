using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
            ConformanceLevel = ConformanceLevel.Document,
            CloseInput = true
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
        /// Creates Fb2Document with empty FictionBook, with `IsLoaded` set to `true`.
        /// </summary>
        /// <returns>New instance of Fb2Document with empty FictionBook. </returns>
        public static Fb2Document CreateDocument()
        {
            var document = new Fb2Document();
            document.Book = new FictionBook();
            document.IsLoaded = true;

            return document;
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from XDocument insance
        /// </summary>
        /// <param name="document">Content of a file read as xml</param>
        /// This method is not Encoding-safe 
        /// Loading will proceed with Encoding of XDocument received.
        public void Load([In] XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException($"{nameof(document)} is null");

            Load(document.Root);
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from string
        /// </summary>
        /// <param name="document">Content of a file read as string</param>
        /// This method is not Encoding-safe 
        /// Loading will proceed with Encoding of string received.
        public void Load([In] string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
                throw new ArgumentNullException($"{nameof(fileContent)} is null");

            var document = XDocument.Parse(fileContent);
            Load(document.Root);
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from stream.
        /// </summary>
        /// <param name="fileContent">Stream of file data, opened for read.</param>
        /// This method is Encoding-safe 
        /// Encoding for reading content will be determined during load process 
        /// or Encoding.Default will be used
        public void Load([In] Stream fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException($"{nameof(fileContent)} stream is null!");

            if (!fileContent.CanRead)
                throw new ArgumentException($"Can`t read file content : {nameof(fileContent)}.CanRead is {false}");

            using (var reader = XmlReader.Create(fileContent, DefaultXmlReaderSettings))
            {
                var document = XDocument.Load(reader);
                Load(document.Root);
            }
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from stream asynchronously.
        /// </summary>
        /// <param name="fileContent">Stream of file data, opened for read.</param>
        /// This method is Encoding-safe 
        /// Encoding for reading content will be determined during load process 
        /// or Encoding.Default will be used
        public async Task LoadAsync([In] Stream fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException($"{nameof(fileContent)} stream is null!");

            if (!fileContent.CanRead)
                throw new ArgumentException($"Can`t read file content : {nameof(fileContent)}.CanRead is {false}");

            using (var reader = XmlReader.Create(fileContent, DefaultXmlReaderSettings))
            {
                var document = await XDocument.LoadAsync(reader, LoadOptions.None, default);
                Load(document.Root);
            }
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

        private void Load([In] XElement root)
        {
            if (root == null)
                throw new ArgumentNullException($"{nameof(root)} element is null");

            Book = new FictionBook();
            Book.Load(root);

            IsLoaded = true;
        }
    }
}

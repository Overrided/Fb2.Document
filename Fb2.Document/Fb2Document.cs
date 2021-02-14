using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2.Document.Extensions;
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
        private IEnumerable<BookBody> bodies = null;
        private IEnumerable<BinaryImage> binaryImages = null;

        private XDeclaration DefaultDeclaration = new XDeclaration(defaultXmlVersion, Encoding.UTF8.HeaderName, null);

        public FictionBook Book { get; private set; }

        private BookDescription FictionBookDescription
        {
            get
            {
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
                    title = FictionBookDescription?.GetFirstChild<TitleInfo>();

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
                    sourceTitle = FictionBookDescription?.GetFirstChild<SrcTitleInfo>();

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
                    documentInfo = FictionBookDescription?.GetFirstChild<DocumentInfo>();

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
                    publishInfo = FictionBookDescription?.GetFirstChild<PublishInfo>();

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
                    customInfo = FictionBookDescription?.GetFirstChild<CustomInfo>();

                return customInfo;
            }
        }

        /// <summary>
        /// Shortcut property. Gets list of BookBody elements from FictionBook.
        /// </summary>
        public IEnumerable<BookBody> Bodies
        {
            get
            {
                if (bodies == null || !bodies.Any())
                    bodies = Book?.GetChildren<BookBody>();

                return bodies;
            }
        }

        /// <summary>
        /// Shortcut property. Gets list of BinaryImages elements from FictionBook.
        /// </summary>
        public IEnumerable<BinaryImage> BinaryImages
        {
            get
            {
                if (binaryImages == null || !binaryImages.Any())
                    binaryImages = Book?.GetChildren<BinaryImage>();

                return binaryImages;
            }
        }

        /// <summary>
        /// Indicates whether Fb2Document instance was loaded using Load or LoadAsync methods
        /// </summary>
        public bool IsLoaded { get; private set; } = false;

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
        public void Load(XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("Document is null");

            Load(document.Root);
        }

        /// <summary>
        /// Loads fb2 file's content into Fb2Document model from string
        /// </summary>
        /// <param name="document">Content of a file read as string</param>
        /// This method is not Encoding-safe 
        /// Loading will proceed with Encoding of string received.
        public void Load(string fileContent)
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
        public void Load(Stream fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException($"{nameof(fileContent)} stream is null!");

            if (!fileContent.CanRead)
                throw new ArgumentException("Can`t read file content : CanRead is false");

            Encoding encoding = null;
            using (var encodingClone = fileContent.Clone())
            {
                encoding = encodingClone.GetXmlEncodingOrDefault(Encoding.Default);
            }

            var contentCopy = fileContent.Clone();
            using (var reader = new StreamReader(contentCopy, encoding, true))
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
        public async Task LoadAsync(Stream fileContent)
        {
            if (fileContent == null)
                throw new ArgumentNullException($"{nameof(fileContent)} stream is null!");

            if (!fileContent.CanRead)
                throw new ArgumentException($"Can`t read file content : {nameof(fileContent)}.CanRead is false");

            Encoding encoding = null;
            using (var encodingClone = await fileContent.CloneAsync())
            {
                encoding = encodingClone.GetXmlEncodingOrDefault(Encoding.Default);
            }

            var contentCopy = await fileContent.CloneAsync();
            using (var reader = new StreamReader(contentCopy, encoding, true))
            {
                var content = await reader.ReadToEndAsync();
                var document = XDocument.Parse(content);

                Load(document.Root);
            }
        }

        /// <summary>
        /// Generates XDocument using previously loaded FictionBook
        /// </summary>
        /// <returns>XDocument instance formatted accordingly to fb2 rules</returns>
        public XDocument ToXml()
        {
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
            return string.Join(Environment.NewLine,
                document.Declaration ?? DefaultDeclaration,
                document.ToString());
        }

        private void Load(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException("Root element is null");

            Book = new FictionBook();
            Book.Load(root);

            IsLoaded = true;
        }
    }
}

using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;
using Fb2.Document.LoadingOptions;
using Fb2.Document.Models;
using Fb2.Document.SerializingOptions;

namespace Fb2.Document;

/// <summary>
/// Represents Fiction Book at file level.
/// Provides API for loading, reading and serializing fb2 files to FictionBook model and vice versa.
/// </summary>
public sealed class Fb2Document
{
    private const string DefaultXmlVersion = "1.0";

    private static readonly XDeclaration DefaultDeclaration = new(DefaultXmlVersion, Encoding.UTF8.HeaderName, null);
    private static readonly XmlReaderSettings DefaultXmlReaderSettings = new()
    {
        Async = true,
        CheckCharacters = true,
        IgnoreWhitespace = true,
        ConformanceLevel = ConformanceLevel.Document
    };

    /// <summary>
    /// Represents <FictionBook> - root element of a file.
    /// </summary>
    public FictionBook? Book { get; private set; }

    /// <summary>
    /// Represents Description element of a FictionBook
    /// </summary>
    public BookDescription? BookDescription
    {
        get
        {
            if (!IsLoaded || Book == null)
                return null;

            return Book.GetFirstChild<BookDescription>();
        }
    }

    /// <summary>
    /// Shortcut property. Gets Title of Description from FictionBook
    /// </summary>
    public TitleInfo? Title => BookDescription?.GetFirstChild<TitleInfo>();

    /// <summary>
    /// Shortcut property. Gets SrcTitle of Description from FictionBook
    /// Mainly exists if book is translated - and contains Title in original language
    /// </summary>
    public SrcTitleInfo? SourceTitle => BookDescription?.GetFirstChild<SrcTitleInfo>();

    /// <summary>
    /// Shortcut property. Gets DocumentInfo of Description from FictionBook
    /// </summary>
    public DocumentInfo? DocumentInfo => BookDescription?.GetFirstChild<DocumentInfo>();

    /// <summary>
    /// Shortcut property. Gets PublishInfo of Description from FictionBook
    /// </summary>
    public PublishInfo? PublishInfo => BookDescription?.GetFirstChild<PublishInfo>();

    /// <summary>
    /// Shortcut property. Gets CustomInfo of Description from FictionBook
    /// </summary>
    public CustomInfo? CustomInfo => BookDescription?.GetFirstChild<CustomInfo>();

    /// <summary>
    /// Shortcut property. Gets list of BookBody elements from FictionBook.
    /// </summary>
    public ImmutableList<BookBody> Bodies
    {
        get
        {
            if (!IsLoaded || Book == null)
                return [];

            return Book.GetChildren<BookBody>().ToImmutableList();
        }
    }

    /// <summary>
    /// Shortcut property. Gets list of BinaryImages elements from FictionBook.
    /// </summary>
    public ImmutableList<BinaryImage> BinaryImages
    {
        get
        {
            if (!IsLoaded || Book == null)
                return [];

            return Book.GetChildren<BinaryImage>().ToImmutableList();
        }
    }

    /// <summary>
    /// Indicates whether Fb2Document instance was loaded using Load or LoadAsync methods
    /// </summary>
    public bool IsLoaded { get; private set; } = false;

    /// <summary>
    /// Creates new, empty instance of Fb2Document with <see cref="IsLoaded"/> set to <see langword="true"/>.
    /// </summary>
    /// <param name="fictionBook">
    /// Optional parameter. Book to use with Fb2Document. If ommited, <see cref="Book"/> property of created document returns <see langword="null"/>.
    /// </param>
    /// <returns>New instance of Fb2Document.</returns>
    public static Fb2Document CreateDocument(FictionBook? fictionBook = null)
    {
        var document = new Fb2Document
        {
            Book = fictionBook,
            IsLoaded = fictionBook != null
        };

        return document;
    }

    /// <summary>
    /// Loads fb2 file's content into Fb2Document model from XDocument insance
    /// </summary>
    /// <param name="document">Content of a file read as xml</param>
    /// <param name="loadingOptions">Fb2Document loading options. This parameter is optional.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// This method is not Encoding-safe. 
    /// Loading will proceed with Encoding of XDocument received.
    /// </remarks>
    public void Load(
        [In] XDocument document,
        [In] Fb2LoadingOptions? loadingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));

        LoadHandled(() => Load(document.Root, loadingOptions));
    }

    /// <summary>
    /// Loads fb2 file's content into Fb2Document model from string
    /// </summary>
    /// <param name="fileContent">Content of a file read as string</param>
    /// <param name="loadingOptions">Fb2Document loading options. This parameter is optional.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// This method is not Encoding-safe.
    /// Loading will proceed with Encoding of string received.
    /// </remarks>
    public void Load(
        [In] string fileContent,
        [In] Fb2LoadingOptions? loadingOptions = null)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
            throw new ArgumentNullException(nameof(fileContent));

        LoadHandled(() =>
        {
            var document = XDocument.Parse(fileContent);
            Load(document.Root, loadingOptions);
        });
    }

    /// <summary>
    /// Loads fb2 file's content into Fb2Document model from string asynchronously
    /// </summary>
    /// <param name="fileContent">Content of a file read as string</param>
    /// <param name="loadingOptions">Fb2Document loading options. This parameter is optional.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <remarks>
    /// This method is not Encoding-safe.
    /// Loading will proceed with Encoding of string received.
    /// This method exists mostly for lulz :)
    /// </remarks>
    public async Task LoadAsync(
        [In] string fileContent,
        [In] Fb2LoadingOptions? loadingOptions = null)
    {
        if (string.IsNullOrWhiteSpace(fileContent))
            throw new ArgumentNullException(nameof(fileContent));

        await LoadHandledAsync(async () =>
        {
            using var reader = new StringReader(fileContent);
            var document = await XDocument
                .LoadAsync(reader, LoadOptions.None, default)
                .ConfigureAwait(false);

            Load(document.Root, loadingOptions);
        });
    }

    /// <summary>
    /// Loads fb2 file's content into Fb2Document model from stream.
    /// </summary>
    /// <param name="fileContent">Stream of file data, opened for read.</param>
    /// <param name="loadingOptions">Fb2Document stream loading options. This parameter is optional.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <remarks>Actual encoding of content will be determined automatically or <see cref="Encoding.Default"/> will be used.</remarks>
    public void Load(
        [In] Stream fileContent,
        [In] Fb2StreamLoadingOptions? loadingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(fileContent, nameof(fileContent));

        if (!fileContent.CanRead)
            throw new ArgumentException($"Can`t read {nameof(fileContent)}, {nameof(Stream.CanRead)} is {false}");

        var options = loadingOptions ?? new Fb2StreamLoadingOptions();

        var xmlReaderSetting = DefaultXmlReaderSettings.Clone();
        xmlReaderSetting.CloseInput = options.CloseInputStream;

        LoadHandled(() =>
        {
            using (var reader = XmlReader.Create(fileContent, xmlReaderSetting))
            {
                var document = XDocument.Load(reader);
                Load(document.Root, options);
            }
        });
    }

    /// <summary>
    /// Loads fb2 file's content into Fb2Document model from stream asynchronously.
    /// </summary>
    /// <param name="fileContent">Stream of file data, opened for read.</param>
    /// <param name="loadingOptions">Fb2Document stream loading options. This parameter is optional.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception> 
    /// <remarks> Actual encoding of content will be determined automatically or <see cref="Encoding.Default"/> will be used. </remarks>
    public async Task LoadAsync(
        [In] Stream fileContent,
        [In] Fb2StreamLoadingOptions? loadingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(fileContent, nameof(fileContent));

        if (!fileContent.CanRead)
            throw new ArgumentException($"Can`t read {nameof(fileContent)}, {nameof(Stream.CanRead)} is {false}");

        var options = loadingOptions ?? new Fb2StreamLoadingOptions();

        var xmlReaderSetting = DefaultXmlReaderSettings.Clone();
        xmlReaderSetting.CloseInput = options.CloseInputStream;

        await LoadHandledAsync(async () =>
        {
            using var reader = XmlReader.Create(fileContent, xmlReaderSetting);
            var document = await XDocument
                .LoadAsync(reader, LoadOptions.None, default)
                .ConfigureAwait(false);

            Load(document.Root, options);
        });
    }

    /// <summary>
    /// Renders content of FictionBook as XDocument.
    /// </summary>
    /// <param name="fb2XmlSerializingOptions">Fb2Document to XML serialization options. This parameter is optional.</param>
    /// <returns>
    /// XDocument instance formatted accordingly to Fb2 rules or <see langword="null"/> if <see cref="Book"/> is <see langword="null"/> or <see cref="IsLoaded"/> is <see langword="false"/>.
    /// </returns>
    public XDocument? ToXml([In] Fb2XmlSerializingOptions? fb2XmlSerializingOptions = null)
    {
        if (Book == null || !IsLoaded)
            return null;

        var serializeUnsafeNodes = fb2XmlSerializingOptions?.SerializeUnsafeElements ?? true;
        var declaration = fb2XmlSerializingOptions?.XDeclaration ?? DefaultDeclaration;

        var xmlRoot = Book.ToXml(serializeUnsafeNodes);
        var xmlDoc = new XDocument(declaration, xmlRoot);
        return xmlDoc;
    }

    /// <summary>
    /// Renders content of FictionBook as formatted XML string.
    /// </summary>
    /// <param name="fb2XmlSerializingOptions">Fb2Document to XML serialization options. This parameter is optional.</param>
    /// <returns>Content of a <see cref="Fb2Document"/> in <see cref="string"/> format if <see cref="Book"/> is loaded - otherwise <see langword="null"/>.
    /// </returns> 
    public string? ToXmlString([In] Fb2XmlSerializingOptions? fb2XmlSerializingOptions = null)
    {
        var document = ToXml(fb2XmlSerializingOptions);

        if (document == null)
            return null;

        var declaration = fb2XmlSerializingOptions?.XDeclaration ?? DefaultDeclaration;

        return string.Join(
            Environment.NewLine,
            declaration,
            document.ToString());
    }

    private static void LoadHandled(Action loadingAction)
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

    private static async Task LoadHandledAsync(Func<Task> loadingAsync)
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

    private void Load([In] XElement root, [In] Fb2LoadingOptions? loadingOptions = null)
    {
        ArgumentNullException.ThrowIfNull(root, nameof(root));

        var options = loadingOptions ?? new Fb2LoadingOptions();

        var loadUnsafeElements = options.LoadUnsafeElements;
        var loadNamespaceMetadata = options.LoadNamespaceMetadata;

        Book = new FictionBook();
        Book.Load(root, loadUnsafe: loadUnsafeElements, loadNamespaceMetadata: loadNamespaceMetadata);

        IsLoaded = true;
    }

    public override bool Equals(object? obj) =>
        obj != null &&
        obj is Fb2Document other && // if `Book` is `null` and `other.Book` is also null - those are equal
        IsLoaded == other.IsLoaded &&
        (Book?.Equals(other.Book) ?? other.Book == null);

    public override int GetHashCode() => HashCode.Combine(Book?.GetHashCode() ?? 0, IsLoaded);

    public override string ToString() => IsLoaded && Book != null ? Book.ToString() : string.Empty;
}

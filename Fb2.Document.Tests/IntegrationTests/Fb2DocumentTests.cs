using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.LoadingOptions;
using Fb2.Document.Models;
using Fb2.Document.SerializingOptions;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.IntegrationTests;

public class Fb2DocumentTests
{
    private const string SamplesFolderName = "Samples";
    private const string SampleFileName = "_Test_1.fb2";
    private const string InvalidSampleFileName = "_Test_Invalid.fb2";

    [Fact]
    public void Fb2Document_NotLoaded_ShortcutProperties_ReturnNull()
    {
        var doc = new Fb2Document();

        doc.IsLoaded.Should().BeFalse();
        doc.Book.Should().BeNull();

        doc.Bodies.Should().BeEmpty();
        doc.BookDescription.Should().BeNull();
        doc.Title.Should().BeNull();
        doc.SourceTitle.Should().BeNull();
        doc.DocumentInfo.Should().BeNull();
        doc.PublishInfo.Should().BeNull();
        doc.CustomInfo.Should().BeNull();
        doc.BinaryImages.Should().BeNullOrEmpty();

        var fb2DocumentDefaultXDeclaration = Fb2Document.DefaultXDeclaration;

        fb2DocumentDefaultXDeclaration.Should().NotBeNull();
        fb2DocumentDefaultXDeclaration.Version.Should().Be(Fb2Document.DefaultXmlVersion);
        fb2DocumentDefaultXDeclaration.Encoding.Should().Be(Encoding.UTF8.HeaderName);
        fb2DocumentDefaultXDeclaration.Standalone.Should().BeNullOrEmpty();

        var defaultReaderSettings = Fb2Document.DefaultXmlReaderSettings;

        defaultReaderSettings.Async.Should().BeTrue();
        defaultReaderSettings.CheckCharacters.Should().BeTrue();
        defaultReaderSettings.IgnoreWhitespace.Should().BeTrue();
        defaultReaderSettings.ConformanceLevel.Should().Be(ConformanceLevel.Document);
    }

    [Fact]
    public void Fb2Document_NotLoaded_ToXml_ReturnsNull()
    {
        var doc = new Fb2Document();
        doc.IsLoaded.Should().BeFalse();

        doc.Book.Should().BeNull();

        doc.ToXml().Should().BeNull();
        doc.ToXmlString().Should().BeNull();
    }


    [Fact]
    public void Fb2Document_NotLoaded_ToString_ReturnsNull()
    {
        var doc = new Fb2Document();
        doc.IsLoaded.Should().BeFalse();
        doc.Book.Should().BeNull();

        doc.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Fb2Document_Loaded_MalformedContent_Throws()
    {
        var doc = new Fb2Document();
        doc.IsLoaded.Should().BeFalse();
        doc.Book.Should().BeNull();

        doc.Invoking(d => d.Load((Stream)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        doc.Invoking(d => d.Load((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        doc.Invoking(d => d.Load((XDocument)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        doc.Invoking(async d => await d.LoadAsync((Stream)null))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();

        doc.Invoking(async d => await d.LoadAsync((string)null))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();
    }

    [Fact]
    public void Fb2Document_Empty_EqualityTest()
    {
        var emptyFirstDocument = new Fb2Document();
        emptyFirstDocument.Book.Should().BeNull();
        emptyFirstDocument.IsLoaded.Should().BeFalse();

        emptyFirstDocument.Equals(null).Should().BeFalse();

        var emptySecondDocument = new Fb2Document();
        emptySecondDocument.Book.Should().BeNull();
        emptySecondDocument.IsLoaded.Should().BeFalse();

        emptyFirstDocument.Should().Be(emptySecondDocument);

        var emptyCreatedFirstDocument = Fb2Document.CreateDocument();
        emptyCreatedFirstDocument.Book.Should().BeNull();
        emptyCreatedFirstDocument.IsLoaded.Should().BeFalse();

        var emptyCreatedSecondDocument = Fb2Document.CreateDocument();
        emptyCreatedSecondDocument.Book.Should().BeNull();
        emptyCreatedSecondDocument.IsLoaded.Should().BeFalse();

        emptyCreatedFirstDocument.Should().Be(emptyCreatedSecondDocument);

        emptyFirstDocument.Should().Be(emptyCreatedFirstDocument);
    }

    [Fact]
    public void Fb2Document_CreateWithContent_Test()
    {
        // let's imagine there was real content, right?
        var emptyCreatedFirstDocument = Fb2Document.CreateDocument(new FictionBook());
        emptyCreatedFirstDocument.Book.Should().NotBeNull();
        emptyCreatedFirstDocument.IsLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task InstancesOfBookAreSame()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using (var fileReadStream = sampleFileInfo.OpenRead())
        {
            var firstDocument = new Fb2Document();
            await firstDocument.LoadAsync(fileReadStream);

            RewindStream(fileReadStream);

            var secondDocument = Fb2Document.CreateDocument();
            await secondDocument.LoadAsync(fileReadStream);

            firstDocument.Should().Be(secondDocument);

            var firstBook = firstDocument.Book;
            var secondBook = secondDocument.Book;

            firstBook.Should().Be(secondBook);
            fileReadStream.Close();
        }
    }

    [Fact]
    public async Task BookContentCheck()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using (var fileReadStream = sampleFileInfo.OpenRead())
        {
            var document = new Fb2Document();
            await document.LoadAsync(fileReadStream);

            document.Bodies.Should().HaveCount(3);

            var firstBody = document.Bodies[0];
            var firstBodyTitle = firstBody.GetFirstChild<Title>();
            firstBodyTitle.Should().NotBeNull();
            var firstBodySections = firstBody.GetChildren<BodySection>();
            firstBodySections.Should().HaveCount(9);

            var secondBody = document.Bodies[1];
            var secondBodyAttributes = secondBody.Attributes.Should().HaveCount(1);
            var secondBodyNameAttribute = secondBody.Attributes.First();
            secondBodyNameAttribute.Key.Should().Be(AttributeNames.Name);
            secondBodyNameAttribute.Value.Should().Be("notes");

            var secondBodyTitle = secondBody.GetFirstChild<Title>();
            secondBodyTitle.Should().NotBeNull();

            var secondBodySections = secondBody.GetChildren<BodySection>();
            secondBodySections.Should().HaveCount(20);

            var thirdBody = document.Bodies[2];
            var thirdBodySections = thirdBody.GetChildren<BodySection>();
            thirdBodySections.Should().HaveCount(1);

            document.BinaryImages.Should().HaveCount(33);
        }
    }

    [Fact]
    public async Task ExportDocument_AndReload_SameContent()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using var fileReadStream = sampleFileInfo.OpenRead();
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(fileReadStream);

        var firstDocXml = firstDocument.ToXml();
        var secondDocument = new Fb2Document();
        secondDocument.Load(firstDocXml!);

        firstDocument.Should().Be(secondDocument);

        var firstBook = firstDocument.Book;
        var secondBook = secondDocument.Book;

        firstBook.Should().Be(secondBook);
    }

    [Fact]
    public async Task ExportDocument_WithoutUnsafeNodes_AndReload_DifferentContent()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using var fileReadStream = sampleFileInfo.OpenRead();
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(fileReadStream);


        var firstUnsafeNodes = firstDocument.Book!.GetDescendants(n => n.IsUnsafe).ToList();
        firstUnsafeNodes.Should().NotBeNullOrEmpty();


        var firstDocXml = firstDocument.ToXml(new Fb2XmlSerializingOptions(false));
        var secondDocument = new Fb2Document();
        secondDocument.Load(firstDocXml!);

        var secondUnsafeNodes = secondDocument.Book!.GetDescendants(n => n.IsUnsafe).ToList();
        secondUnsafeNodes.Should().BeNullOrEmpty();

        firstDocument.Should().NotBe(secondDocument);

        var firstBook = firstDocument.Book;
        var secondBook = secondDocument.Book;

        firstBook.Should().NotBe(secondBook);
    }

    [Fact]
    public async Task ExportDocument_WithDifferentOptions()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using var fileReadStream = sampleFileInfo.OpenRead();
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(fileReadStream);


        var firstUnsafeNodes = firstDocument.Book!.GetDescendants(n => n.IsUnsafe).ToList();
        firstUnsafeNodes.Should().NotBeNullOrEmpty();


        var firstDocXml = firstDocument.ToXml(new Fb2XmlSerializingOptions(false, new XDeclaration("2.0", Encoding.UTF8.HeaderName, null)));
        firstDocXml.Declaration.Version.Should().Be("2.0");
        firstDocXml.Declaration.Encoding.Should().Be(Encoding.UTF8.HeaderName);
    }

    [Fact]
    public async Task LoadWithoutUnsafeNodes_DifferentContent()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using (var fileReadStream = sampleFileInfo.OpenRead())
        {
            // loading document first time
            var firstDocument = new Fb2Document();
            await firstDocument.LoadAsync(fileReadStream);

            RewindStream(fileReadStream);

            // loading document without unsafe nodes
            var secondDocument = new Fb2Document();
            await secondDocument.LoadAsync(fileReadStream, new Fb2StreamLoadingOptions(false));

            firstDocument.Should().NotBe(secondDocument);

            var firstBook = firstDocument.Book;
            var secondBook = secondDocument.Book;

            // different content due to skipped unsafe nodes
            firstBook.Should().NotBe(secondBook);
        }
    }

    [Fact]
    public async Task LoadWithoutMetadata_DifferentContent()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using (var fileReadStream = sampleFileInfo.OpenRead())
        {
            // loading document first time
            var firstDocument = new Fb2Document();
            await firstDocument.LoadAsync(fileReadStream);

            RewindStream(fileReadStream);

            // loading document without unsafe nodes
            var secondDocument = new Fb2Document();
            await secondDocument.LoadAsync(fileReadStream, new Fb2StreamLoadingOptions(loadNamespaceMetadata: false));

            firstDocument.Book!.NodeMetadata.Should().NotBeNull();
            secondDocument.Book!.NodeMetadata.Should().BeNull();

            firstDocument.Bodies.First().NodeMetadata.Should().NotBeNull();
            secondDocument.Bodies.First().NodeMetadata.Should().BeNull();

            firstDocument.Should().NotBe(secondDocument);
        }
    }

    [Fact]
    public async Task Load_WithCloseInput_ClosesStream()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        using (var fileReadStream = sampleFileInfo.OpenRead())
        {
            // loading document without unsafe nodes
            var firstDocument = new Fb2Document();
            await firstDocument
                .LoadAsync(fileReadStream, new Fb2StreamLoadingOptions(closeInputStream: true));

            await fileReadStream
                .Invoking(async s => await s.WriteAsync(new byte[5] { 4, 2, 0, 6, 9 }))
                .Should()
                .ThrowExactlyAsync<ObjectDisposedException>();
        }
    }

    [Fact]
    public async Task SameFile_DifferentLoads_SameContent()
    {
        var sampleFileInfo = GetSampleFileInfo(SampleFileName);

        var fileStringContent = await ReadFileAsString(sampleFileInfo);
        var xDocument = await ReadFileAsXDocument(sampleFileInfo);

        var stringLoadedFb2Document = new Fb2Document();
        stringLoadedFb2Document.Load(fileStringContent); // string

        var stringLoadedAsyncFb2Document = new Fb2Document();
        await stringLoadedAsyncFb2Document.LoadAsync(fileStringContent);

        var xmlLoadedFb2Document = new Fb2Document();
        xmlLoadedFb2Document.Load(xDocument); // xDocument

        var streamLoadedFb2Document = new Fb2Document();
        var streamLoadedAsyncFb2Document = new Fb2Document();

        using (var stream = sampleFileInfo.OpenRead())
        {
            streamLoadedFb2Document.Load(stream); // sync stream
            RewindStream(stream);
            await streamLoadedAsyncFb2Document.LoadAsync(stream); // async stream
        }

        stringLoadedFb2Document
            .Should().Be(stringLoadedAsyncFb2Document)
            .And.Be(xmlLoadedFb2Document)
            .And.Be(streamLoadedFb2Document)
            .And.Be(streamLoadedAsyncFb2Document);


        stringLoadedFb2Document.Book
            .Should().Be(stringLoadedAsyncFb2Document.Book)
            .And.Be(xmlLoadedFb2Document.Book)
            .And.Be(streamLoadedFb2Document.Book)
            .And.Be(streamLoadedAsyncFb2Document.Book);
    }

    [Fact]
    public async Task Load_InvalidFile_Throws()
    {
        var invalidFileInfo = GetSampleFileInfo(InvalidSampleFileName);

        using (var stream = invalidFileInfo.OpenRead())
        {
            var fb2Document = new Fb2Document();

            await fb2Document
                .Invoking(async f => await f.LoadAsync(stream))
                .Should()
                .ThrowExactlyAsync<Fb2DocumentLoadingException>()
                .WithMessage("Document asynchronous loading failed.");

            RewindStream(stream);

            fb2Document
                .Invoking(f => f.Load(stream))
                .Should()
                .ThrowExactly<Fb2DocumentLoadingException>()
                .WithMessage("Document loading failed.");
        }

        var invalidSampleXmlString = await ReadFileAsString(invalidFileInfo);
        var secondDocument = new Fb2Document();
        secondDocument
            .Invoking(s => s.Load(invalidSampleXmlString))
            .Should()
            .ThrowExactly<Fb2DocumentLoadingException>()
            .WithMessage("Document loading failed.");
    }

    private static FileInfo GetSampleFileInfo(string fileName)
    {
        var samplesFolderPath = Path.Combine(Environment.CurrentDirectory, SamplesFolderName);

        if (!Directory.Exists(samplesFolderPath))
            throw new Exception($"{samplesFolderPath} folder does not exist.");

        var filePath = Path.Combine(samplesFolderPath, fileName);
        if (!File.Exists(filePath))
            throw new Exception($"{filePath} file does not exist.");

        var fileInfo = new FileInfo(filePath);
        if (!fileInfo.Exists)
            throw new Exception("Sample file does not exist");

        return fileInfo;
    }

    private static async Task<string> ReadFileAsString(FileInfo fileInfo)
    {
        using (var stream = fileInfo.OpenRead())
        using (var streamReader = new StreamReader(stream, true))
        {
            var text = await streamReader.ReadToEndAsync();
            return text;
        }
    }

    // recommended setting to read xml
    private static async Task<XDocument> ReadFileAsXDocument(FileInfo fileInfo)
    {
        using (var stream = fileInfo.OpenRead())
        using (var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            Async = true,
            CheckCharacters = true,
            IgnoreWhitespace = true,
            ConformanceLevel = ConformanceLevel.Document
        }))
        {
            var xDocument = await XDocument.LoadAsync(reader, LoadOptions.None, default);
            return xDocument;
        }
    }

    private static void RewindStream(Stream stream)
    {
        if (!stream.CanSeek)
            return;

        stream.Seek(0, SeekOrigin.Begin);
        stream.Position = 0;
    }
}

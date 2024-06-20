using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public void Fb2Document_CreateDocument_ShortcutProperties_ReturnNull()
    {
        var doc = Fb2Document.CreateDocument();

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
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);

        RewindStream(sampleFileInfoStream);

        var secondDocument = Fb2Document.CreateDocument();
        await secondDocument.LoadAsync(sampleFileInfoStream);

        firstDocument.Should().Be(secondDocument);

        var firstBook = firstDocument.Book;
        var secondBook = secondDocument.Book;

        firstBook.Should().Be(secondBook);
        sampleFileInfoStream.Close();
    }

    [Fact]
    public async Task BookContentCheck()
    {
        using var sampleFileInfo = GetSampleFileInfo(SampleFileName);
        var document = new Fb2Document();
        await document.LoadAsync(sampleFileInfo);

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
        document.BookDescription.Should().NotBeNull();
        document.Title.Should().NotBeNull();
        document.DocumentInfo.Should().NotBeNull();

        document.SourceTitle.Should().BeNull();
        document.PublishInfo.Should().BeNull();
        document.CustomInfo.Should().BeNull();

        var strContent = document.ToString();
        strContent.Should().NotBeNullOrEmpty();
        var stringXmlContent = document.ToXmlString();
        stringXmlContent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportDocument_AndReload_SameContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);

        var firstDocXml = firstDocument.ToXml();
        var secondDocument = new Fb2Document();
        secondDocument.Load(firstDocXml!);

        firstDocument.Should().Be(secondDocument);

        var firstBook = firstDocument.Book;
        var secondBook = secondDocument.Book;

        firstBook.Should().Be(secondBook);
    }

    [Fact]
    public async Task ExportDocument_AsString_AndReload_SameContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);

        var docXmlString = firstDocument.ToXmlString(new Fb2XmlSerializingOptions(xDeclaration: new XDeclaration("2.0", Encoding.UTF8.HeaderName, null)));
        docXmlString.Should().StartWith("<?xml version=\"2.0\" encoding=\"utf-8\"?>");

        var docXmlStringNoOptions = firstDocument.ToXmlString();
        docXmlStringNoOptions.Should().StartWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
    }

    [Fact]
    public async Task ExportDocument_WithoutUnsafeNodes_AndReload_DifferentContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);


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
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);


        var firstUnsafeNodes = firstDocument.Book!.GetDescendants(n => n.IsUnsafe).ToList();
        firstUnsafeNodes.Should().NotBeNullOrEmpty();


        var firstDocXml = firstDocument.ToXml(new Fb2XmlSerializingOptions(false, new XDeclaration("2.0", Encoding.UTF8.HeaderName, null)));
        firstDocXml.Declaration.Version.Should().Be("2.0");
        firstDocXml.Declaration.Encoding.Should().Be(Encoding.UTF8.HeaderName);
    }

    [Fact]
    public async Task LoadWithoutUnsafeNodes_DifferentContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);
        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);

        RewindStream(sampleFileInfoStream);

        // loading document without unsafe nodes
        var secondDocument = new Fb2Document();
        await secondDocument.LoadAsync(sampleFileInfoStream, new Fb2StreamLoadingOptions(false));

        firstDocument.Should().NotBe(secondDocument);

        var firstBook = firstDocument.Book;
        var secondBook = secondDocument.Book;

        // different content due to skipped unsafe nodes
        firstBook.Should().NotBe(secondBook);
    }

    [Fact]
    public async Task LoadWithoutMetadata_DifferentContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);

        // loading document first time
        var firstDocument = new Fb2Document();
        await firstDocument.LoadAsync(sampleFileInfoStream);

        RewindStream(sampleFileInfoStream);

        // loading document without unsafe nodes
        var secondDocument = new Fb2Document();
        await secondDocument.LoadAsync(sampleFileInfoStream, new Fb2StreamLoadingOptions(loadNamespaceMetadata: false));

        firstDocument.Book!.NodeMetadata.Should().NotBeNull();
        secondDocument.Book!.NodeMetadata.Should().BeNull();

        firstDocument.Bodies.First().NodeMetadata.Should().NotBeNull();
        secondDocument.Bodies.First().NodeMetadata.Should().BeNull();

        firstDocument.Should().NotBe(secondDocument);
    }

    [Fact]
    public async Task Load_WithCloseInput_ClosesStream()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);

        // loading document without unsafe nodes
        var firstDocument = new Fb2Document();
        await firstDocument
            .LoadAsync(sampleFileInfoStream, new Fb2StreamLoadingOptions(closeInputStream: true));

        await sampleFileInfoStream
            .Invoking(async s => await s.WriteAsync(new byte[5] { 4, 2, 0, 6, 9 }))
            .Should()
            .ThrowExactlyAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task SameFile_DifferentLoads_SameContent()
    {
        using var sampleFileInfoStream = GetSampleFileInfo(SampleFileName);

        var fileStringContent = await ReadFileAsString(sampleFileInfoStream);
        RewindStream(sampleFileInfoStream);
        var xDocument = await ReadFileAsXDocument(sampleFileInfoStream);
        RewindStream(sampleFileInfoStream);

        var stringLoadedFb2Document = new Fb2Document();
        stringLoadedFb2Document.Load(fileStringContent); // string

        var stringLoadedAsyncFb2Document = new Fb2Document();
        await stringLoadedAsyncFb2Document.LoadAsync(fileStringContent);

        var xmlLoadedFb2Document = new Fb2Document();
        xmlLoadedFb2Document.Load(xDocument); // xDocument

        var streamLoadedFb2Document = new Fb2Document();
        var streamLoadedAsyncFb2Document = new Fb2Document();


        streamLoadedFb2Document.Load(sampleFileInfoStream); // sync stream
        RewindStream(sampleFileInfoStream);
        await streamLoadedAsyncFb2Document.LoadAsync(sampleFileInfoStream); // async stream

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
        var invalidFileInfoStream = GetSampleFileInfo(InvalidSampleFileName);

        // using stream OK
        using (invalidFileInfoStream)
        {
            var fb2Document = new Fb2Document();

            await fb2Document
                .Invoking(async f => await f.LoadAsync(invalidFileInfoStream))
                .Should()
                .ThrowExactlyAsync<Fb2DocumentLoadingException>()
                .WithMessage("Document asynchronous loading failed.");

            RewindStream(invalidFileInfoStream);

            fb2Document
                .Invoking(f => f.Load(invalidFileInfoStream))
                .Should()
                .ThrowExactly<Fb2DocumentLoadingException>()
                .WithMessage("Document loading failed.");

            RewindStream(invalidFileInfoStream);

            var invalidSampleXmlString = await ReadFileAsString(invalidFileInfoStream);
            var secondDocument = new Fb2Document();
            secondDocument
                .Invoking(s => s.Load(invalidSampleXmlString))
                .Should()
                .ThrowExactly<Fb2DocumentLoadingException>()
                .WithMessage("Document loading failed.");

            RewindStream(invalidFileInfoStream);
        }

        var thrirdDocument = new Fb2Document();
        thrirdDocument
            .Invoking(s => s.Load(invalidFileInfoStream))
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage($"Can`t read fileContent, {nameof(Stream.CanRead)} is {false}");

        await thrirdDocument
            .Invoking(async s => await s.LoadAsync(invalidFileInfoStream))
            .Should()
            .ThrowExactlyAsync<ArgumentException>()
            .WithMessage($"Can`t read fileContent, {nameof(Stream.CanRead)} is {false}");
    }

    private static Stream? GetSampleFileInfo(string fileName)
    {
        var x = Assembly.GetExecutingAssembly();
        var names = x.GetManifestResourceNames();

        var normalizedName = names.FirstOrDefault(n => n.EndsWith(fileName));
        if (string.IsNullOrEmpty(normalizedName))
            throw new Exception();

        var fb2FileContentStream = x.GetManifestResourceStream(normalizedName);
        return fb2FileContentStream;
    }

    private static async Task<string> ReadFileAsString(Stream fileContent)
    {
        var streamReader = new StreamReader(fileContent, true);
        var text = await streamReader.ReadToEndAsync();
        return text;
    }

    // recommended setting to read xml
    private static async Task<XDocument> ReadFileAsXDocument(Stream fileContent)
    {
        var reader = XmlReader.Create(fileContent, new XmlReaderSettings
        {
            Async = true,
            CheckCharacters = true,
            IgnoreWhitespace = true,
            ConformanceLevel = ConformanceLevel.Document
        });

        var xDocument = await XDocument.LoadAsync(reader, LoadOptions.None, default);
        return xDocument;
    }

    private static void RewindStream(Stream stream)
    {
        if (!stream.CanSeek)
            return;

        stream.Seek(0, SeekOrigin.Begin);
        stream.Position = 0;
    }
}

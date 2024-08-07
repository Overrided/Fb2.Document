﻿using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

public class Fb2ElementTests
{
    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_Load_NullNode_Throws(Fb2Element fb2Element)
    {
        fb2Element.Should().NotBeNull();

        fb2Element.Invoking(f => f.Load(null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_Load_InvalidNode_Throws(Fb2Element fb2Element)
    {
        fb2Element.Should().NotBeNull();

        var invalidXmlNode = new XElement("invalidName", "test content");

        fb2Element.Invoking(f => f.Load(invalidXmlNode))
            .Should()
            .ThrowExactly<Fb2NodeLoadingException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_Load_InvalidXmlNodeType_Throws(Fb2Element fb2Element)
    {
        fb2Element.Should().NotBeNull();

        var invalidXmlNode = new XComment("test comment");

        fb2Element.Invoking(f => f.Load(invalidXmlNode))
            .Should()
        .ThrowExactly<Fb2NodeLoadingException>()
            .WithMessage($"Unsupported nodeType: received {XmlNodeType.Comment}, expected {XmlNodeType.Element} or {XmlNodeType.Text}");
    }

    [Fact]
    public void Fb2Element_Load_ValidNode_Works()
    {
        var fb2Element = new TextItem(); // fb2 counterpart of XText, plain text node

        var validNode = new XText("test content");

        fb2Element.Load(validNode);

        fb2Element.Content.Should().Be("test content");
    }

    [Fact]
    public void EmptyLine_ContentChange_IsIgnored()
    {
        var emptyLine = new EmptyLine();
        emptyLine.Content.Should().Be(Environment.NewLine);

        emptyLine
            .AppendContent("new part 2", "blah1")
            .AddContent("new part", "blah");

        emptyLine.Content.Should().Be(Environment.NewLine);

        emptyLine.ClearContent();

        emptyLine.Content.Should().Be(Environment.NewLine);
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public async Task Fb2Element_AddNullOrEmpty_Content_Throws(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element
            .Invoking(n => n.AddContent((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        fb2Element
            .Invoking(n => n.AddContent((Func<string>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        await fb2Element
            .Invoking(n => n.AddContentAsync(null))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_AddContent_Works(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element = fb2Element.AddContent("test content 1", "   "); // 3 whitespaces
        fb2Element.Content.Should().Be("   test content 1");
        fb2Element.HasContent.Should().BeTrue();

        fb2Element.ClearContent();
        fb2Element.HasContent.Should().BeFalse();
        fb2Element.Content.Should().Be(string.Empty);
        fb2Element.Content.Length.Should().Be(0);

        fb2Element.AddContent("test content 1"); // no separator
        fb2Element.HasContent.Should().BeTrue();

        fb2Element.Content.Should().Be("test content 1");

        fb2Element.AddContent("test content 2", "   ");
        fb2Element.Content.Should().Be("test content 1   test content 2");

        fb2Element.AddContent("test content 3"); // no separator
        fb2Element.Content.Should().Be("test content 1   test content 2test content 3");

        fb2Element.AddContent(() => $"test {Environment.NewLine} content 4", " _blah_ ");
        fb2Element
            .Content
            .Should()
            .Be("test content 1   test content 2test content 3 _blah_ test   content 4");

        fb2Element.AddContent(() => $"test {Environment.NewLine} content 5", "  _blah_ ");
        fb2Element
            .Content
            .Should()
            .Be("test content 1   test content 2test content 3 _blah_ test   content 4  _blah_ test   content 5");
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public async Task Fb2Element_AddContent_FunctionProviders_Works(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element = fb2Element.AddContent(() => "test content 1", "   "); // 3 whitespaces
        fb2Element.Content.Should().Be("   test content 1");
        fb2Element.HasContent.Should().BeTrue();

        fb2Element.ClearContent();
        fb2Element.HasContent.Should().BeFalse();

        await fb2Element.AddContentAsync(async () =>
        {
            await Task.Delay(1);
            return "test content 1";
        }); // no separator
        fb2Element.HasContent.Should().BeTrue();

        fb2Element.Content.Should().Be("test content 1");

        fb2Element.AddContent(() => "test content 2", "   ");
        fb2Element.Content.Should().Be("test content 1   test content 2");

        await fb2Element.AddContentAsync(() => Task.FromResult("test content 3")); // no separator
        fb2Element.Content.Should().Be("test content 1   test content 2test content 3");

        fb2Element.AddContent(() => $"test {Environment.NewLine} content 4", " _blah_ ");
        fb2Element
            .Content
            .Should()
            .Be("test content 1   test content 2test content 3 _blah_ test   content 4");

        await fb2Element.AddContentAsync(async () =>
        {
            await Task.Delay(2);
            return $"test {Environment.NewLine} content 5";
        }, "  _blah_ ");
        fb2Element
            .Content
            .Should()
            .Be("test content 1   test content 2test content 3 _blah_ test   content 4  _blah_ test   content 5");
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_AddContent_EscapesValue(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element.AddContent("<test Value content 1");
        fb2Element.Content.Should().Be("&lt;test Value content 1");

        fb2Element.ClearContent();
        fb2Element.Content.Should().BeEmpty();

        fb2Element.AddContent(@"<""testValue&tv'2"">");
        fb2Element.Content.Should().Be("&lt;&quot;testValue&amp;tv&apos;2&quot;&gt;");

        fb2Element.ClearContent();
        fb2Element.Content.Should().BeEmpty();

        fb2Element.AddContent($"<test Value{Environment.NewLine}content 1");
        fb2Element.Content.Should().Be("&lt;test Value content 1");
    }

    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public void Fb2Element_AddContent_EscapesSeparator(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element.AddContent("test Value content 1", "<sep/> ");
        fb2Element.Content.Should().Be("&lt;sep/&gt; test Value content 1");

        fb2Element.ClearContent();
        fb2Element.Content.Should().BeEmpty();

        fb2Element.AddContent("testContent", @"<""sep&tv'2""> ");
        fb2Element.Content.Should().Be("&lt;&quot;sep&amp;tv&apos;2&quot;&gt; testContent");

        fb2Element.ClearContent();
        fb2Element.Content.Should().BeEmpty();

        fb2Element.AddContent("test content", $"<test Value{Environment.NewLine}content 1 ");
        fb2Element.Content.Should().Be("&lt;test Value content 1 test content");
    }

    [Fact]
    public void BinaryImage_Load_TrimsAllWhitespaces()
    {
        var binaryImageNode = new XElement(ElementNames.BinaryImage, "some text with spaces");
        var binaryImage = new BinaryImage();
        binaryImage.Load(binaryImageNode);
        binaryImage.Content.Should().Be("sometextwithspaces");
    }
}

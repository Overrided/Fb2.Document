using System;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class Fb2ElementTests
    {
        [Theory]
        [ClassData(typeof(Fb2ElementCollection))]
        public void Fb2Element_Load_NullNode_Throws(Fb2Element fb2Element)
        {
            fb2Element.Should().NotBeNull();

            fb2Element.Invoking(f => f.Load(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2ElementCollection))]
        public void Fb2Element_Load_InvalidNode_Throws(Fb2Element fb2Element)
        {
            fb2Element.Should().NotBeNull();

            var invalidXmlNode = new XElement("invalidName", "test content");

            fb2Element.Invoking(f => f.Load(invalidXmlNode))
                .Should()
                .Throw<Fb2NodeLoadingException>();
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
                .WithContent("new part 2", "blah1", true)
                .AddContent("new part", "blah", true);

            emptyLine.Content.Should().Be(Environment.NewLine);

            emptyLine.ClearContent();

            emptyLine.Content.Should().Be(Environment.NewLine);
        }

        [Theory]
        [ClassData(typeof(Fb2ElementCollection))]
        public void Fb2Element_AddContent_Works(Fb2Element fb2Element)
        {
            if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
                return;

            fb2Element.AddContent("test content 1", "   "); // 3 whitespaces
            fb2Element.Content.Should().Be("test content 1");

            fb2Element.AddContent("test content 2", "   ");
            fb2Element.Content.Should().Be("test content 1 test content 2");

            fb2Element.AddContent("test content 3"); // no separator
            fb2Element.Content.Should().Be("test content 1 test content 2test content 3");

            fb2Element.AddContent(() => $"test {Environment.NewLine} content 4", " _blah_ "); // no preserving whitespace
            fb2Element
                .Content
                .Should()
                .Be("test content 1 test content 2test content 3 _blah_ test content 4");

            // preserving whitespace works for new content, but not for separator - it starts with 2 whitespaces
            fb2Element.AddContent(() => $"test {Environment.NewLine} content 5", "  _blah_ ", true);
            fb2Element
                .Content
                .Should() // separator always getting trimmed, even when preserving whitespaces
                .Be($"test content 1 test content 2test content 3 _blah_ test content 4 _blah_ test {Environment.NewLine} content 5");
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
}

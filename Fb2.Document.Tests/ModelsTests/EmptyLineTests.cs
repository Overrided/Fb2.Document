using System;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class EmptyLineTests
    {
        [Fact]
        public void EmptyLine_AddTextContent_IsIgnored()
        {
            var emptyLine = Fb2NodeFactory.GetNodeByName(ElementNames.EmptyLine) as Fb2Element;

            emptyLine.Should().NotBeNull();

            emptyLine.Content.Should().Be(Environment.NewLine);

            emptyLine.AddContent("test content", " ");

            emptyLine.Content.Should().Be(Environment.NewLine);

            emptyLine.AddContent(() => "test content", " ");

            emptyLine.Content.Should().Be(Environment.NewLine);
        }

        [Fact]
        public void EmptyLine_HasContent_Returns_True()
        {
            var emptyLine = Fb2NodeFactory.GetNodeByName(ElementNames.EmptyLine) as Fb2Element;
            emptyLine.HasContent.Should().BeTrue(); // funny, but Environment.Newline is not empty string ;)
        }
    }
}

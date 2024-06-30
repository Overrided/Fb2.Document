using System.Threading.Tasks;
using Fb2.Document.Extensions;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ExtensionsTests;

public class Fb2ElementExtensionsTests
{
    [Theory]
    [ClassData(typeof(Fb2ElementCollection))]
    public async Task Fb2Element_AppendContent_Works(Fb2Element fb2Element)
    {
        if (fb2Element is EmptyLine || fb2Element is SequenceInfo)
            return;

        fb2Element.HasContent.Should().BeFalse();
        fb2Element.Content.Should().BeNullOrEmpty();

        fb2Element.AppendContent("test content");

        fb2Element.HasContent.Should().BeTrue();
        fb2Element.Content.Should().Be("test content");

        fb2Element.EraseContent();

        fb2Element.HasContent.Should().BeFalse();
        fb2Element.Content.Should().BeNullOrEmpty();

        fb2Element.AppendContent(() => "func test content");
        fb2Element.HasContent.Should().BeTrue();
        fb2Element.Content.Should().Be("func test content");

        fb2Element.EraseContent();

        fb2Element.HasContent.Should().BeFalse();
        fb2Element.Content.Should().BeNullOrEmpty();

        await fb2Element.AppendContentAsync(async () => await Task.FromResult("from result content"));

        fb2Element.HasContent.Should().BeTrue();
        fb2Element.Content.Should().Be("from result content");

        fb2Element.EraseContent();

        fb2Element.HasContent.Should().BeFalse();
        fb2Element.Content.Should().BeNullOrEmpty();
    }
}

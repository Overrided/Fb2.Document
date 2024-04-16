using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

public class TextLinkTests
{
    [Fact]
    public void TextLink_WithContent_NoHref_ToString_Works()
    {
        var textLink = (Fb2NodeFactory.GetNodeByName(ElementNames.TextLink) as Fb2Container);
        textLink.Should().NotBeNull();
        textLink!.HasAttributes.Should().BeFalse();
        textLink.HasContent.Should().BeFalse();

        textLink.AddTextContent("test link text");

        textLink.HasAttributes.Should().BeFalse();
        textLink.HasContent.Should().BeTrue();

        var textLinkString = textLink.ToString();
        textLinkString.Should().NotBeNullOrEmpty();
        textLinkString.Should().Be("test link text");
    }

    [Fact]
    public void TextLink_NoContent_WithHref_ToString()
    {
        var textLink = (Fb2NodeFactory.GetNodeByName(ElementNames.TextLink) as Fb2Container);
        textLink.Should().NotBeNull();
        textLink!.HasAttributes.Should().BeFalse();
        textLink.HasContent.Should().BeFalse();

        textLink.AddAttribute(AttributeNames.XHref, "www.test.com");

        textLink.HasAttributes.Should().BeTrue();
        textLink.HasContent.Should().BeFalse();

        var textLinkString = textLink.ToString();
        textLinkString.Should().NotBeNullOrEmpty();
        textLinkString.Should().Be("www.test.com");
    }

    [Fact]
    public void TextLink_Both_ContentAndHref_ToString()
    {
        var textLink = (Fb2NodeFactory.GetNodeByName(ElementNames.TextLink) as Fb2Container);
        textLink.Should().NotBeNull();
        textLink!.HasAttributes.Should().BeFalse();
        textLink.HasContent.Should().BeFalse();

        textLink.AddTextContent("test link text");
        textLink.AddAttribute(AttributeNames.XHref, "www.test.com");

        textLink.HasAttributes.Should().BeTrue();
        textLink.HasContent.Should().BeTrue();

        var textLinkString = textLink.ToString();
        textLinkString.Should().NotBeNullOrEmpty();
        textLinkString.Should().Be("test link text (www.test.com)");
    }
}

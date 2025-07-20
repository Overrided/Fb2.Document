using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

public class ImageTests
{
    [Fact]
    public async Task LoadImage_WithoutParentNode_DefaultsToInline()
    {
        var image = new Image();
        image.IsInline.Should().BeTrue(); // default value

        var invalidBase64Content = "someTestText";
        var validImageXNode = new XElement(ElementNames.Image, invalidBase64Content);

        await image.Load(validImageXNode);

        image.IsInline.Should().BeTrue();
    }

    [Fact]
    public void NoAttributes_ToString_ReturnsName()
    {
        var image = new Image();
        image.HasAttributes.Should().BeFalse();

        var stringWithoutHref = image.ToString();
        stringWithoutHref.Should().NotBeNullOrWhiteSpace().And.Be(image.Name);

        image.AddAttribute(AttributeNames.XHref, "testXHrefAttributeValue");
        image.HasAttributes.Should().BeTrue();

        var stringWithHref = image.ToString();
        stringWithHref.Should().NotBeNullOrWhiteSpace().And.Be($"{image.Name} testXHrefAttributeValue");
    }
}

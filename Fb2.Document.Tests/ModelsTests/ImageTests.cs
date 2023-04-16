using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class ImageTests
    {
        [Fact]
        public void LoadImage_WithoutParentNode_DefaultsToInline()
        {
            var image = new Image();
            image.IsInline.Should().BeTrue(); // default value

            var invalidBase64Content = "someTestText";
            var validImageXNode = new XElement(ElementNames.Image, invalidBase64Content);

            image.Load(validImageXNode);

            image.IsInline.Should().BeTrue();
        }
    }
}

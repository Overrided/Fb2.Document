using System;
using Fb2.Document.Exceptions;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class Fb2AttributeTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public void Fb2Attribute_Create_EmptyKey_Throws(string emptyKey)
        {
            Action action = () =>
            {
                var attribute = new Fb2Attribute(emptyKey, "2");
            };

            action
                .Should()
                .ThrowExactly<InvalidAttributeException>()
                .WithMessage("AttributeKey is null or empty string, or contains invalid characters.");
        }

        [Fact]
        public void Fb2Attribute_Create_Works()
        {
            var attribute1 = new Fb2Attribute("1", "2");
            attribute1.Key.Should().Be("1");
            attribute1.Value.Should().Be("2");
            attribute1.NamespaceName.Should().BeNull();

            var attribute2 = new Fb2Attribute("1.1", "2.2", "3");
            attribute2.Key.Should().Be("1.1");
            attribute2.Value.Should().Be("2.2");
            attribute2.NamespaceName.Should().Be("3");
        }

        [Fact]
        public void Fb2Attribute_Create_EscapesKeyValue_Works()
        {
            var attribute1 = new Fb2Attribute("1<", "2>");
            attribute1.Key.Should().Be("1&lt;");
            attribute1.Value.Should().Be("2&gt;");
            attribute1.NamespaceName.Should().BeNull();

            var attribute2 = new Fb2Attribute("1.1!", "2.2?", "<3>");
            attribute2.Key.Should().Be("1.1!");
            attribute2.Value.Should().Be("2.2?");
            attribute2.NamespaceName.Should().Be("<3>");

            var attribute3 = new Fb2Attribute("1<", "2>", "<3>");
            attribute3.Key.Should().Be("1&lt;");
            attribute3.Value.Should().Be("2&gt;");
            attribute3.NamespaceName.Should().Be("<3>");
        }

        [Fact]
        public void Fb2Attribute_Comparison_SameValues_Works()
        {
            var attribute1 = new Fb2Attribute("1", "2");
            var attribute2 = new Fb2Attribute("1", "2");

            attribute1.Should().Be(attribute1);
            attribute1.Equals(null).Should().BeFalse();
            attribute1.Should().NotBeNull();

            attribute1.Should().Be(attribute2);
            attribute1.Should().BeEquivalentTo(attribute2);

            var attribute11 = new Fb2Attribute("1", "2", "a");
            var attribute21 = new Fb2Attribute("1", "2", "a");

            attribute11.Should().Be(attribute21);
            attribute11.Should().BeEquivalentTo(attribute21);

            attribute11.Should().NotBe(attribute1);
            attribute11.Should().NotBe(attribute2);

            attribute21.Should().NotBe(attribute1);
            attribute21.Should().NotBe(attribute2);
        }

        [Fact]
        public void Fb2Attribute_Comparison_DifferentValues_Works()
        {
            var attribute1 = new Fb2Attribute("1", "2");
            var attribute2 = new Fb2Attribute("1", "3");

            attribute1.Should().NotBe(attribute2);
            attribute1.Should().NotBeEquivalentTo(attribute2);

            var attribute11 = new Fb2Attribute("1", "2", "a");
            var attribute21 = new Fb2Attribute("1", "2", "b");

            attribute11.Should().NotBe(attribute1);
            attribute11.Should().NotBe(attribute2);

            attribute21.Should().NotBe(attribute1);
            attribute21.Should().NotBe(attribute2);

            attribute11.Should().NotBe(attribute21);
            attribute11.Should().NotBeEquivalentTo(attribute21);
        }
    }
}

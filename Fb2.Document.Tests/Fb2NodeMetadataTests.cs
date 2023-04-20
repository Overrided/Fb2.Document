using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests
{
    public class Fb2NodeMetadataTests
    {
        [Fact]
        public void Fb2Metadata_Create_NoNamespaceDeclarations_Works()
        {
            Fb2NodeMetadata test;
            Action act = () =>
            {
                test = new Fb2NodeMetadata(XNamespace.Xml);
                test.Should().NotBeNull();
                test.DefaultNamespace.Should().Be(XNamespace.Xml);
                test.NamespaceDeclarations.Should().BeNullOrEmpty();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Fb2Metadata_Create_WithNamespaceDeclarations_Works()
        {
            Fb2NodeMetadata meta;
            Action act = () =>
            {
                var namespaceAttribute = new XAttribute("xmlns", "www.fourthcoffee.com");
                meta = new Fb2NodeMetadata(XNamespace.Xml, new List<XAttribute> { namespaceAttribute });
                meta.Should().NotBeNull();
                meta.DefaultNamespace.Should().Be(XNamespace.Xml);
                meta.NamespaceDeclarations.Should().NotBeNullOrEmpty().And.HaveCount(1);
                meta.NamespaceDeclarations.First().Should().NotBeNull().And.Be(namespaceAttribute);
            };

            act.Should().NotThrow();
        }

        [Fact]
        public void Fb2MetadataCopyConstructor_Works()
        {
            var originalMetadataWithNamespaceOnly = new Fb2NodeMetadata(XNamespace.Xml);
            var metadataCloneWithNamespaceOnly = new Fb2NodeMetadata(originalMetadataWithNamespaceOnly);

            metadataCloneWithNamespaceOnly.DefaultNamespace.Should().Be(XNamespace.Xml);
            metadataCloneWithNamespaceOnly.Should().Be(originalMetadataWithNamespaceOnly);

            var fullMetadata = new Fb2NodeMetadata(
                XNamespace.Xml,
                new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            var fullMetadataClone = new Fb2NodeMetadata(fullMetadata);
            fullMetadataClone.DefaultNamespace.Should().Be(XNamespace.Xml);
            fullMetadataClone.NamespaceDeclarations.Should().HaveCount(1);
            fullMetadataClone.Should().Be(fullMetadata);
        }

        [Fact]
        public void Fb2Metadata_EqualNodes_Test()
        {
            var emptyMetadata = new Fb2NodeMetadata();
            var emptyMetadataClone = new Fb2NodeMetadata();

            emptyMetadata.Should().Be(emptyMetadataClone);

            var metadataWithNamespaceOnly = new Fb2NodeMetadata(XNamespace.Xmlns);
            var metadataWithNamespaceOnlyClone = new Fb2NodeMetadata(XNamespace.Xmlns);

            metadataWithNamespaceOnly.Should().Be(metadataWithNamespaceOnlyClone);

            var metadataWithNamespaceAttributesOnly =
                new Fb2NodeMetadata(namespaceDeclarations: new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            var metadataWithNamespaceAttributesOnlyClone =
                new Fb2NodeMetadata(namespaceDeclarations: new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            metadataWithNamespaceAttributesOnly.Should().Be(metadataWithNamespaceAttributesOnlyClone);

            var fullMetadata = new Fb2NodeMetadata(XNamespace.Xml, new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            fullMetadata.Should().Be(fullMetadata); // Reference equals

            var fullMetadataClone = new Fb2NodeMetadata(XNamespace.Xml, new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            fullMetadata.Should().Be(fullMetadataClone);
        }

        [Fact]
        public void Fb2Metadata_NotEqualNodes_Test()
        {
            var emptyMetadata = new Fb2NodeMetadata();

            emptyMetadata.Should().NotBe(null);
            emptyMetadata.Should().NotBe(new object());

            var metadataWithNamespaceOnly = new Fb2NodeMetadata(XNamespace.Xmlns);

            var metadataWithNamespaceAttributesOnly =
                new Fb2NodeMetadata(namespaceDeclarations: new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            var fullMetadata = new Fb2NodeMetadata(XNamespace.Xml, new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            var fullMetadataWithDifferentNamespace = new Fb2NodeMetadata(XNamespace.Xmlns, new List<XAttribute>
                {
                    new XAttribute("xmlns", "www.fourthcoffee.com")
                });

            emptyMetadata.Should()
                .NotBe(metadataWithNamespaceOnly)
                .And.NotBe(metadataWithNamespaceAttributesOnly)
                .And.NotBe(fullMetadata)
                .And.NotBe(fullMetadataWithDifferentNamespace);

            metadataWithNamespaceOnly.Should()
                .NotBe(emptyMetadata)
                .And.NotBe(metadataWithNamespaceAttributesOnly)
                .And.NotBe(fullMetadata)
                .And.NotBe(fullMetadataWithDifferentNamespace);

            metadataWithNamespaceAttributesOnly.Should()
                .NotBe(metadataWithNamespaceOnly)
                .And.NotBe(emptyMetadata)
                .And.NotBe(fullMetadata)
                .And.NotBe(fullMetadataWithDifferentNamespace);

            fullMetadata.Should()
                .NotBe(metadataWithNamespaceOnly)
                .And.NotBe(metadataWithNamespaceAttributesOnly)
                .And.NotBe(emptyMetadata)
                .And.NotBe(fullMetadataWithDifferentNamespace);

            fullMetadataWithDifferentNamespace.Should()
                .NotBe(metadataWithNamespaceOnly)
                .And.NotBe(metadataWithNamespaceAttributesOnly)
                .And.NotBe(fullMetadata)
                .And.NotBe(emptyMetadata);
        }

        [Fact]
        public void CreateFb2Metadata_InvalidAttributes_Throws()
        {
            var attr = new XAttribute("testName", "testValue");
            Action action = () =>
            {
                var metadata = new Fb2NodeMetadata(
                    XNamespace.Xml,
                    new List<XAttribute> { attr });
            };
            action
                .Should()
                .ThrowExactly<ArgumentException>();
        }
    }
}
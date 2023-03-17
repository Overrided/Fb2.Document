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
        public void Fb2MetadataCopyContructor_Works()
        {
            var originalMetadata = new Fb2NodeMetadata(XNamespace.Xml);
            var metadataClone = new Fb2NodeMetadata(originalMetadata);

            metadataClone.DefaultNamespace.Should().Be(XNamespace.Xml);
        }

        [Fact]
        public void CreateFb2Metadata_InvalidAttributes_Throws()
        {
            var attr = new XAttribute("testName", "testValue");
            Action action = () =>
            {
                var metadata = new Fb2NodeMetadata(
                    namespaceDeclarations: new List<XAttribute> { attr });
            };
            action
                .Should()
                .ThrowExactly<ArgumentException>();
        }
    }
}
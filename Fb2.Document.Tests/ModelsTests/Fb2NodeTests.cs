using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class Fb2NodeTests
    {
        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void EmptyNodes_EqualityNullTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var equals = instanceOne.Equals(null);
            equals.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void EmptyNodes_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var instanceTwo = Fb2ElementFactory.GetNodeByName(nodeName);

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeTrue();
            instanceOne.Should().BeEquivalentTo(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void CloneNode_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var instanceTwo = instanceOne.Clone();

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeTrue();
            instanceOne.Should().BeEquivalentTo(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void Clone_AddAttribute_NotEquals(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            if (instanceOne.AllowedAttributes == null || !instanceOne.AllowedAttributes.Any())
                return;

            var instanceTwo = instanceOne.Clone() as Fb2Node;
            //instanceTwo.AddAttribute(() => new KeyValuePair<string, string>(instanceOne.AllowedAttributes.First(), "testValue"));
            //instanceTwo.AddAttribute(new KeyValuePair<string, string>(instanceOne.AllowedAttributes.First(), "testValue"));
            instanceTwo.AddAttribute(instanceOne.AllowedAttributes.First(), "testValue");

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeFalse();

            instanceOne.Attributes.Should().BeEmpty();
            instanceTwo.Attributes.Should().NotBeEmpty();
            instanceTwo.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void SimilarModifications_ElementsEquality()
        {
            var paragOne = new Paragraph();
            var paragTwo = new Paragraph();

            paragOne.Should().Be(paragTwo);

            paragOne.AddContent(new Emphasis().AddTextContent("test text content"));
            paragOne.Should().NotBe(paragTwo);

            paragTwo.AddContent(new Emphasis().AddTextContent("test text content"));
            paragOne.Should().Be(paragTwo);

            paragOne.AddAttribute(AttributeNames.Id, "testId");
            paragOne.Should().NotBe(paragTwo);

            paragTwo.AddAttribute(AttributeNames.Id, "testId");
            paragOne.Should().Be(paragTwo);
        }

        //[Theory]
        //[ClassData(typeof(Fb2NodeNamesData))]
        //public void ContentReferencesAreIndependant(string nodeName)
        //{
        //}

        //[Theory]
        //[ClassData(typeof(Fb2NodeNamesData))]
        //public void Clone_RemoveAttribute_Equals(string nodeName)
        //{
        //    var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
        //    if (instanceOne.AllowedAttributes == null || !instanceOne.AllowedAttributes.Any())
        //        return;

        //    var firstAllowedAttributeName = instanceOne.AllowedAttributes.First();

        //    var instanceTwo = instanceOne.Clone() as Fb2Node;
        //    instanceTwo.AddAttribute(() => new KeyValuePair<string, string>(firstAllowedAttributeName, "testValue"));

        //    var equals = instanceOne.Equals(instanceTwo);
        //    equals.Should().BeFalse();

        //    instanceOne.Attributes().Should().BeEmpty();
        //    instanceTwo.Attributes().Should().NotBeEmpty();
        //    instanceTwo.Attributes().Count.Should().Be(1);

        //    instanceTwo.RemoveAttribute(firstAllowedAttributeName);
        //    instanceTwo.Attributes().Should().BeEmpty();

        //    var equals2 = instanceOne.Equals(instanceTwo);
        //    equals2.Should().BeTrue();
        //}
    }
}

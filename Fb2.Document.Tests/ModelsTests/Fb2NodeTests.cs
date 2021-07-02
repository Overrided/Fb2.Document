using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
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
            instanceOne.Should().NotBe(null);

            var instanceTwo = Fb2ElementFactory.GetNodeByName(nodeName);
            instanceTwo.Should().NotBe(null);

            instanceOne.Should().Be(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void CloneNode_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var instanceTwo = instanceOne.Clone();

            instanceOne.Should().Be(instanceTwo);
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

            instanceOne.Should().NotBe(instanceTwo);

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

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void AddAttribute_NoAttributesAllowed_Throws(string nodeName)
        {
            var instance = Fb2ElementFactory.GetNodeByName(nodeName);
            instance.Should().NotBe(null);

            if (instance.AllowedAttributes.Any())
                return;

            instance
                .Invoking(i => i.AddAttribute("testKey", "testValue"))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttribute(() => new KeyValuePair<string, string>("testKey", "testValue")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttribute(new KeyValuePair<string, string>("testKey", "testValue")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttributes(
                    new("testKey", "testValue"),
                    new("testKey2", "testValue2")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttributes(
                    new Dictionary<string, string> {
                        { "testKey", "testValue" },
                        { "testKey2", "testValue2" }
                    }))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");

            instance
                .Invoking(async i => await i.AddAttributeAsync(() => Task.FromResult(new KeyValuePair<string, string>("testKey", "testValue"))))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"{instance.Name} has no allowed attributes.");
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void AddAttribute_EmptyOrNull_Throws(string nodeName)
        {
            var instance = Fb2ElementFactory.GetNodeByName(nodeName);
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            instance
                .Invoking(i => i.AddAttribute(null))
                .Should()
                .ThrowExactly<ArgumentNullException>();

            instance
                .Invoking(i => i.AddAttributeAsync(null))
                .Should()
                .ThrowExactly<ArgumentNullException>();

            instance
                .Invoking(i => i.AddAttributes())
                .Should()
                .ThrowExactly<ArgumentNullException>();

            instance
                .Invoking(i => i.AddAttributes(Array.Empty<KeyValuePair<string, string>>()))
                .Should()
                .ThrowExactly<ArgumentNullException>();

            instance
                .Invoking(i => i.AddAttributes((Dictionary<string, string>)null))
                .Should()
                .ThrowExactly<ArgumentNullException>();

            instance
                .Invoking(i => i.AddAttributes(new Dictionary<string, string>()))
                .Should()
                .ThrowExactly<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void AddAttribute_InvalidAttribute_Throws(string nodeName)
        {
            var instance = Fb2ElementFactory.GetNodeByName(nodeName);
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            instance
                .Invoking(i => i.AddAttribute(() => new KeyValuePair<string, string>("testK", "")))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute(() => new KeyValuePair<string, string>("", "testV")))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute(new KeyValuePair<string, string>("testK", "")))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute(new KeyValuePair<string, string>("", "testV")))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute("testK", ""))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute("", "testV"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttributes(new("", ""), new("", "")))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttributes(new Dictionary<string, string> { { "testKey", "" }, { "", "testValue" } }))
                .Should()
                .ThrowExactly<InvalidAttributeException>();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void AddAttribute_NotAllowedAttribute_Throws(string nodeName)
        {
            var instance = Fb2ElementFactory.GetNodeByName(nodeName);
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            instance
                .Invoking(i => i.AddAttribute("NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<UnexpectedAtrributeException>();
        }

        //[Theory]
        //[ClassData(typeof(Fb2NodeNamesData))]
        //public void AddAttribute_AllowedAttribute_Works(string nodeName)
        //{
        //    var instance = Fb2ElementFactory.GetNodeByName(nodeName);
        //    instance.Should().NotBe(null);

        //    if (!instance.AllowedAttributes.Any())
        //        return;

        //    var firstAlowedAttributeName = instance.AllowedAttributes.First();

        //    instance.AddAttribute(firstAlowedAttributeName, "testValue");
        //}
    }
}

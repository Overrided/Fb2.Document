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
        [ClassData(typeof(Fb2NodeCollection))]
        public void EmptyNodes_EqualityNullTest(Fb2Node instance)
        {
            instance.Should().NotBe(null);

            var instanceTwo = Fb2NodeFactory.GetNodeByName(instance.Name);
            instanceTwo.Should().NotBe(null);

            instance.Should().Be(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void CloneNode_EqualityTest(Fb2Node instance)
        {
            var instanceTwo = Fb2NodeFactory.GetNodeByName(instance.Name);
            instance.Should().Be(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void Clone_AddAttribute_NotEquals(Fb2Node instance)
        {
            if (instance.AllowedAttributes == null || !instance.AllowedAttributes.Any())
                return;

            var instanceTwo = instance.Clone() as Fb2Node;
            //instanceTwo.AddAttribute(() => new KeyValuePair<string, string>(instanceOne.AllowedAttributes.First(), "testValue"));
            //instanceTwo.AddAttribute(new KeyValuePair<string, string>(instanceOne.AllowedAttributes.First(), "testValue"));
            instanceTwo.AddAttribute(instance.AllowedAttributes.First(), "testValue");

            instance.Should().NotBe(instanceTwo);

            instance.Attributes.Should().BeEmpty();
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
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_NoAttributesAllowed_Throws(Fb2Node instance)
        {
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
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_EmptyOrNull_Throws(Fb2Node instance)
        {
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
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_InvalidAttribute_Throws(Fb2Node instance)
        {
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
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_Whitespaces_Throws(Fb2Node instance)
        {
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            instance // whitespace
                .Invoking(i => i.AddAttribute("  NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // whitespace
                .Invoking(i => i.AddAttribute(" NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute('\t' + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute(Environment.NewLine + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute('\n' + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute('\r' + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // whitespace
                .Invoking(i => i.AddAttribute("NotExistingKey", "NotExistingValue "))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute("NotExistingKey", '\t' + "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance // /t 2, lol
                .Invoking(i => i.AddAttribute("NotExistingKey", Environment.NewLine + "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_NotAllowedAttribute_Throws(Fb2Node instance)
        {
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            instance
                .Invoking(i => i.AddAttribute("NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<UnexpectedAtrributeException>();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_AllowedAttribute_Works(Fb2Node instance)
        {
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            var firstAlowedAttributeName = instance.AllowedAttributes.First();

            instance.AddAttribute(firstAlowedAttributeName, "testValue");

            var attributes = instance.Attributes;
            attributes.Should().HaveCount(1);
            attributes.Should().ContainKey(firstAlowedAttributeName);
            attributes[firstAlowedAttributeName].Should().Be("testValue");
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void AddAttribute_AllowedAttribute_EscapesAttributeValue_Works(Fb2Node instance)
        {
            instance.Should().NotBe(null);

            if (!instance.AllowedAttributes.Any())
                return;

            var firstAlowedAttributeName = instance.AllowedAttributes.First();

            instance.AddAttribute(firstAlowedAttributeName, "<testValue");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "&lt;testValue");

            instance.AddAttribute(firstAlowedAttributeName, "testValue>");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&gt;");

            instance.AddAttribute(firstAlowedAttributeName, @"""testValue");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "&quot;testValue");

            instance.AddAttribute(firstAlowedAttributeName, "testValue'tv");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&apos;tv");

            instance.AddAttribute(firstAlowedAttributeName, "testValue&tv");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&amp;tv");

            instance.AddAttribute(firstAlowedAttributeName, @"<""testValue&tv'2"">");
            CheckAttributes(instance, 1, firstAlowedAttributeName, "&lt;&quot;testValue&amp;tv&apos;2&quot;&gt;");
        }

        private void CheckAttributes(
            Fb2Node instance,
            int expectedCount,
            string expectedName,
            string expectedValue)
        {
            instance.Should().NotBeNull();
            instance.Attributes.Should().HaveCount(expectedCount);
            instance.Attributes.Should().ContainKey(expectedName);
            instance.Attributes[expectedName].Should().Be(expectedValue);
        }
    }
}

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
            instanceTwo.AddAttribute(instance.AllowedAttributes.First(), "testValue");

            instance.Should().NotBe(instanceTwo);

            instance.Attributes.Should().BeEmpty();
            instanceTwo.Attributes.Should().NotBeEmpty().And.HaveCount(1);
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
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttribute(() => new KeyValuePair<string, string>("testKey", "testValue")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttribute(new KeyValuePair<string, string>("testKey", "testValue")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttributes(
                    new("testKey", "testValue"),
                    new("testKey2", "testValue2")))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");

            instance
                .Invoking(i => i.AddAttributes(
                    new Dictionary<string, string> {
                        { "testKey", "testValue" },
                        { "testKey2", "testValue2" }
                    }))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");

            instance
                .Invoking(async i => await i.AddAttributeAsync(() => Task.FromResult(new KeyValuePair<string, string>("testKey", "testValue"))))
                .Should()
                .ThrowExactly<NoAttributesAllowedException>()
                .WithMessage($"Node '{instance.Name}' has no allowed attributes.");
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

            instance
                .Invoking(i => i.AddAttribute('\t' + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute(Environment.NewLine + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute('\n' + "NotExistingKey", "NotExistingValue"))
                .Should()
                .ThrowExactly<InvalidAttributeException>();

            instance
                .Invoking(i => i.AddAttribute('\r' + "NotExistingKey", "NotExistingValue"))
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
            attributes.Should().HaveCount(1).And.ContainKey(firstAlowedAttributeName);
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

            instance.AddAttribute(firstAlowedAttributeName, " test value with whitespace ");
            CheckAttributes(instance, 1, firstAlowedAttributeName, " test value with whitespace ");
        }

        [Fact]
        public void Fb2Node_RemoveAttributes_ByAttributeName_CaseSensitive()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            sequenceInfo.RemoveAttribute("NamE");
            sequenceInfo.RemoveAttribute("NUmBeR");
            sequenceInfo.RemoveAttribute("lAnG");

            sequenceInfo.Attributes.Should().HaveCount(3);

            sequenceInfo.RemoveAttribute(AttributeNames.Name);
            sequenceInfo.Attributes.Should().HaveCount(2).And.NotContainKey(AttributeNames.Name);

            sequenceInfo.RemoveAttribute(AttributeNames.Number);
            sequenceInfo.Attributes.Should().HaveCount(1).And.NotContainKey(AttributeNames.Name, AttributeNames.Number);

            sequenceInfo.RemoveAttribute(AttributeNames.Language);
            sequenceInfo.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void Fb2Node_RemoveAttributes_ByAttributeName_CaseInSensitive()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            sequenceInfo.RemoveAttribute("NamE", true);
            sequenceInfo.Attributes.Should().HaveCount(2).And.NotContainKey(AttributeNames.Name);

            sequenceInfo.RemoveAttribute("NUmBeR", true);
            sequenceInfo.Attributes.Should().HaveCount(1).And.NotContainKey(AttributeNames.Name, AttributeNames.Number);

            sequenceInfo.RemoveAttribute("lAnG", true);
            sequenceInfo.Attributes.Should().BeEmpty();
        }

        [Fact]
        public void Fb2Node_RemoveAttributes_ByPredicate()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            Func<KeyValuePair<string, string>, bool> nameWrongCasePredicate = (kvp) => kvp.Key.Equals("nAMe");
            Func<KeyValuePair<string, string>, bool> nameAttributePredicate = (kvp) => kvp.Key.Equals(AttributeNames.Name);

            sequenceInfo.RemoveAttribute(nameWrongCasePredicate);
            sequenceInfo.Attributes.Should().HaveCount(3);

            sequenceInfo.RemoveAttribute(nameAttributePredicate);
            sequenceInfo.Attributes.Should().HaveCount(2).And.NotContainKey(AttributeNames.Name);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void EmptyNode_HasAttribute_ReturnsFalse(Fb2Node instance)
        {
            if (instance.AllowedAttributes.Count == 0)
                return;

            instance.Attributes.Should().BeEmpty();

            instance.HasAttribute(instance.AllowedAttributes.First()).Should().BeFalse();
            instance.HasAttribute(instance.AllowedAttributes.First(), "blah").Should().BeFalse();

            instance.HasAttribute(instance.AllowedAttributes.First(), true).Should().BeFalse();
            instance.HasAttribute(instance.AllowedAttributes.First(), "blah", true).Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void EmptyNode_GetAttribute_ReturnsDefaultKeyValuePair(Fb2Node instance)
        {
            if (instance.AllowedAttributes.Count == 0)
                return;

            instance.Attributes.Should().BeEmpty();

            instance.GetAttribute(instance.AllowedAttributes.First()).Should().Be(default(KeyValuePair<string, string>));
            instance.GetAttribute(instance.AllowedAttributes.First(), true).Should().Be(default(KeyValuePair<string, string>));
        }

        [Theory]
        [ClassData(typeof(Fb2NodeCollection))]
        public void EmptyNode_TryGetAttribute_ReturnsFalse(Fb2Node instance)
        {
            if (instance.AllowedAttributes.Count == 0)
                return;

            instance.Attributes.Should().BeEmpty();

            instance.TryGetAttribute(instance.AllowedAttributes.First(), out var result)
                .Should()
                .BeFalse();

            result.Key.Should().BeNullOrEmpty();
            result.Value.Should().BeNullOrEmpty();

            instance.TryGetAttribute(instance.AllowedAttributes.First(), out var resultIgnoreCase, true)
                .Should()
                .BeFalse();

            resultIgnoreCase.Key.Should().BeNullOrEmpty();
            resultIgnoreCase.Value.Should().BeNullOrEmpty();
        }

        [Fact]
        public void SequenceInfoNode_HasAttribute_Works()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            // query

            var hasNameAttribute = sequenceInfo.HasAttribute(AttributeNames.Name);
            hasNameAttribute.Should().BeTrue();

            var hasNameAttributeWrongCasing = sequenceInfo.HasAttribute("NaMe");
            hasNameAttributeWrongCasing.Should().BeFalse();

            var hasNameAttributeIgnoringCasing = sequenceInfo.HasAttribute("NaMe", true);
            hasNameAttributeIgnoringCasing.Should().BeTrue();

            var hasNameAttributeKeyWrongCasing = sequenceInfo.HasAttribute("NaMe", "Test Sequence");
            hasNameAttributeKeyWrongCasing.Should().BeFalse();

            var hasNameAttributeValueWrongCasing = sequenceInfo.HasAttribute(AttributeNames.Name, "TeSt SeQuEnCe");
            hasNameAttributeValueWrongCasing.Should().BeFalse();

            var hasNameAttributeKeyWrongCasingIgnore = sequenceInfo.HasAttribute("NaMe", "Test Sequence", true);
            hasNameAttributeKeyWrongCasingIgnore.Should().BeTrue();

            var hasNameAttributeValueWrongCasingIgnore = sequenceInfo.HasAttribute(AttributeNames.Name, "TeSt SeQuEnCe", true);
            hasNameAttributeValueWrongCasingIgnore.Should().BeTrue();

            sequenceInfo.HasAttribute(AttributeNames.Name, "Test Sequence").Should().BeTrue();
            sequenceInfo.HasAttribute(AttributeNames.Number, "1").Should().BeTrue();
            sequenceInfo.HasAttribute(AttributeNames.Language, "eng").Should().BeTrue();
        }

        [Fact]
        public void SequenceInfoNode_GetAttribute_Works()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            // query

            sequenceInfo.GetAttribute(AttributeNames.Name)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"));
            sequenceInfo.GetAttribute(AttributeNames.Number)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Number, "1"));
            sequenceInfo.GetAttribute(AttributeNames.Language)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.GetAttribute("NaMe").Should().Be(default(KeyValuePair<string, string>));
            sequenceInfo.GetAttribute("NuMbEr").Should().Be(default(KeyValuePair<string, string>));
            sequenceInfo.GetAttribute("LaNg").Should().Be(default(KeyValuePair<string, string>));

            sequenceInfo.GetAttribute("NaMe", true)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"));
            sequenceInfo.GetAttribute("NuMbEr", true)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Number, "1"));
            sequenceInfo.GetAttribute("LaNg", true)
                .Should()
                .Be(new KeyValuePair<string, string>(AttributeNames.Language, "eng"));
        }

        [Fact]
        public void SequenceInfoNode_TryGetAttribute_Works()
        {
            // setup
            var sequenceInfo = new SequenceInfo();
            sequenceInfo.AllowedAttributes.Should().HaveCount(3);
            sequenceInfo.Attributes.Should().BeEmpty();

            sequenceInfo
                .AddAttributes(
                    new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"),
                    new KeyValuePair<string, string>(AttributeNames.Number, "1"))
                .AddAttribute(() => new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            sequenceInfo.Attributes.Should().HaveCount(3);

            // query

            // case sensitive
            sequenceInfo.TryGetAttribute(AttributeNames.Name, out var nameResult)
                .Should()
                .BeTrue();
            nameResult.Should().Be(new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"));

            sequenceInfo.TryGetAttribute(AttributeNames.Number, out var numberResult)
                .Should()
                .BeTrue();
            numberResult.Should().Be(new KeyValuePair<string, string>(AttributeNames.Number, "1"));

            sequenceInfo.TryGetAttribute(AttributeNames.Language, out var langResult)
                .Should()
                .BeTrue();
            langResult.Should().Be(new KeyValuePair<string, string>(AttributeNames.Language, "eng"));

            // case sensitive, wrong key casing
            sequenceInfo.TryGetAttribute("NaMe", out var nameResultInvalidCasing)
                .Should()
                .BeFalse();
            nameResultInvalidCasing.Should().Be(default(KeyValuePair<string, string>));

            sequenceInfo.TryGetAttribute("NuMbEr", out var numberResultInvalidCasing)
                .Should()
                .BeFalse();
            numberResultInvalidCasing.Should().Be(default(KeyValuePair<string, string>));

            sequenceInfo.TryGetAttribute("LaNg", out var langResultIvalidCasing)
                .Should()
                .BeFalse();
            langResultIvalidCasing.Should().Be(default(KeyValuePair<string, string>));

            // case in-sensitive, wrong key casing
            sequenceInfo.TryGetAttribute("NaMe", out var nameResultInvalidCasingIgnore, true)
                .Should()
                .BeTrue();
            nameResultInvalidCasingIgnore.Should().Be(new KeyValuePair<string, string>(AttributeNames.Name, "Test Sequence"));

            sequenceInfo.TryGetAttribute("NuMbEr", out var numberResultInvalidCasingIgnore, true)
                .Should()
                .BeTrue();
            numberResultInvalidCasingIgnore.Should().Be(new KeyValuePair<string, string>(AttributeNames.Number, "1"));

            sequenceInfo.TryGetAttribute("LaNg", out var langResultIvalidCasingIgnore, true)
                .Should()
                .BeTrue();
            langResultIvalidCasingIgnore.Should().Be(new KeyValuePair<string, string>(AttributeNames.Language, "eng"));
        }

        private static void CheckAttributes(
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

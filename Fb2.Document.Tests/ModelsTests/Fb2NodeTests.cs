using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

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
        instance.Equals(new object()).Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void CloneNode_EqualityTest(Fb2Node instance)
    {
        var instance2 = instance.Clone();
        var instance3 = Fb2NodeFactory.GetNodeByName(instance.Name);
        instance.Should().Be(instance2);
        instance.Should().Be(instance3);
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void Clone_AddAttribute_NotEquals(Fb2Node instance)
    {
        if (instance.AllowedAttributes is not { Count: > 0 })
            return;

        var instanceTwo = instance.Clone() as Fb2Node;
        instanceTwo!.AddAttribute(new Fb2Attribute(instance.AllowedAttributes.First(), "testValue"));

        instance.Should().NotBe(instanceTwo);

        instance.Attributes.Should().BeEmpty();
        instanceTwo.Attributes.Should().NotBeEmpty().And.HaveCount(1);
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void Clone_Node_WithAttributes_AndMetadata(Fb2Node instance)
    {
        if (instance.AllowedAttributes is not { Count: > 0 })
            return;

        instance.AddAttribute(new Fb2Attribute(instance.AllowedAttributes.First(), "testValue"));
        instance.NodeMetadata = new Fb2NodeMetadata(XNamespace.Xml);

        instance.HasAttributes.Should().BeTrue();
        instance.Attributes.Should().HaveCount(1);
        instance.NodeMetadata.Should().NotBeNull();

        var instanceTwo = instance.Clone() as Fb2Node;

        instanceTwo.Should().NotBeNull();
        instanceTwo!.HasAttributes.Should().BeTrue();
        instanceTwo.Attributes.Should().HaveCount(1);
        instanceTwo.NodeMetadata.Should().NotBeNull();

        instance.Should().Be(instanceTwo);
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

        paragOne.AddAttribute(new Fb2Attribute(AttributeNames.Id, "testId"));
        paragOne.Should().NotBe(paragTwo);

        paragTwo.AddAttribute(new Fb2Attribute(AttributeNames.Id, "testId"));
        paragOne.Should().Be(paragTwo);
    }

    [Fact]
    public void Load_WithDuplicateAttributes_Works()
    {
        var paragraphString = "<p id=\"p_1\" ID=\"p_2\" iD=\"p_3\" Id=\"p_3\" lang=\"ua\" Lang=\"ua1\" lAng=\"ua2\" laNg=\"ua3\" lanG=\"ua4\" LAng=\"ua5\">test text</p>";

        var document = XDocument.Parse(paragraphString);
        var xNode = document.FirstNode;

        var paragraphNode = new Paragraph();
        paragraphNode.Load(xNode!);

        paragraphNode.HasContent.Should().BeTrue();
        paragraphNode.Content.Count.Should().Be(1);

        paragraphNode.HasAttributes.Should().BeTrue();
        paragraphNode.Attributes.Should().HaveCount(2);
        var idAttr = paragraphNode
            .Attributes
            .FirstOrDefault(a => a.Key.Equals(AttributeNames.Id));

        idAttr.Should().NotBeNull();
        idAttr!.Key.Should().Be(AttributeNames.Id);
        idAttr!.Value.Should().Be("p_1");

        var langAttr = paragraphNode
            .Attributes
            .FirstOrDefault(a => a.Key.Equals(AttributeNames.Language));

        langAttr.Should().NotBeNull();
        langAttr!.Key.Should().Be(AttributeNames.Language);
        langAttr!.Value.Should().Be("ua");
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public async Task AddAttribute_NoAttributesAllowed_Throws(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute("testKey", "testValue")))
            .Should()
            .ThrowExactly<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes.")
            .And.NodeName
            .Should()
            .Be(instance.Name);

        instance
            .Invoking(i => i.AddAttribute(() => new Fb2Attribute("testKey", "testValue")))
            .Should()
            .ThrowExactly<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes.")
            .And.NodeName
            .Should()
            .Be(instance.Name);

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute("testKey", "testValue")))
            .Should()
            .ThrowExactly<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes.")
            .And.NodeName
            .Should()
            .Be(instance.Name); ;

        instance
            .Invoking(i => i.AddAttributes(
                new Fb2Attribute("testKey", "testValue"),
                new Fb2Attribute("testKey2", "testValue2")))
            .Should()
            .ThrowExactly<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes.")
            .And.NodeName
            .Should()
            .Be(instance.Name); ;

        instance
            .Invoking(i => i.AddAttributes(
                [
                    new("testKey", "testValue"),
                    new("testKey2", "testValue2")
                ]))
            .Should()
            .ThrowExactly<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes.")
            .And.NodeName
            .Should()
            .Be(instance.Name);

        (await instance
            .Invoking(async i => await i.AddAttributeAsync(() => Task.FromResult(new Fb2Attribute("testKey", "testValue"))))
            .Should()
            .ThrowExactlyAsync<NoAttributesAllowedException>()
            .WithMessage($"Node '{instance.Name}' has no allowed attributes."))
            .And.NodeName
            .Should()
            .Be(instance.Name);
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_EmptyOrNull_Throws(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.AddAttribute((Fb2Attribute)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.AddAttribute((Func<Fb2Attribute>)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.AddAttributeAsync(null!))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();

        instance
            .Invoking(i => i.AddAttributes())
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.AddAttributes([]))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.AddAttributes((List<Fb2Attribute>)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_InvalidAttribute_Throws(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.AddAttribute(() => new Fb2Attribute("", "testV")))
            .Should()
            .ThrowExactly<InvalidAttributeException>();


        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute("", "testV")))
            .Should()
            .ThrowExactly<InvalidAttributeException>();

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute("", "testV")))
            .Should()
            .ThrowExactly<InvalidAttributeException>();

        instance
            .Invoking(i => i.AddAttributes(new Fb2Attribute("", ""), new Fb2Attribute("", "")))
            .Should()
            .ThrowExactly<InvalidAttributeException>();

        instance
            .Invoking(i => i.AddAttributes([
                    new("testKey", ""),
                    new( "", "testValue" )]))
            .Should()
            .ThrowExactly<InvalidAttributeException>()
            .And.AttributeKey.Should().Be("");
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_Whitespaces_Throws(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        var exc = instance // whitespace
            .Invoking(i => i.AddAttribute(new Fb2Attribute("  NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();

        exc.Which.NodeName.Should().Be(instance.Name);
        exc.Which.AttributeName.Should().Be("  NotExistingKey");

        instance // whitespace
            .Invoking(i => i.AddAttribute(new Fb2Attribute(" NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute('\t' + "NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute(Environment.NewLine + "NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute('\n' + "NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute('\r' + "NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_NotAllowedAttribute_Throws(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.AddAttribute(new Fb2Attribute("NotExistingKey", "NotExistingValue")))
            .Should()
            .ThrowExactly<UnexpectedAttributeException>();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_AllowedAttribute_Works(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        var firstAlowedAttributeName = instance.AllowedAttributes!.First();

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, "testValue"));

        var attributes = instance.Attributes;
        attributes.Should().HaveCount(1).And.Contain((attr) => attr.Key == firstAlowedAttributeName);
        attributes.First().Value.Should().Be("testValue");
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void AddAttribute_AllowedAttribute_EscapesAttributeValue_Works(Fb2Node instance)
    {
        instance.Should().NotBe(null);

        if (!instance.HasAllowedAttributes)
            return;

        var firstAlowedAttributeName = instance.AllowedAttributes!.First();

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, "<testValue"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "&lt;testValue");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, "testValue>"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&gt;");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, @"""testValue"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "&quot;testValue");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, "testValue'tv"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&apos;tv");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, "testValue&tv"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "testValue&amp;tv");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, @"<""testValue&tv'2"">"));
        CheckAttributes(instance, 1, firstAlowedAttributeName, "&lt;&quot;testValue&amp;tv&apos;2&quot;&gt;");

        instance.AddAttribute(new Fb2Attribute(firstAlowedAttributeName, " test value with whitespace "));
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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        // shouldnt remove stuff, no matches
        sequenceInfo.RemoveAttribute("NamE");
        sequenceInfo.RemoveAttribute("NUmBeR");
        sequenceInfo.RemoveAttribute("lAnG");

        sequenceInfo.Attributes.Should().HaveCount(3);

        sequenceInfo.RemoveAttribute(AttributeNames.Name);
        sequenceInfo.Attributes.Should().HaveCount(2).And.NotContain((attr) => attr.Key == AttributeNames.Name);

        sequenceInfo.RemoveAttribute(AttributeNames.Number);
        sequenceInfo.Attributes.Should().HaveCount(1).And.NotContain((attr) => attr.Key == AttributeNames.Name || attr.Key == AttributeNames.Number);

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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        sequenceInfo.RemoveAttribute("NamE", true);
        sequenceInfo.Attributes.Should().HaveCount(2).And.NotContain((attr) => attr.Key == AttributeNames.Name);

        sequenceInfo.RemoveAttribute("NUmBeR", true);
        sequenceInfo.Attributes.Should().HaveCount(1).And.NotContain((attr) => attr.Key == AttributeNames.Name || attr.Key == AttributeNames.Number);

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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        Func<Fb2Attribute, bool> nameWrongCasePredicate = (kvp) => kvp.Key.Equals("nAMe");
        Func<Fb2Attribute, bool> nameAttributePredicate = (kvp) => kvp.Key.Equals(AttributeNames.Name);

        sequenceInfo.RemoveAttribute(nameWrongCasePredicate); // not removing anything, no matches
        sequenceInfo.Attributes.Should().HaveCount(3);

        sequenceInfo.RemoveAttribute(nameAttributePredicate);
        sequenceInfo.Attributes.Should().HaveCount(2).And.NotContain((attr) => attr.Key == AttributeNames.Name);
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void Fb2Node_RemoveAttribute_InvalidAttribute_Fails(Fb2Node instance)
    {
        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.RemoveAttribute((Fb2Attribute)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName("fb2Attribute");

        instance
            .Invoking(i => i.RemoveAttribute((Func<Fb2Attribute, bool>)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName("attributePredicate");

        instance
            .Invoking(i => i.RemoveAttribute((string)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName("key");

        instance
            .Invoking(i => i.RemoveAttribute(string.Empty))
            .Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName("key");
    }


    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void Fb2Node_EmptyNode_RemoveAttribute_Ignored(Fb2Node instance)
    {
        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.RemoveAttribute(instance.AllowedAttributes!.First()))
            .Should()
            .NotThrow();

        instance
            .Invoking(i => i.RemoveAttribute((a) => a.Key == instance.AllowedAttributes!.First()))
            .Should()
            .NotThrow();

        instance
            .Invoking(i => i.RemoveAttribute(new Fb2Attribute(instance.AllowedAttributes!.First(), "test")))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void Fb2Node_HasAttributes_ClearAttribute_Works()
    {
        // setup
        var sequenceInfo = new SequenceInfo();
        sequenceInfo.AllowedAttributes.Should().HaveCount(3);
        sequenceInfo.Attributes.Should().BeEmpty();
        sequenceInfo.HasAttributes.Should().BeFalse();

        sequenceInfo
            .AddAttributes(
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.HasAttributes.Should().BeTrue();
        sequenceInfo.Attributes.Should().HaveCount(3);

        sequenceInfo.ClearAttributes();
        sequenceInfo.Attributes.Should().BeEmpty();
        sequenceInfo.HasAttributes.Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void Fb2Node_HasAttribute_NullAttribute_Throws(Fb2Node instance)
    {
        instance.HasAttributes.Should().BeFalse();

        if (!instance.HasAllowedAttributes)
            return;

        instance
            .Invoking(i => i.HasAttribute((Fb2Attribute)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.HasAttribute((string)null!))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        instance
            .Invoking(i => i.HasAttribute(string.Empty))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void EmptyNode_HasAttribute_ReturnsFalse(Fb2Node instance)
    {
        instance.HasAttributes.Should().BeFalse();

        if (!instance.HasAllowedAttributes)
            return;

        instance.Attributes.Should().BeEmpty();

        instance.HasAttribute(instance.AllowedAttributes!.First()).Should().BeFalse();
        instance.HasAttribute(new Fb2Attribute(instance.AllowedAttributes!.First(), "blah")).Should().BeFalse();

        instance.HasAttribute(instance.AllowedAttributes!.First(), true).Should().BeFalse();
        instance.HasAttribute(new Fb2Attribute(instance.AllowedAttributes!.First(), "blah", "http://www.w3.org//xlink")).Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void EmptyNode_GetAttribute_ReturnsDefaultKeyValuePair(Fb2Node instance)
    {
        if (!instance.HasAllowedAttributes)
            return;

        instance.Attributes.Should().BeEmpty();

        instance.GetAttribute(instance.AllowedAttributes!.First()).Should().BeNull();
        instance.GetAttribute(instance.AllowedAttributes!.First(), true).Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void EmptyNode_TryGetAttribute_ReturnsFalse(Fb2Node instance)
    {
        if (!instance.HasAllowedAttributes)
            return;

        instance.Attributes.Should().BeEmpty();

        instance.TryGetAttribute(instance.AllowedAttributes!.First(), out var result)
            .Should()
            .BeFalse();

        result.Should().BeNull();

        instance.TryGetAttribute(instance.AllowedAttributes!.First(), true, out var resultIgnoreCase)
            .Should()
            .BeFalse();

        resultIgnoreCase.Should().BeNull();

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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        // query

        var hasNameAttribute = sequenceInfo.HasAttribute(AttributeNames.Name);
        hasNameAttribute.Should().BeTrue();

        var hasNameAttributeWrongCasing = sequenceInfo.HasAttribute("NaMe");
        hasNameAttributeWrongCasing.Should().BeFalse();

        var hasNameAttributeIgnoringCasing = sequenceInfo.HasAttribute("NaMe", true);
        hasNameAttributeIgnoringCasing.Should().BeTrue();

        var hasNameAttributeKeyWrongCasing = sequenceInfo.HasAttribute(new Fb2Attribute("NaMe", "Test Sequence"));
        hasNameAttributeKeyWrongCasing.Should().BeTrue(); // default key case-insensitive comparison

        var hasNameAttributeValueWrongCasing = sequenceInfo.HasAttribute(new Fb2Attribute(AttributeNames.Name, "TeSt SeQuEnCe"));
        hasNameAttributeValueWrongCasing.Should().BeFalse();

        sequenceInfo.HasAttribute(new Fb2Attribute(AttributeNames.Name, "Test Sequence")).Should().BeTrue();
        sequenceInfo.HasAttribute(new Fb2Attribute(AttributeNames.Number, "1")).Should().BeTrue();
        sequenceInfo.HasAttribute(new Fb2Attribute(AttributeNames.Language, "eng")).Should().BeTrue();
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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        // query

        var expectedAttr = new Fb2Attribute(AttributeNames.Name, "Test Sequence");

        sequenceInfo.GetAttribute(AttributeNames.Name)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Name, "Test Sequence"));

        sequenceInfo.GetAttribute(AttributeNames.Number)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Number, "1"));


        sequenceInfo.GetAttribute(AttributeNames.Language)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Language, "eng"));


        sequenceInfo.GetAttribute("NaMe").Should().Be(null);
        sequenceInfo.GetAttribute("NuMbEr").Should().Be(null);
        sequenceInfo.GetAttribute("LaNg").Should().Be(null);

        sequenceInfo.GetAttribute("NaMe", true)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Name, "Test Sequence"));
        sequenceInfo.GetAttribute("NuMbEr", true)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Number, "1"));
        sequenceInfo.GetAttribute("LaNg", true)
            .Should()
            .Be(new Fb2Attribute(AttributeNames.Language, "eng"));
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
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AddAttribute(() => new Fb2Attribute(AttributeNames.Language, "eng"));

        sequenceInfo.Attributes.Should().HaveCount(3);

        // query

        // case sensitive
        sequenceInfo.TryGetAttribute(AttributeNames.Name, out var nameResult)
            .Should()
            .BeTrue();
        nameResult.Should().Be(new Fb2Attribute(AttributeNames.Name, "Test Sequence"));

        sequenceInfo.TryGetAttribute(AttributeNames.Number, out var numberResult)
            .Should()
            .BeTrue();
        numberResult.Should().Be(new Fb2Attribute(AttributeNames.Number, "1"));

        sequenceInfo.TryGetAttribute(AttributeNames.Language, out var langResult)
            .Should()
            .BeTrue();
        langResult.Should().Be(new Fb2Attribute(AttributeNames.Language, "eng"));

        // case sensitive, wrong key casing
        sequenceInfo.TryGetAttribute("NaMe", out var nameResultInvalidCasing)
            .Should()
            .BeFalse();

        nameResultInvalidCasing.Should().Be(null);

        sequenceInfo.TryGetAttribute("NuMbEr", out var numberResultInvalidCasing)
            .Should()
            .BeFalse();

        numberResultInvalidCasing.Should().Be(null);

        sequenceInfo.TryGetAttribute("LaNg", out var langResultIvalidCasing)
            .Should()
            .BeFalse();

        langResultIvalidCasing.Should().Be(null);

        // case in-sensitive, wrong key casing
        sequenceInfo.TryGetAttribute("NaMe", true, out var nameResultInvalidCasingIgnore)
            .Should()
            .BeTrue();
        nameResultInvalidCasingIgnore.Should().Be(new Fb2Attribute(AttributeNames.Name, "Test Sequence"));

        sequenceInfo.TryGetAttribute("NuMbEr", true, out var numberResultInvalidCasingIgnore)
            .Should()
            .BeTrue();
        numberResultInvalidCasingIgnore.Should().Be(new Fb2Attribute(AttributeNames.Number, "1"));

        sequenceInfo.TryGetAttribute("LaNg", true, out var langResultIvalidCasingIgnore)
            .Should()
            .BeTrue();
        langResultIvalidCasingIgnore.Should().Be(new Fb2Attribute(AttributeNames.Language, "eng"));
    }

    [Fact]
    public void SequenceInfoNode_AppendAttribute_Works()
    {
        // setup
        var sequenceInfo = new SequenceInfo();
        sequenceInfo.AllowedAttributes.Should().HaveCount(3);
        sequenceInfo.Attributes.Should().BeEmpty();

        sequenceInfo
            .AddAttributes(
                new Fb2Attribute(AttributeNames.Name, "Test Sequence"),
                new Fb2Attribute(AttributeNames.Number, "1"))
            .AppendAttribute(AttributeNames.Language, "eng");

        sequenceInfo.Attributes.Should().HaveCount(3);

        // query
        sequenceInfo.TryGetAttribute(AttributeNames.Name, out var nameResult)
            .Should()
            .BeTrue();
        nameResult.Should().Be(new Fb2Attribute(AttributeNames.Name, "Test Sequence"));

        sequenceInfo.TryGetAttribute(AttributeNames.Number, out var numberResult)
            .Should()
            .BeTrue();
        numberResult.Should().Be(new Fb2Attribute(AttributeNames.Number, "1"));

        sequenceInfo.TryGetAttribute(AttributeNames.Language, out var langResult)
            .Should()
            .BeTrue();
        langResult.Should().Be(new Fb2Attribute(AttributeNames.Language, "eng"));
    }

    [Fact]
    public void Ancestors_Works()
    {
        var parent1 = new Strong().AppendTextContent("leaf node text");
        parent1.Content.Should().HaveCount(1);

        var leafText = parent1.Content.First();
        leafText.Should().NotBeNull();
        leafText.Should().BeOfType<TextItem>();
        (leafText as TextItem)!.Content.Should().Be("leaf node text");

        parent1.Parent.Should().BeNull();
        parent1.GetAncestors().Should().NotBeNull().And.BeEmpty();
        leafText.Parent.Should().Be(parent1);
        var initialAncestors = leafText.GetAncestors();
        initialAncestors.Should().HaveCount(1);
        initialAncestors.Should().Contain(parent1);

        var parent2 = new Emphasis().AppendContent(parent1);
        var updatedAncestors = leafText.GetAncestors();
        updatedAncestors.Should().HaveCount(2);
        updatedAncestors.Should().Contain(parent1);
        updatedAncestors.Should().Contain(parent2);
    }

    private static void CheckAttributes(
        Fb2Node instance,
        int expectedCount,
        string expectedName,
        string expectedValue)
    {
        instance.Should().NotBeNull();
        instance!.Attributes.Should().HaveCount(expectedCount);
        instance.Attributes.Should().Contain((attr) => attr.Key == expectedName);
        instance!.GetAttribute(expectedName)!.Value.Should().Be(expectedValue);
    }
}

﻿using System;
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
using Fb2.Document.Tests.Common;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests;

public class Fb2ContainerTests
{
    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_AddContent_NullNode_Throws(Fb2Container node)
    {
        node.Should().NotBeNull();
        var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

        node.Invoking(n => n.AddContent((Fb2Node?)null)) // Fb2Node 
           .Should()
           .Throw<ArgumentNullException>();

        //string nodeName
        node.Invoking(n => n.AddContent("")).Should().ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddContent(string.Empty)).Should().ThrowExactly<ArgumentNullException>();

        //string nodeName
        node.Invoking(n => n.AddContent((string?)null)).Should().ThrowExactly<ArgumentNullException>();

        // params Fb2Node[] nodes
        node.Invoking(n => n.AddContent()).Should().ThrowExactly<ArgumentNullException>();

        // params Fb2Node[] nodes
        node.Invoking(n => n.AddContent(null, null)) // lol
            .Should()
            .Throw<ArgumentNullException>();

        node.Invoking(n => n.AddContent(() => null)) // Func<Fb2Node>
            .Should()
            .Throw<ArgumentNullException>();

        node.Invoking(n => n.AddContent((Func<Fb2Node>)null)) // Func<Fb2Node>
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddContent((List<Fb2Node>)null)) // IEnumerable<Fb2Node>
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
            .Should()
            .Throw<ArgumentNullException>();

        node.Invoking(n => n.AddContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
            .Should()
            .Throw<ArgumentNullException>();

        node.Invoking(async n => await n.AddContentAsync(
            async () => await Task.FromResult<Fb2Node>(null))) // async node provider
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();

        node.Invoking(async n => await n.AddContentAsync(null))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();
    }

    // just for lulz
    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_AddUnknownNode_Throws(Fb2Container node)
    {
        var impostor = new ImpostorNode();

        node.Invoking(n => n.AddContent(impostor)) // Fb2Node 
           .Should()
           .ThrowExactly<InvalidNodeException>()
           .WithMessage($"'Impostor' is not known Fb2 node name.");

        node.Invoking(n => n.AddContent(impostor.Name)) // name
           .Should()
           .ThrowExactly<InvalidNodeException>()
           .WithMessage($"'Impostor' is not known Fb2 node name.");

        var sneakyImpostor = new ImpostorNode(ElementNames.Paragraph);

        node.Invoking(n => n.AddContent(sneakyImpostor)) // Fb2Node 
           .Should()
           .ThrowExactly<InvalidNodeException>()
           .WithMessage($"'{sneakyImpostor.Name}' is not known Fb2 node name.");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_AddContent_ByInvalidName_Throws(Fb2Container node)
    {
        node.Invoking(n => n.AddContent("impostorNodeName"))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .WithMessage("'impostorNodeName' is not known Fb2 node name.");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public async Task Container_CantContainText_AddTextContent_Throws(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (node.CanContainText)
            return;

        node.Invoking(n => n.AddContent(new TextItem().AddContent("test text")))
            .Should()
            .ThrowExactly<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain 'text'.");

        node.Invoking(n => n.AddTextContent("test text"))
            .Should()
            .ThrowExactly<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain 'text'.");

        node.Invoking(n => n.AddTextContent(() => "test text"))
            .Should()
            .ThrowExactly<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain 'text'.");

        await node
            .Invoking(async n => await n.AddTextContentAsync(async () =>
            {
                await Task.Delay(5);
                return "test text";
            }))
            .Should()
            .ThrowExactlyAsync<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain 'text'.");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public async Task Container_CanContainText_AddTextContent_NullContent_Throws(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.Invoking(n => n.AddContent(new TextItem().AddContent((string)null)))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddTextContent((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddTextContent(() => null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.AddTextContent((Func<string>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        await node.Invoking(n => n.AddTextContentAsync(null))
             .Should()
             .ThrowExactlyAsync<ArgumentNullException>();

        await node
            .Invoking(async n => await n.AddTextContentAsync(async () =>
            {
                await Task.Delay(5);
                return null;
            }))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public async Task Container_CanContainText_AddTextContent_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.AddContent(new TextItem().AddContent("test text"));

        node.Content.Count.Should().Be(1);
        var first = node.Content.First();
        first.Should().BeOfType(typeof(TextItem));
        (first as Fb2Element).Content.Should().Be("test text");

        ClearContainerContent(node);

        node.AddTextContent("test text");

        node.Content.Count.Should().Be(1);
        var second = node.Content.First();
        second.Should().BeOfType(typeof(TextItem));
        (second as Fb2Element).Content.Should().Be("test text");

        ClearContainerContent(node);

        node.AddTextContent(() => "test text");

        node.Content.Count.Should().Be(1);
        var third = node.Content.First();
        third.Should().BeOfType(typeof(TextItem));
        (third as Fb2Element).Content.Should().Be("test text");

        ClearContainerContent(node);

        await node.AddTextContentAsync(async () =>
            {
                await Task.Delay(5);
                return "test text";
            });

        node.Content.Count.Should().Be(1);
        var forths = node.Content.First();
        forths.Should().BeOfType(typeof(TextItem));
        (forths as Fb2Element).Content.Should().Be("test text");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_CanContainText_AddMultipleTextNodes_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.AddContent(new TextItem().AddContent("test text"));

        node.Content.Count.Should().Be(1);
        var first = node.GetFirstChild<TextItem>();
        first.Should().NotBeNull();
        first.Content.Should().Be("test text");

        node.AddContent(new TextItem().AddContent(" test text 2 "));

        node.Content.Count.Should().Be(1);
        var second = node.GetFirstChild<TextItem>();
        second.Should().NotBeNull();
        second.Content.Should().Be("test text test text 2 ");
        first.Content.Should().Be("test text test text 2 ");

        node.AddContent(new TextItem().AddContent("test text 3 "));

        node.Content.Count.Should().Be(1);
        second.Content.Should().Be("test text test text 2 test text 3 ");
        first.Content.Should().Be("test text test text 2 test text 3 ");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_CanContainText_AddMultipleTextNodes_WithContainers_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.AddContent(new TextItem().AddContent("test text"));

        node.Content.Count.Should().Be(1);
        node.Parent.Should().BeNull();
        var first = node.GetFirstChild<TextItem>()!;
        first.Should().NotBeNull();
        first.Content.Should().Be("test text");
        first.Parent.Should().NotBeNull().And.Be(node);

        node.AddContent(new TextItem().AddContent(" test text 2 "));

        node.Content.Count.Should().Be(1);
        var second = node.GetFirstChild<TextItem>()!;
        second.Should().NotBeNull();
        second.Content.Should().Be("test text test text 2 ");
        first.Content.Should().Be("test text test text 2 ");

        node.AddContent(node.AllowedElements.First());
        var firstAllowedElement = node.GetFirstChild(node.AllowedElements.First())!;
        firstAllowedElement.Should().NotBeNull();
        firstAllowedElement.Parent.Should().NotBeNull().And.Be(node);

        node.Content.Count.Should().Be(2);

        node.AddContent(new TextItem().AddContent("test text 3 "));

        node.Content.Count.Should().Be(3);
        var textItems = node.GetChildren<TextItem>().ToList();
        textItems.Count.Should().Be(2);
        textItems.First().Content.Should().Be("test text test text 2 ");
        textItems.Last().Parent.Should().NotBeNull().And.Be(node);
        textItems.Last().Content.Should().Be("test text 3 ");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_CanContainText_AddMultipleTextContent_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.AddTextContent("test text");

        node.Content.Count.Should().Be(1);
        var first = node.GetFirstChild<TextItem>();
        first.Should().NotBeNull();
        first.Content.Should().Be("test text");

        node.AddTextContent("test text 2", " ");

        node.Content.Count.Should().Be(1);
        var second = node.GetFirstChild<TextItem>();
        second.Should().NotBeNull();
        second.Content.Should().Be("test text test text 2");
        first.Content.Should().Be("test text test text 2");

        node.AddTextContent("test text 3", " ");

        node.Content.Count.Should().Be(1);
        second.Content.Should().Be("test text test text 2 test text 3");
        first.Content.Should().Be("test text test text 2 test text 3");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_CanContainText_AddMultipleTextContent_WithContainers_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        if (!node.CanContainText)
            return;

        node.AddTextContent("test text");

        node.Content.Count.Should().Be(1);
        var first = node.GetFirstChild<TextItem>();
        first.Should().NotBeNull();
        first.Content.Should().Be("test text");

        node.AddTextContent("test text 2", "  ");

        node.Content.Count.Should().Be(1);
        var second = node.GetFirstChild<TextItem>();
        second.Should().NotBeNull();
        second.Content.Should().Be("test text  test text 2");
        first.Content.Should().Be("test text  test text 2");

        node.AddContent(node.AllowedElements.First());
        node.Content.Count.Should().Be(2);

        node.AddTextContent("test text 3", " ");
        node.Content.Count.Should().Be(3);
        var textItems = node.GetChildren<TextItem>().ToList();
        textItems.Count.Should().Be(2);
        textItems.First().Content.Should().Be("test text  test text 2");
        textItems.Last().Content.Should().Be(" test text 3");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_AddContent_NotAllowedElement_Throws(Fb2Container node)
    {
        node.Should().NotBeNull();
        var dataCollection = new Fb2ContainerCollection();

        var notAllowedElementName = dataCollection.AllElementsNames.Except(node.AllowedElements).Skip(5).First();

        var notAllowedNode = Fb2NodeFactory.GetNodeByName(notAllowedElementName);

        node.Invoking(n => n.AddContent(notAllowedNode.Name)) // string nodeName
            .Should()
            .Throw<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

        node.Invoking(n => n.AddContent(notAllowedNode)) // Fb2Node 
            .Should()
            .Throw<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

        // params Fb2Node[] nodes
        node.Invoking(n => n.AddContent(notAllowedNode, notAllowedNode)) // lol
            .Should()
            .Throw<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

        node.Invoking(n => n.AddContent(() => notAllowedNode)) // Func<Fb2Node>
            .Should()
            .Throw<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

        node.Invoking(n => n.AddContent(new List<Fb2Node> { notAllowedNode })) // IEnumerable<Fb2Node>
            .Should()
            .Throw<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

        node.Invoking(async n =>
            await n.AddContentAsync(async () => await Task.FromResult(notAllowedNode))) // async node provider
            .Should()
            .ThrowExactlyAsync<UnexpectedNodeException>()
            .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public async Task Container_AddContent_AllowedElement_Works(Fb2Container node)
    {
        node.Should().NotBeNull();
        var allowedElementName = node.AllowedElements.First();
        var firstAllowedNode = Fb2NodeFactory.GetNodeByName(allowedElementName);

        node.AddContent(firstAllowedNode);

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

        ClearContainerContent(node);

        // params Fb2Node[] nodes
        node.AddContent(firstAllowedNode, firstAllowedNode); // lol

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

        ClearContainerContent(node);

        node.AddContent(() => firstAllowedNode); // Func<Fb2Node>

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

        ClearContainerContent(node);

        node.AddContent(new List<Fb2Node> { firstAllowedNode }); // IEnumerable<Fb2Node>

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

        ClearContainerContent(node);

        // async node provider
        await node.AddContentAsync(async () => await Task.FromResult(firstAllowedNode));

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

        ClearContainerContent(node);

        //string name
        node.AddContent(allowedElementName);

        node.Content.Should().HaveCount(1);
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_WithMetadata_AddContent_AllowedElement_Works(Fb2Container node)
    {
        node.Should().NotBeNull();
        var allowedElementName = node.AllowedElements.First();
        var firstAllowedNode = Fb2NodeFactory.GetNodeByName(allowedElementName);

        var testMetadata = new Fb2NodeMetadata(XNamespace.Xml);
        node.NodeMetadata = testMetadata;
        node.AddContent(firstAllowedNode);

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);
        var first = node.Content.First();
        first.Should().NotBeNull();
        first.NodeMetadata.Should().NotBeNull().And.Be(testMetadata);
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_RemoveContent_NullNode_Throws(Fb2Container node)
    {
        var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

        node.Invoking(n => n.RemoveContent((Fb2Node)null)) // Fb2Node 
           .Should()
           .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.RemoveContent((IEnumerable<Fb2Node>)null)) // IEnumerable<Fb2Node>
           .Should()
           .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.RemoveContent((Func<Fb2Node, bool>)null)) // Func<Fb2Node, bool>
           .Should()
           .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.RemoveContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
           .Should()
           .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.RemoveContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
           .Should()
           .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void Container_RemoveContent_ExistingElement_Works(Fb2Container node)
    {
        node.Should().NotBeNull();

        var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());
        var lastAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.Last());

        var nodesEquals = firstAllowedNode.Equals(lastAllowedNode);

        node.AddContent(firstAllowedNode, lastAllowedNode);
        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

        firstAllowedNode.Parent.Should().NotBeNull();
        firstAllowedNode.Parent.Should().Be(node);

        lastAllowedNode.Parent.Should().NotBeNull();
        lastAllowedNode.Parent.Should().Be(node);

        node.RemoveContent(firstAllowedNode); // Fb2Node
        firstAllowedNode.Parent.Should().BeNull();

        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);
        node.Content.Should().Contain(lastAllowedNode);

        // empty nodes (without sub tree) of same type are, basically, equal lol
        // so if you add 2 empty "bold" elements to "paragraph"
        // and then use RemoveContent(firstAllowedNode), and check Contains - it will return true
        if (!nodesEquals) // when container can have only one type child
            node.Content.Contains(firstAllowedNode).Should().BeFalse();

        ClearContainerContent(node);

        lastAllowedNode.Parent.Should().BeNull();

        node.AddContent(firstAllowedNode, lastAllowedNode);
        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

        firstAllowedNode.Parent.Should().NotBeNull();
        firstAllowedNode.Parent.Should().Be(node);

        lastAllowedNode.Parent.Should().NotBeNull();
        lastAllowedNode.Parent.Should().Be(node);

        node.RemoveContent(new List<Fb2Node> { firstAllowedNode, lastAllowedNode }); // IEnumerable<Fb2Node>

        node.Content.Should().BeEmpty();

        node.AddContent(firstAllowedNode, lastAllowedNode);
        node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

        node.RemoveContent(n => n.Name.Equals(firstAllowedNode.Name)); // Func<Fb2Node, bool> predicate

        if (nodesEquals)
            node.Content.Should().BeEmpty();
        else
        {
            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);
            node.Content.Should().Contain(lastAllowedNode);
            node.Content.Should().NotContain(firstAllowedNode);
        }

        ClearContainerContent(node);
    }

    [Fact]
    public void ContainerNode_Load_IgnoreUnsafeNode_Works()
    {
        var strong = new Strong();

        var validStrongXNodeText = "simple bold text";
        var validBoldXNode = new XElement(ElementNames.Strong, validStrongXNodeText);

        // normal scenario
        strong.Load(validBoldXNode);

        strong.Content.Should().HaveCount(1);
        strong.Content.First().Should().BeOfType<TextItem>();
        (strong.Content.First() as TextItem).Content.Should().Be(validStrongXNodeText);
        (strong.Content.First() as TextItem).IsUnsafe.Should().BeFalse();

        ClearContainerContent(strong);

        var unsafeParagraph = new XElement(ElementNames.Paragraph, "render-breaking text"); // this will really bend rendering
        var boldXNodeWithParagraph = new XElement(ElementNames.Strong, unsafeParagraph);

        // bad scenario
        strong.Load(boldXNodeWithParagraph);

        strong.Content.Should().HaveCount(1);
        strong.Content.First().Should().BeOfType<Paragraph>(); // this is bad as most readers will not comply
        (strong.Content.First() as Paragraph).IsUnsafe.Should().BeTrue();

        ClearContainerContent(strong);

        // bad scenario, now without unsafe elements
        strong.Load(boldXNodeWithParagraph, loadUnsafe: false);

        strong.Content.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContainerNode_Serialize_IgnoreUnsafeNode_Works(bool serializeUnsafe)
    {
        var strong = new Strong();

        var unsafeParagraph = new XElement(ElementNames.Paragraph, "render-breaking text"); // this will really bend rendering
        var boldXNodeWithParagraph = new XElement(ElementNames.Strong, unsafeParagraph);

        // bad scenario
        strong.Load(boldXNodeWithParagraph);

        strong.Content.Should().HaveCount(1);
        strong.Content.First().Should().BeOfType<Paragraph>(); // this is bad as most readers will not comply
        (strong.Content.First() as Paragraph).IsUnsafe.Should().BeTrue();

        var serialized = strong.ToXml(serializeUnsafe);
        serialized.Should().NotBeNull().And.BeOfType<XElement>();

        var serializedNodes = serialized.Nodes();

        if (serializeUnsafe)
        {
            serializedNodes.Should().HaveCount(1);
            serializedNodes.First().Should().BeOfType<XElement>();
            (serializedNodes.First() as XElement)!.Name.ToString().Should().Be(ElementNames.Paragraph);
        }
        else
        {
            serializedNodes.Should().BeEmpty();
        }

        ClearContainerContent(strong);
    }

    [Fact]
    public void ContainerNode_ChangeChildrenContent_Works()
    {
        var paragraph = new Paragraph();
        paragraph.Content.Should().BeEmpty();

        paragraph.AddContent(new Strong()); // the only child
        paragraph.Content.Should().HaveCount(1);

        var firstChild = paragraph.Content.First();
        firstChild.Should().BeOfType<Strong>();

        var firstStrongChild = firstChild as Strong;
        firstStrongChild.Content.Should().BeEmpty();

        firstStrongChild.AddTextContent("strong");

        firstStrongChild.Content.Should().HaveCount(1);
        firstStrongChild.Content.First().Should().BeOfType<TextItem>().Subject.Content.Should().Be("strong");

        //re-use prev variables, assert paragraph children anew
        var firstChild1 = paragraph.Content.First();
        firstChild1.Should().BeOfType<Strong>();
        var firstStrongChild1 = firstChild1 as Strong;
        firstStrongChild1.Content.Should().HaveCount(1);
        firstStrongChild1.Content.First().Should().BeOfType<TextItem>().Subject.Content.Should().Be("strong");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void GetChildren_NullParam_Throws(Fb2Container node)
    {
        node.Invoking(n => n.GetChildren((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetChildren(""))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetChildren(string.Empty))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetChildren((Func<Fb2Node, bool>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void GetDescendants_NullParam_Throws(Fb2Container node)
    {
        node.Invoking(n => n.GetDescendants((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetDescendants(""))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetDescendants(string.Empty))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetDescendants((Func<Fb2Node, bool>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void GetFirstChild_NullParam_Throws(Fb2Container node)
    {
        node.Invoking(n => n.GetFirstChild((Func<Fb2Node, bool>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void GetFirstDescendant_NullParam_Throws(Fb2Container node)
    {
        node.Invoking(n => n.GetFirstDescendant((string)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetFirstDescendant(""))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetFirstDescendant(string.Empty))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.GetFirstDescendant((Func<Fb2Node, bool>)null))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void TryGetFirstDescendant_NullParam_Throws(Fb2Container node)
    {
        node.Invoking(n => n.TryGetFirstDescendant((string)null, out var result))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.TryGetFirstDescendant("", out var result))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.TryGetFirstDescendant(string.Empty, out var result))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        node.Invoking(n => n.TryGetFirstDescendant((Func<Fb2Node, bool>)null, out var result))
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void ContainerNode_InvalidQueryChildren_Throws(Fb2Container node)
    {
        var invalidNodeName = "blahNameInvalid";

        node.Invoking(n => n.GetDescendants(invalidNodeName))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .And.Message.Should().Be("'blahNameInvalid' is not known Fb2 node name.");

        node.Invoking(n => n.GetFirstChild(invalidNodeName))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .And.Message.Should().Be("'blahNameInvalid' is not known Fb2 node name.");

        node.Invoking(n => n.GetFirstDescendant(invalidNodeName))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .And.Message.Should().Be("'blahNameInvalid' is not known Fb2 node name.");

        node.Invoking(n => n.TryGetFirstDescendant(invalidNodeName, out var result))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .And.Message.Should().Be("'blahNameInvalid' is not known Fb2 node name.");

        node.Invoking(n => n.GetChildren(invalidNodeName))
            .Should()
            .ThrowExactly<InvalidNodeException>()
            .And.Message.Should().Be("'blahNameInvalid' is not known Fb2 node name.");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void EmptyContainerNode_QueryChildrenNodes_ReturnsNullOrEmpty(Fb2Container node)
    {
        var firstAllowedChildName = node.AllowedElements.First();
        bool firstAllowedChildPredicate(Fb2Node nodeToCompare) => nodeToCompare.Name.Equals(firstAllowedChildName);

        node.GetChildren(firstAllowedChildName).Should().BeEmpty();
        node.GetChildren(firstAllowedChildPredicate).Should().BeEmpty();
        node.GetChildren<Fb2Node>().Should().BeEmpty();
        node.GetChildren<BodySection>().Should().BeEmpty();

        node.GetFirstChild(firstAllowedChildName).Should().BeNull();
        node.GetFirstChild(firstAllowedChildPredicate).Should().BeNull();
        node.GetFirstChild((string)null).Should().BeNull();
        node.GetFirstChild("").Should().BeNull();
        node.GetFirstChild<Fb2Node>().Should().BeNull();
        node.GetFirstChild<BodySection>().Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void EmptyContainerNode_QueryDescendantNodes_ReturnsNullOrEmpty(Fb2Container node)
    {
        var firstAllowedChildName = node.AllowedElements.First();
        Func<Fb2Node, bool> firstAllowedChildPredicate = nodeToCompare => nodeToCompare.Name.Equals(firstAllowedChildName);

        node.GetDescendants(firstAllowedChildName).Should().BeEmpty();
        node.GetDescendants(firstAllowedChildPredicate).Should().BeEmpty();
        node.GetDescendants<Fb2Node>().Should().BeEmpty();
        node.GetDescendants<BodySection>().Should().BeEmpty();

        node.GetFirstDescendant(firstAllowedChildName).Should().BeNull();
        node.GetFirstDescendant(firstAllowedChildPredicate).Should().BeNull();
        node.GetFirstDescendant<Fb2Node>().Should().BeNull();
        node.GetFirstDescendant<BodySection>().Should().BeNull();

        var success = node.TryGetFirstDescendant(firstAllowedChildName, out var resultNode);
        success.Should().BeFalse();
        resultNode.Should().BeNull();

        var predicateSuccess = node.TryGetFirstDescendant(firstAllowedChildPredicate, out var resultPredicateNode);
        predicateSuccess.Should().BeFalse();
        resultPredicateNode.Should().BeNull();

        var genericSuccess = node.TryGetFirstDescendant<Fb2Node>(out var resultGenericNode);
        genericSuccess.Should().BeFalse();
        resultGenericNode.Should().BeNull();

        var bookBodySuccess = node.TryGetFirstDescendant<BookBody>(out var resultBookBody);
        bookBodySuccess.Should().BeFalse();
        resultBookBody.Should().BeNull();
    }

    [Fact]
    public void Container_QueryDescendants_Works()
    {
        var section = new BodySection();

        var paragraph = new Paragraph();
        var plainText1 = new TextItem().AddContent("plain text 1 ");
        var strong = new Strong().AddTextContent("strong content ");
        var plainText2 = new TextItem().AddContent("plain text 2");

        paragraph.AddContent(plainText1, strong, plainText2);

        section.AddContent(paragraph);

        var queryByNameResult = section.GetDescendants(ElementNames.Strong);
        var queryByTypeResult = section.GetDescendants<Strong>();
        var queryByPredicateResult = section.GetDescendants((n) => n is Strong);

        queryByNameResult.Should().HaveCount(1);
        queryByNameResult.Should().HaveSameCount(queryByTypeResult);
        queryByPredicateResult.Should().HaveSameCount(queryByNameResult);

        var firstQueryByNameResult = queryByNameResult.First();
        var firstQueryByTypeResult = queryByTypeResult.First();
        var firstQueryByPredicateResult = queryByPredicateResult.First();

        firstQueryByNameResult.Should().Be(firstQueryByTypeResult);
        firstQueryByPredicateResult.Should().Be(firstQueryByNameResult);
        firstQueryByNameResult.Should().BeOfType<Strong>();
        firstQueryByNameResult.Name.Should().Be(ElementNames.Strong);

        firstQueryByNameResult.HasContent.Should().BeTrue();
        firstQueryByTypeResult.Content.Should().HaveCount(1);
        firstQueryByTypeResult.Content.First().Should().BeOfType<TextItem>();
        (firstQueryByTypeResult.Content.First() as TextItem)!.Content.Should().Be("strong content ");

        var singularResultByName = section.GetFirstDescendant(ElementNames.Strong);
        var singularResultByType = section.GetFirstDescendant<Strong>();
        var singularResultByPredicate = section.GetFirstDescendant((n) => n is Strong);

        singularResultByName.Should().NotBeNull();
        singularResultByType.Should().NotBeNull();
        singularResultByPredicate.Should().NotBeNull();

        singularResultByName.Should().Be(singularResultByType);
        singularResultByPredicate.Should().Be(singularResultByName);

        singularResultByName!.Name.Should().Be(ElementNames.Strong);
        singularResultByName!.Should().BeOfType<Strong>();

        singularResultByName.HasContent.Should().BeTrue();

        singularResultByType!.Content.Should().HaveCount(1);
        var singularResultByTypeChild = singularResultByType.Content.First();
        singularResultByTypeChild.Should().NotBeNull();
        singularResultByTypeChild.Should().BeOfType<TextItem>();

        (singularResultByTypeChild as TextItem)!.Content.Should().Be("strong content ");

        var abstractDescendantQuery = section.GetDescendants<Fb2Node>();
        abstractDescendantQuery.Should().NotBeNullOrEmpty().And.HaveCount(5);
    }

    // it's a bit complex to run tests for each model
    // so in those lazy tests Paragraph is used as parent element
    [Fact]
    public void Paragraph_QueryContent_Works()
    {
        // setup 
        var paragraph = new Paragraph();
        paragraph.HasContent.Should().BeFalse();

        paragraph.AddContent(new Strong().AddTextContent("strong text 1 "));

        paragraph.HasContent.Should().BeTrue();

        paragraph
            .AddContent(
                new Emphasis()
                    .AppendTextContent("italic text 1 ")
                    .AddContent(
                        new Strong()
                            .AddTextContent("strong italic text ")
                            .AddContent(new Strikethrough().AddTextContent("bold strikethrough text "))),
                new Strong().AddTextContent("strong text 2 "))
            .AddTextContent("plain text 1");

        // verify setup
        paragraph.Content.Should().HaveCount(4);

        var firstStrong = paragraph.Content.First() as Strong;
        firstStrong!.Content.Should().HaveCount(1);
        firstStrong.Content.First().Should().BeOfType<TextItem>();

        var firstItalic = paragraph.Content[1] as Emphasis;
        firstItalic!.Content.Should().HaveCount(2);
        firstItalic.Content.First().Should().BeOfType<TextItem>();
        firstItalic.Content[1].Should().BeOfType<Strong>();

        var secondStrong = paragraph.Content[2] as Strong;
        secondStrong!.Content.Should().HaveCount(1);
        secondStrong.Content.First().Should().BeOfType<TextItem>();

        var plainText = paragraph.Content.Last() as TextItem;
        plainText!.Content.Should().Be("plain text 1");

        // children query example

        // looking for "Strong" childer - should be 2
        var strongChildren = paragraph.GetChildren(ElementNames.Strong);
        var strongPredicateChildren = paragraph.GetChildren(c => c is Strong);
        var strongGenericChildren = paragraph.GetChildren<Strong>();

        strongChildren.Should().HaveCount(2);
        strongPredicateChildren.Should().HaveCount(2);
        strongGenericChildren.Should().HaveCount(2);

        strongChildren
            .Should()
            .BeEquivalentTo(strongPredicateChildren)
            .And
            .BeEquivalentTo(strongGenericChildren);

        // looking for single child
        var plainTextByName = paragraph.GetFirstChild(ElementNames.FictionText);
        var plainPredicateText = paragraph.GetFirstChild(p => p is TextItem);
        var plainGenericText = paragraph.GetFirstChild<TextItem>();

        plainTextByName.Should().NotBeNull();
        (plainTextByName as Fb2Element)!.Content.Should().Be("plain text 1");
        plainTextByName.Should().Be(plainPredicateText).And.Be(plainGenericText);

        // and to stress the obvios
        paragraph.GetFirstChild(c => c is Strong)
            .Should().BeOfType<Strong>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("strong text 1 ");

        // okay, descendants queries
        paragraph
            .GetFirstDescendant(n => n is Strikethrough)
            .Should().BeOfType<Strikethrough>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("bold strikethrough text ");

        paragraph
            .GetFirstDescendant(ElementNames.Strikethrough)
            .Should().BeOfType<Strikethrough>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("bold strikethrough text ");

        paragraph
            .GetFirstDescendant<Strikethrough>()
            .Should().BeOfType<Strikethrough>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("bold strikethrough text ");

        var attemptToGetStrikethroughByName =
            paragraph.TryGetFirstDescendant(ElementNames.Strikethrough, out var strikethrough);
        attemptToGetStrikethroughByName.Should().BeTrue();
        strikethrough
            .Should().BeOfType<Strikethrough>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("bold strikethrough text ");

        var attemptToGetStrikethroughByPredicate =
            paragraph.TryGetFirstDescendant(n => n is Strikethrough, out var predicateStrikethrough);

        predicateStrikethrough
            .Should().BeOfType<Strikethrough>()
            .Subject.Content.Should().HaveCount(1)
            .And.Subject.First()
            .Should().BeOfType<TextItem>()
            .Subject.Content.Should().Be("bold strikethrough text ");
    }

    [Theory]
    [ClassData(typeof(Fb2ContainerCollection))]
    public void EmptyContainer_ToString_ReturnsEmptyString(Fb2Container node)
    {
        node.Should().NotBeNull();
        var toString = node.ToString();
        toString.Should().BeEmpty();
    }

    [Fact]
    public void NotEmptyContainer_ToString_ReturnsValue()
    {
        var paragraph = new Paragraph();
        paragraph.AddTextContent("Test text ");
        paragraph.AddContent(new Strong().AddTextContent("and strong text"));

        var toString = paragraph.ToString();

        toString.Should().NotBeNullOrEmpty();
        toString.Should().NotBeNullOrWhiteSpace();

        toString.Should().Be("Test text and strong text");
    }

    [Fact]
    public void NotEmptyContainer_NotInlineChildren_ToString_ReturnsValue()
    {
        var bodySection = new BodySection();

        var paragraph1 = new Paragraph();
        paragraph1.AddTextContent("Test text paragraph 1 ");
        paragraph1.AddContent(new Strong().AddTextContent("and strong text 1."));

        var paragraph2 = new Paragraph();
        paragraph2.AddTextContent("Test text paragraph 2 ");
        paragraph2.AddContent(new Strong().AddTextContent("and strong text 2."));

        bodySection.AddContent(paragraph1, paragraph2);

        var toString = bodySection.ToString();
        toString.Should().NotBeNullOrEmpty();
        toString.Should().NotBeNullOrWhiteSpace();

        toString.Should().Be($"{Environment.NewLine}Test text paragraph 1 and strong text 1.{Environment.NewLine}Test text paragraph 2 and strong text 2.");
    }

    [Fact]
    public void CloneContainer_HasContent_ClonesContent()
    {
        var paragraph1 = new Paragraph();
        paragraph1.AddTextContent("Test text paragraph 1 ");
        paragraph1.AddContent(new Strong().AddTextContent("and strong text 1."));
        paragraph1.HasContent.Should().BeTrue();
        paragraph1.Content.Should().HaveCount(2);

        var paragraph2 = paragraph1.Clone();

        paragraph2.Should().Be(paragraph1);
        paragraph2.Should().BeOfType<Paragraph>();
        (paragraph2 as Fb2Container)!.HasContent.Should().BeTrue();
        (paragraph2 as Fb2Container)!.Content.Should().HaveCount(2);

        var strong1 = paragraph1.Content.Last() as Fb2Container;
        strong1.Should().NotBeNull();
        strong1!.Name.Should().Be(ElementNames.Strong);
        strong1.Parent.Should().NotBeNull().And.Be(paragraph1);
        strong1.HasContent.Should().BeTrue();
        strong1.Content.Should().HaveCount(1);

        var strong1Clone = strong1.Clone() as Fb2Container;
        strong1Clone.Should().NotBeNull();
        strong1Clone!.Parent.Should().BeNull();
        strong1Clone.HasContent.Should().BeTrue();
        strong1Clone.Content.Should().HaveCount(1);
    }

    private static void ClearContainerContent(Fb2Container node)
    {
        node.ClearContent();
        node.Content.Should().BeEmpty();
        node.HasContent.Should().BeFalse();
    }
}

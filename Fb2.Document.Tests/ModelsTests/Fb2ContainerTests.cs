using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
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
    public class Fb2ContainerTests
    {
        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_AddContent_NullNode_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

            node.Invoking(n => n.AddContent((Fb2Node)null)) // Fb2Node 
               .Should()
               .Throw<ArgumentNullException>();

            // params Fb2Node[] nodes
            node.Invoking(n => n.AddContent()).Should().Throw<ArgumentNullException>();

            // params Fb2Node[] nodes
            node.Invoking(n => n.AddContent(null, null)) // lol
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AddContent(() => null)) // Func<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AddContent((Func<Fb2Node>)null)) // Func<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AddContent((List<Fb2Node>)null)) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AddContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AddContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(async n => await n.AddContentAsync(
                async () =>
                {
                    await Task.Delay(10); // async node provider
                    return null;
                }))
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(async n => await n.AddContentAsync(null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_CantContainText_AddTextContent_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();

            if (node.CanContainText)
                return;

            node.Invoking(n => n.AddContent(new TextItem().AddContent("test text")))
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node 'text'.");

            node.Invoking(n => n.AddTextContent("test text"))
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node 'text'.");
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_CanContainText_AddTextContent_Works(Fb2Container node)
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
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_AddContent_NotAllowedElement_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();
            var dataCollection = new Fb2ContainerCollection();

            var notAllowedElementName = dataCollection.AllElementsNames.Except(node.AllowedElements).Skip(5).First();

            var notAllowedNode = Fb2NodeFactory.GetNodeByName(notAllowedElementName);

            node.Invoking(n => n.AddContent(notAllowedNode)) // Fb2Node 
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node '{notAllowedNode.Name}'.");

            // params Fb2Node[] nodes
            node.Invoking(n => n.AddContent(notAllowedNode, notAllowedNode)) // lol
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node '{notAllowedNode.Name}'.");

            node.Invoking(n => n.AddContent(() => notAllowedNode)) // Func<Fb2Node>
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node '{notAllowedNode.Name}'.");

            node.Invoking(n => n.AddContent(new List<Fb2Node> { notAllowedNode })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node '{notAllowedNode.Name}'.");

            node.Invoking(
                async n => await n.AddContentAsync(async () =>
                {
                    await Task.Delay(10); // async node provider
                    return notAllowedNode;
                }))
                .Should()
                .Throw<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain node '{notAllowedNode.Name}'.");
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public async Task Container_AddContent_AllowedElement_Works(Fb2Container node)
        {
            node.Should().NotBeNull();
            var allowedElementName = node.AllowedElements.First();
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(allowedElementName);

            node.AddContent(firstAllowedNode);

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);

            ClearContainerContent(node);

            // params Fb2Node[] nodes
            node.AddContent(firstAllowedNode, firstAllowedNode); // lol

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(2);

            ClearContainerContent(node);

            node.AddContent(() => firstAllowedNode); // Func<Fb2Node>

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);

            ClearContainerContent(node);

            node.AddContent(new List<Fb2Node> { firstAllowedNode }); // IEnumerable<Fb2Node>

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);

            ClearContainerContent(node);

            await node.AddContentAsync(async () =>
            {
                await Task.Delay(10); // async node provider
                return firstAllowedNode;
            });

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);

            ClearContainerContent(node);
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_RemoveContent_NullNode_Throws(Fb2Container node)
        {
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

            node.Invoking(n => n.RemoveContent((Fb2Node)null)) // Fb2Node 
               .Should()
               .Throw<ArgumentNullException>();

            node.Invoking(n => n.RemoveContent((IEnumerable<Fb2Node>)null)) // IEnumerable<Fb2Node>
               .Should()
               .Throw<ArgumentNullException>();

            node.Invoking(n => n.RemoveContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
               .Should()
               .Throw<ArgumentNullException>();

            node.Invoking(n => n.RemoveContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
               .Should()
               .Throw<ArgumentNullException>();

            node.Invoking(n => n.RemoveContent((Func<Fb2Node, bool>)null)) // Func<Fb2Node, bool>
               .Should()
               .Throw<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_RemoveContent_AllowedElement_Works(Fb2Container node)
        {
            node.Should().NotBeNull();

            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());
            var lastAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.Last());

            var nodesEquals = firstAllowedNode.Equals(lastAllowedNode);

            node.AddContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(2);

            node.RemoveContent(firstAllowedNode); // Fb2Node

            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);
            node.Content.Should().Contain(lastAllowedNode);

            if (!nodesEquals) // when container can have only one type child
                node.Content.Contains(firstAllowedNode).Should().BeFalse();

            ClearContainerContent(node);

            node.AddContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(2);

            node.RemoveContent(new List<Fb2Node> { firstAllowedNode, lastAllowedNode }); // IEnumerable<Fb2Node>

            node.Content.Should().BeEmpty();

            node.AddContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(2);

            node.RemoveContent(n => n.Name.Equals(firstAllowedNode.Name)); // Func<Fb2Node, bool> predicate

            if (nodesEquals)
                node.Content.Should().BeEmpty();
            else
            {
                node.Content.Should().NotBeEmpty().And.Subject.Count().Should().Be(1);
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

            ClearContainerContent(strong);

            var unsafeParagraph = new XElement(ElementNames.Paragraph, "render-breaking text"); // this will really bend rendering
            var boldXNodeWithParagraph = new XElement(ElementNames.Strong, unsafeParagraph);

            // bad scenario
            strong.Load(boldXNodeWithParagraph);

            strong.Content.Should().HaveCount(1);
            strong.Content.First().Should().BeOfType<Paragraph>(); // this is bad as most readers will not comply

            ClearContainerContent(strong);

            // bad scenario, now without unsafe elements
            strong.Load(boldXNodeWithParagraph, loadUnsafe: false);

            strong.Content.Should().BeEmpty();
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

        private void ClearContainerContent(Fb2Container node)
        {
            node.ClearContent();
            node.Content.Should().BeEmpty();
        }
    }
}

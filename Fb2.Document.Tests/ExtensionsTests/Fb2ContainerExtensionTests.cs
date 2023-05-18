using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace Fb2.Document.Tests.ExtensionTests
{

    public class Fb2ContainerExtensionTests
    {
        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_AppendContent_NullNode_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

            node.Invoking(n => n.AppendContent((Fb2Node)null)) // Fb2Node 
               .Should()
               .Throw<ArgumentNullException>();

            //string nodeName
            node.Invoking(n => n.AppendContent("")).Should().Throw<ArgumentNullException>();

            //string nodeName
            node.Invoking(n => n.AppendContent((string)null)).Should().Throw<ArgumentNullException>();

            // params Fb2Node[] nodes
            node.Invoking(n => n.AppendContent()).Should().Throw<ArgumentNullException>();

            // params Fb2Node[] nodes
            node.Invoking(n => n.AppendContent(null, null)) // lol
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AppendContent(() => null)) // Func<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AppendContent((Func<Fb2Node>)null)) // Func<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AppendContent((List<Fb2Node>)null)) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AppendContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(n => n.AppendContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentNullException>();

            node.Invoking(async n => await n.AppendContentAsync(
                async () => await Task.FromResult<Fb2Node>(null))) // async node provider
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();

            node.Invoking(async n => await n.AppendContentAsync(null))
                .Should()
                .ThrowExactlyAsync<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_AppendContent_UnknownNode_Throws(Fb2Container node)
        {
            var impostor = new ImpostorNode();

            node.Invoking(n => n.AppendContent(impostor)) // Fb2Node 
               .Should()
               .ThrowExactly<InvalidNodeException>()
               .WithMessage($"'Impostor' is not known Fb2 node name.")
               .And.NodeName
               .Should()
               .Be(impostor.Name);

            node.Invoking(n => n.AppendContent(() => impostor)) // Func<Fb2Node>
                .Should()
                .ThrowExactly<InvalidNodeException>()
                .WithMessage($"'{impostor.Name}' is not known Fb2 node name.")
                .And.NodeName
                .Should()
                .Be(impostor.Name);

            node.Invoking(n => n.AppendContent(impostor.Name)) // name
               .Should()
               .ThrowExactly<InvalidNodeException>()
               .WithMessage($"'Impostor' is not known Fb2 node name.")
               .And.NodeName
               .Should()
               .Be(impostor.Name);

            var sneakyImpostor = new ImpostorNode(ElementNames.Paragraph);

            node.Invoking(n => n.AppendContent(sneakyImpostor)) // Fb2Node 
               .Should()
               .ThrowExactly<InvalidNodeException>()
               .WithMessage($"'{sneakyImpostor.Name}' is not known Fb2 node name.")
               .And.NodeName
               .Should()
               .Be(sneakyImpostor.Name);

            node.Invoking(n => n.AppendContent(() => sneakyImpostor)) // Func<Fb2Node>
               .Should()
               .ThrowExactly<InvalidNodeException>()
               .WithMessage($"'{sneakyImpostor.Name}' is not known Fb2 node name.")
               .And.NodeName
               .Should()
               .Be(sneakyImpostor.Name);
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public async Task Container_CantContainText_AppendTextContent_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();

            if (node.CanContainText)
                return;

            node.Invoking(n => n.AppendContent(new TextItem().AddContent("test text")))
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain 'text'.");

            node.Invoking(n => n.AppendTextContent("test text"))
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain 'text'.");

            node.Invoking(n => n.AppendTextContent(() => "test text"))
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain 'text'.");

            await node
                .Invoking(async n => await n.AppendTextContentAsync(async () =>
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
        public async Task Container_CanContainText_AppendTextContent_Works(Fb2Container node)
        {
            node.Should().NotBeNull();

            if (!node.CanContainText)
                return;

            node.AddContent(new TextItem().AppendContent("test text"));

            node.Content.Count.Should().Be(1);
            var first = node.Content.First();
            first.Should().BeOfType(typeof(TextItem));
            (first as Fb2Element).Content.Should().Be("test text");

            ClearContainerContent(node);

            node.AppendTextContent("test text");

            node.Content.Count.Should().Be(1);
            var second = node.Content.First();
            second.Should().BeOfType(typeof(TextItem));
            (second as Fb2Element).Content.Should().Be("test text");

            ClearContainerContent(node);

            node.AppendTextContent(() => "test text");

            node.Content.Count.Should().Be(1);
            var third = node.Content.First();
            third.Should().BeOfType(typeof(TextItem));
            (third as Fb2Element).Content.Should().Be("test text");

            ClearContainerContent(node);

            await node.AppendTextContentAsync(async () =>
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
        public void Container_AppendContent_NotAllowedElement_Throws(Fb2Container node)
        {
            node.Should().NotBeNull();
            var dataCollection = new Fb2ContainerCollection();

            var notAllowedElementName = dataCollection.AllElementsNames.Except(node.AllowedElements).Skip(5).First();

            var notAllowedNode = Fb2NodeFactory.GetNodeByName(notAllowedElementName);

            var exc = node.Invoking(n => n.AppendContent(notAllowedNode.Name)) // string nodeName
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

            exc.Which.ParentNodeName.Should().Be(node.Name);
            exc.Which.ChildNodeName.Should().Be(notAllowedNode.Name);

            node.Invoking(n => n.AppendContent(notAllowedNode)) // Fb2Node 
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

            // params Fb2Node[] nodes
            node.Invoking(n => n.AppendContent(notAllowedNode, notAllowedNode)) // lol
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

            node.Invoking(n => n.AppendContent(() => notAllowedNode)) // Func<Fb2Node>
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

            node.Invoking(n => n.AppendContent(new List<Fb2Node> { notAllowedNode })) // IEnumerable<Fb2Node>
                .Should()
                .ThrowExactly<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");

            node.Invoking(async n =>
                await n.AppendContentAsync(async () => await Task.FromResult(notAllowedNode))) // async node provider
                .Should()
                .ThrowExactlyAsync<UnexpectedNodeException>()
                .WithMessage($"Node '{node.Name}' can not contain '{notAllowedNode.Name}'.");
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public async Task Container_AppendContent_AllowedElement_Works(Fb2Container node)
        {
            node.Should().NotBeNull();
            var allowedElementName = node.AllowedElements.First();
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(allowedElementName);

            node.AppendContent(firstAllowedNode);

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

            ClearContainerContent(node);

            // params Fb2Node[] nodes
            node.AppendContent(firstAllowedNode, firstAllowedNode); // lol

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

            ClearContainerContent(node);

            node.AppendContent(() => firstAllowedNode); // Func<Fb2Node>

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

            ClearContainerContent(node);

            node.AppendContent(new List<Fb2Node> { firstAllowedNode }); // IEnumerable<Fb2Node>

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

            ClearContainerContent(node);

            // async node provider
            await node.AppendContentAsync(async () => await Task.FromResult(firstAllowedNode));

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);

            ClearContainerContent(node);

            //string name
            node.AppendContent(allowedElementName);

            node.Content.Should().HaveCount(1);
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerCollection))]
        public void Container_DeleteContent_NullNode_Throws(Fb2Container node)
        {
            var firstAllowedNode = Fb2NodeFactory.GetNodeByName(node.AllowedElements.First());

            node.Invoking(n => n.DeleteContent((Fb2Node)null)) // Fb2Node 
               .Should()
               .ThrowExactly<ArgumentNullException>();

            node.Invoking(n => n.DeleteContent((IEnumerable<Fb2Node>)null)) // IEnumerable<Fb2Node>
               .Should()
               .ThrowExactly<ArgumentNullException>();

            node.Invoking(n => n.DeleteContent((Func<Fb2Node, bool>)null)) // Func<Fb2Node, bool>
               .Should()
               .ThrowExactly<ArgumentNullException>();

            node.Invoking(n => n.DeleteContent(new List<Fb2Node> { null, null })) // IEnumerable<Fb2Node>
               .Should()
               .ThrowExactly<ArgumentNullException>();

            node.Invoking(n => n.DeleteContent(new List<Fb2Node> { firstAllowedNode, null })) // IEnumerable<Fb2Node>
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

            node.AppendContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

            firstAllowedNode.Parent.Should().NotBeNull();
            firstAllowedNode.Parent.Should().Be(node);

            lastAllowedNode.Parent.Should().NotBeNull();
            lastAllowedNode.Parent.Should().Be(node);

            node.DeleteContent(firstAllowedNode); // Fb2Node
            firstAllowedNode.Parent.Should().BeNull();

            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);
            node.Content.Should().Contain(lastAllowedNode);

            // empty nodes (without sub tree) of same type are, basically, equal lol
            // so if you add 2 empty "bold" elements to "paragraph"
            // and then use DeleteContent(firstAllowedNode), and check Contains - it will return true
            if (!nodesEquals) // when container can have only one type child
                node.Content.Contains(firstAllowedNode).Should().BeFalse();

            ClearContainerContent(node);

            lastAllowedNode.Parent.Should().BeNull();

            node.AppendContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

            firstAllowedNode.Parent.Should().NotBeNull();
            firstAllowedNode.Parent.Should().Be(node);

            lastAllowedNode.Parent.Should().NotBeNull();
            lastAllowedNode.Parent.Should().Be(node);

            node.DeleteContent(new List<Fb2Node> { firstAllowedNode, lastAllowedNode }); // IEnumerable<Fb2Node>

            node.Content.Should().BeEmpty();

            node.AppendContent(firstAllowedNode, lastAllowedNode);
            node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(2);

            node.DeleteContent(n => n.Name.Equals(firstAllowedNode.Name)); // Func<Fb2Node, bool> predicate

            if (nodesEquals)
                node.Content.Should().BeEmpty();
            else
            {
                node.Content.Should().NotBeEmpty().And.Subject.Should().HaveCount(1);
                node.Content.Should().Contain(lastAllowedNode);
                node.Content.Should().NotContain(firstAllowedNode);
            }

            node.EraseContent();
            node.Content.Should().BeEmpty();
            node.HasContent.Should().BeFalse();
        }

        private static void ClearContainerContent(Fb2Container node)
        {
            node.EraseContent();
            node.Content.Should().BeEmpty();
            node.HasContent.Should().BeFalse();
        }
    }
}
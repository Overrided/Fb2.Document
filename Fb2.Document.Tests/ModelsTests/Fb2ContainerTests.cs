using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        [ClassData(typeof(Fb2ContainerTypeData))]
        public void Container_AddContent_NullNode_Throws(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;

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
        [ClassData(typeof(Fb2ContainerTypeData))]
        public void Container_CantContainText_AddTextContent_Throws(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;

            if (node.CanContainText)
                return;

            node.Invoking(n => n.AddContent(new TextItem().AddContent("test text")))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"Element '{node.Name}' is not designed to contain text (direct content). See {node.Name}.{nameof(node.CanContainText)}.");

            node.Invoking(n => n.AddTextContent("test text"))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"Element '{node.Name}' is not designed to contain text (direct content). See {node.Name}.{nameof(node.CanContainText)}.");
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerTypeData))]
        public void Container_AddContent_NotAllowedElement_Throws(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;
            var dataCollection = new Fb2ContainerTypeData();

            var notAllowedElementName = dataCollection.AllElementsNames.Except(node.AllowedElements).Skip(5).First();

            var notAllowedNode = Fb2ElementFactory.GetNodeByName(notAllowedElementName);

            node.Invoking(n => n.AddContent(notAllowedNode)) // Fb2Node 
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElementName}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");

            // params Fb2Node[] nodes
            node.Invoking(n => n.AddContent(notAllowedNode, notAllowedNode)) // lol
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElementName}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");

            node.Invoking(n => n.AddContent(() => notAllowedNode)) // Func<Fb2Node>
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElementName}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");

            node.Invoking(n => n.AddContent(new List<Fb2Node> { notAllowedNode })) // IEnumerable<Fb2Node>
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElementName}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");

            node.Invoking(
                async n => await n.AddContentAsync(async () =>
                {
                    await Task.Delay(10); // async node provider
                    return notAllowedNode;
                }))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElementName}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerTypeData))]
        public async Task Container_AddContent_AllowedElement_Works(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;
            var allowedElementName = node.AllowedElements.First();
            var firstAllowedNode = Fb2ElementFactory.GetNodeByName(allowedElementName);

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
        [ClassData(typeof(Fb2ContainerTypeData))]
        public void Container_RemoveContent_AllowedElement_Works(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;

            var firstAllowedNode = Fb2ElementFactory.GetNodeByName(node.AllowedElements.First());
            var lastAllowedNode = Fb2ElementFactory.GetNodeByName(node.AllowedElements.Last());

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

        private void ClearContainerContent(Fb2Container node)
        {
            node.ClearContent();
            node.Content.Should().BeEmpty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2.Document.Constants;
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
        public void ContainerModel_AddContent_NullNode(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;

            node.Invoking(n => n.AddContent((Fb2Node)null))
                .Should()
                .Throw<ArgumentNullException>();
        }

        [Theory]
        [ClassData(typeof(Fb2ContainerTypeData))]
        public void ContainerModel_AddTextContent_CantContainText(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;

            if (node.CanContainText)
                return;

            node.Invoking(n => n.AddContent(new TextItem()))
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
        public void ContainerModel_AddContent_NotAllowedElement(Type modelType)
        {
            var node = Activator.CreateInstance(modelType) as Fb2Container;
            var dataCollection = new Fb2ContainerTypeData();

            var notAllowedElementName = dataCollection.AllElementsNames.Except(node.AllowedElements).Skip(5).First();

            var notAllowedElement = Fb2ElementFactory.GetNodeByName(notAllowedElementName);

            node.Invoking(n => n.AddContent(() => notAllowedElement))
                .Should()
                .Throw<ArgumentException>()
                .WithMessage($"'{notAllowedElement.Name}' is not valid child for '{node.Name}'. See {node.Name}.{nameof(Fb2Container.AllowedElements)} for valid content elements.");
        }
    }
}

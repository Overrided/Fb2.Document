using System;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests.ModelsTests
{
    [TestClass]
    public class Fb2ContainerTests : TestBase
    {
        [TestMethod]
        public void Fb2Container_Tests()
        {
            var assembly = typeof(Fb2ElementFactory).Assembly;

            var modelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                       !type.IsAbstract && type.IsClass).ToList();

            var containerTypes = modelTypes.Where(ct => ct.IsSubclassOf(typeof(Fb2Container))).ToList();

            foreach (var modelType in containerTypes)
            {
                Fb2Container_Load_Content_Test(modelType);
                Fb2Container_Load_UnsafeContent_Test(modelType);
                Fb2Container_Load_SkipsInvalidNode_Test(modelType);
                Fb2Container_WithContent_NullNode(modelType);
                Fb2Container_WithContent_CantContainText(modelType);
                Fb2Container_WithContent_UnsafeContent(modelType);
                Fb2Container_WithContainer_ValidContent(modelType);
                Fb2Container_WithContainer_Params_NullNodes(modelType);
                Fb2Container_WithContainer_Params_InvalidNode_Rollback(modelType);
                Fb2Container_WithContainer_Params_InvalidNode_Rollback_ToContent(modelType);
            }
        }

        public void Fb2Container_Load_Content_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            //container.WithContent(() => new Paragraph().WithContent(() => new TextItem().WithContent("Hello World!")));
            //container.WithContent(() => new Paragraph().WithTextContent("Hello world"));
            //container.WithContent(() => new Paragraph().WithContent(() => new TextItem().WithContent("Hello ").WithContent("World!")));

            //container.WithContent(() => new Paragraph().WithTextContent("Hello").WithTextContent(" world"));

            //container.WithContent(new Paragraph().WithTextContent("Hello").WithTextContent(" world"));

            //container.WithContent(new Paragraph().WithContent(new Strong().WithTextContent("bold text")));

            //container.WithContent(
            //    new Paragraph().WithTextContent("Hello ").WithAttribute("id", "testId"),
            //    new Paragraph().WithTextContent("world!"));

            //container.WithContent(
            //    new Paragraph().WithContent(new Strong().WithContent(new Emphasis().WithTextContent("Hello"))).WithAttribute("id", "testId"),
            //    new Paragraph().WithContent(new Strikethrough().WithContent(() => new Emphasis().WithContent(new Strong().WithTextContent("Strikethrough Emphasis Strong")))).WithAttribute("id", "testId2"));

            //container.WithContent(
            //    new Paragraph().WithContent(new Strong().WithContent(new Emphasis().WithTextContent("Hello"))).WithAttribute("id", "testId"),
            //    new Paragraph()
            //        .WithContent(
            //            new Strikethrough()
            //                .WithContent(
            //                    new Emphasis()
            //                        .WithContent(
            //                            new Strong().WithTextContent("world!")))).WithAttribute("id", "testId2"));

            //container.WithContent(ElementNames.Paragraph);

            //container.WithContent(ElementNames.Paragraph, (newParag) =>
            //{
            //    ((Fb2Container)newParag).WithTextContent("Hello world");
            //    return newParag;
            //});

            //container.WithContent(ElementNames.Paragraph,
            //    (newParag) => ((Fb2Container)newParag).WithTextContent("Hello world"));

            var containerElement = GetXElementWithValidContent(container);

            container.Load(containerElement);

            // taking additional text element in count
            Assert.AreEqual(container.AllowedElements.Count + 1, container.Content.Count);

            var childTypes = container.Content.Select(child => child.GetType()).ToList();

            // no twice loaded child of a same type
            CollectionAssert.AreEqual(childTypes, childTypes.Distinct().ToList());

            foreach (var child in container.Content)
            {
                // there are 3 models with specific ToString overrides: 
                // Table, TableRow and Image.
                if (Utils.OverridesToString(child))
                    continue;
                else if (child.Name == ElementNames.EmptyLine)
                    Assert.AreEqual(Environment.NewLine, child.ToString());
                else if (child.Name == ElementNames.FictionText)
                    Assert.AreEqual($"test text", child.ToString());
                else
                    Assert.AreEqual($"test {child.Name} text", child.ToString());
            }

            if (!container.CanContainText)
            {
                var textChild = container.Content.SingleOrDefault(ch => ch.Name == ElementNames.FictionText);

                Assert.IsNotNull(textChild);
                Assert.IsTrue(textChild.Unsafe);
            }

            var serialized = container.ToXml();

            Assert.AreEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Container_Load_UnsafeContent_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var containerElement = GetXElementWithUnsafeContent(container);

            container.Load(containerElement);

            Assert.AreEqual(container.Content.Count, UnsafeElementsThreshold);

            foreach (var child in container.Content)
            {
                // so nods are not skipped, but marked as unsafe
                Assert.IsTrue(child.Unsafe);
            }

            var serialized = container.ToXml();

            Assert.AreEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Container_Load_SkipsInvalidNode_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var containerElement = GetXElementWithInvalidContent(container.Name);

            container.Load(containerElement);

            Assert.AreEqual(0, container.Content.Count);

            var serialized = container.ToXml();

            Assert.AreNotEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Container_WithContent_NullNode(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            Assert.ThrowsException<ArgumentNullException>(() => container.WithContent((Fb2Node)null));
        }

        public void Fb2Container_WithContent_CantContainText(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            if (container.CanContainText)
                return;

            Assert.ThrowsException<ArgumentException>(() => container.WithContent(new TextItem()));
        }

        public void Fb2Container_WithContent_UnsafeContent(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var unsafeNodes = Fb2ElementFactory.Instance.KnownNodes.Keys
                .Where(k => !container.AllowedElements.Contains(k))
                .Skip(2).Take(3).ToList(); // random kinda

            foreach (var nodeName in unsafeNodes)
            {
                var node = Fb2ElementFactory.Instance.GetElementByNodeName(nodeName);
                Assert.ThrowsException<ArgumentException>(() => container.WithContent(node));
            }
        }

        public void Fb2Container_WithContainer_ValidContent(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var allowedChilderNames = container.AllowedElements;
            foreach (var name in allowedChilderNames)
            {
                var node = Fb2ElementFactory.Instance.GetElementByNodeName(name);
                container.WithContent(node);
            }

            Assert.AreEqual(container.Content.Count, container.AllowedElements.Count);
        }

        public void Fb2Container_WithContainer_Params_NullNodes(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            Assert.ThrowsException<ArgumentNullException>(() => container.WithContent((Fb2Node)null, (Fb2Node)null));
        }

        public void Fb2Container_WithContainer_Params_InvalidNode_Rollback(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var firstValidNode = Fb2ElementFactory.Instance.GetElementByNodeName(container.AllowedElements.First());
            var lastValidNode = Fb2ElementFactory.Instance.GetElementByNodeName(container.AllowedElements.Last());
            var unsafeNodeName = Fb2ElementFactory.Instance.KnownNodes.Keys
                .Where(k => !container.AllowedElements.Contains(k)).LastOrDefault();
            var unsafeNode = Fb2ElementFactory.Instance.GetElementByNodeName(unsafeNodeName);

            Assert.ThrowsException<ArgumentException>(() => container.WithContent(firstValidNode, lastValidNode, unsafeNode));
            Assert.AreEqual(container.Content.Count, 0);
        }

        public void Fb2Container_WithContainer_Params_InvalidNode_Rollback_ToContent(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var allowedChilderNames = container.AllowedElements;
            foreach (var name in allowedChilderNames)
            {
                var node = Fb2ElementFactory.Instance.GetElementByNodeName(name);
                container.WithContent(node);
            }

            Assert.AreEqual(container.Content.Count, container.AllowedElements.Count);

            var firstValidNode = Fb2ElementFactory.Instance.GetElementByNodeName(container.AllowedElements.First());
            var lastValidNode = Fb2ElementFactory.Instance.GetElementByNodeName(container.AllowedElements.Last());
            var unsafeNodeName = Fb2ElementFactory.Instance.KnownNodes.Keys
                .Where(k => !container.AllowedElements.Contains(k)).LastOrDefault();
            var unsafeNode = Fb2ElementFactory.Instance.GetElementByNodeName(unsafeNodeName);

            Assert.ThrowsException<ArgumentException>(() => container.WithContent(firstValidNode, lastValidNode, unsafeNode));
            Assert.AreEqual(container.Content.Count, container.AllowedElements.Count); // rolled content back
        }
    }
}

using System;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
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
            var assembly = Fb2ElementFactory.Instance.GetType().Assembly;

            var modelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                       !type.IsAbstract && type.IsClass).ToList();

            var containerTypes = modelTypes.Where(ct => ct.IsSubclassOf(typeof(Fb2Container))).ToList();

            foreach (var modelType in containerTypes)
            {
                Fb2Container_Load_Content_Test(modelType);
                Fb2Container_Load_UnsafeContent_Test(modelType);
                Fb2Container_Load_SkipsInvalidNode_Test(modelType);
            }
        }

        public void Fb2Container_Load_Content_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

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
    }
}

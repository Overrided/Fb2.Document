using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Base;
using Fb2.Document.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests
{
    [TestClass]
    public class ModelsTests : BaseTest
    {
        [TestMethod]
        public void Load_And_Parsing_Tests()
        {
            var assembly = Fb2ElementFactory.Instance.GetType().Assembly;

            var modelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                       !type.IsAbstract && type.IsClass).ToList();

            CheckIfCorrectModelsAreLoaded(modelTypes);

            var containerTypes = modelTypes.Where(ct => ct.IsSubclassOf(typeof(Fb2Container))).ToList();

            var elementTypes = modelTypes.Where(et => et.IsSubclassOf(typeof(Fb2Element))).ToList();

            Assert.AreEqual(modelTypes.Count, containerTypes.Count + elementTypes.Count);

            foreach (var modelType in containerTypes)
            {
                Fb2Container_Load_Content_Test(modelType);
                Fb2Container_Load_UnsafeContent_Test(modelType);
                Fb2Container_Load_Attributes_Test(modelType);
                Fb2Container_Load_SkipsInvalidNode_Test(modelType);
                Fb2Container_Load_SkipsInvalidAttributes_Test(modelType);
            }

            foreach (var modelType in elementTypes)
            {
                Fb2Element_Load_Content_Test(modelType);
                Fb2Element_Load_LinearizeMultilineContent_Test(modelType);
                Fb2Element_Load_Attributes_Test(modelType);
                Fb2Element_Load_SkipsInvalidNode_Test(modelType);
            }
        }

        // Fb2Container checks

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
                {
                    Assert.AreEqual(Environment.NewLine, child.ToString());
                }
                else if (child.Name == ElementNames.FictionText)
                {
                    Assert.AreEqual($"test text", child.ToString());
                }
                else
                {
                    Assert.AreEqual($"test {child.Name} text", child.ToString());
                }
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

        public void Fb2Container_Load_Attributes_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var containerElement = GetXElementWithAttributes(container);

            container.Load(containerElement);

            Assert.AreEqual(container.Content.Count, 0);

            if (container.AllowedAttributes == null || !container.AllowedAttributes.Any())
                Assert.IsNull(container.Attributes);
            else
            {
                Assert.AreEqual(container.Attributes.Count, container.AllowedAttributes.Count);

                foreach (var attr in container.Attributes)
                {
                    Assert.IsTrue(container.AllowedAttributes.Contains(attr.Key));
                    Assert.AreEqual($"{attr.Key} test value", attr.Value);
                }
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

        public void Fb2Container_Load_SkipsInvalidAttributes_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Container>(modelType);

            var containerElement = GetXElementWithInvalidAttribute(container.Name);

            container.Load(containerElement);

            Assert.IsNull(container.Attributes);

            var serialized = container.ToXml();

            Assert.AreNotEqual(containerElement.ToString(), serialized.ToString());
        }

        // Fb2Element checks

        public void Fb2Element_Load_Content_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Element>(modelType);

            var containerElement = GetXElementWithSimpleStringContent(container.Name);

            container.Load(containerElement);

            if (container.Name == ElementNames.Image)
                Assert.AreEqual("image", container.ToString());
            else if (container.Name == ElementNames.EmptyLine)
                Assert.AreEqual(Environment.NewLine, container.Content);
            else
                Assert.AreEqual("simple test text", container.Content);

            var serialized = container.ToXml().ToString();

            if (container.Name == ElementNames.EmptyLine) // empty line should have not content
                Assert.AreEqual("<empty-line />", serialized);
            else
                Assert.AreEqual(containerElement.ToString(), serialized);
        }

        public void Fb2Element_Load_LinearizeMultilineContent_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Element>(modelType);

            var containerElement = GetXElementWithMultilineStringContent(container.Name);

            container.Load(containerElement);

            if (container.Name == ElementNames.EmptyLine) // empty line should have no content
                Assert.AreEqual(Environment.NewLine, container.Content);
            else
                Assert.AreEqual(" row 1 row 2 row 3 ", container.Content);
        }

        public void Fb2Element_Load_Attributes_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Element>(modelType);

            var containerElement = GetXElementWithAttributes(container);

            container.Load(containerElement);

            if (container.Name == ElementNames.EmptyLine)
                Assert.AreEqual(Environment.NewLine, container.Content);
            else
                Assert.AreEqual(string.Empty, container.Content);

            if (container.AllowedAttributes == null || !container.AllowedAttributes.Any())
                Assert.IsNull(container.Attributes);
            else
            {
                Assert.AreEqual(container.Attributes.Count, container.AllowedAttributes.Count);

                foreach (var attr in container.Attributes)
                {
                    Assert.IsTrue(container.AllowedAttributes.Contains(attr.Key));
                    Assert.AreEqual($"{attr.Key} test value", attr.Value);
                }
            }

            var serialized = container.ToXml();

            Assert.AreEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Element_Load_SkipsInvalidNode_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Element>(modelType);

            var containerElement = GetXElementWithInvalidAttribute(container.Name);

            container.Load(containerElement);

            Assert.IsNull(container.Attributes);

            var serialized = container.ToXml();

            Assert.AreNotEqual(containerElement.ToString(), serialized.ToString());
        }

        // owerall checks

        private void CheckIfCorrectModelsAreLoaded(List<Type> modelTypes)
        {
            var instances = modelTypes.Select(mt => Utils.Instantiate<Fb2Node>(mt)).ToList();
            var instancesNames = instances.Select(instance => instance.Name).ToList();

            var names = new ElementNames();
            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            CollectionAssert.AreEquivalent(elementNames, instancesNames); // so we do know all models are loaded
        }
    }
}

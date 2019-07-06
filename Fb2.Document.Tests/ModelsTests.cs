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
                Fb2Container_Load_Attributes_Test(modelType);
                Fb2Container_Load_SkipsInvalidNode_Test(modelType);
                Fb2Container_Load_SkipsInvalidAttributes_Test(modelType);
            }
        }

        public void Fb2Container_Load_Content_Test(Type modelType)
        {
            if (!modelType.IsSubclassOf(typeof(Fb2Container)))
                throw new ArgumentException($"{nameof(modelType)} is of wrong type! Expected Fb2Container descendant!");

            var container = Utils.Instantiate<Fb2Container>(modelType);

            var containerElement = GetXElement(container);

            container.Load(containerElement);

            Assert.AreEqual(container.Content.Count, container.AllowedElements.Count);

            var childTypes = container.Content.Select(child => child.GetType()).ToList();

            CollectionAssert.AreEqual(childTypes, childTypes.Distinct().ToList());

            foreach (var child in container.Content)
            {
                // TODO : add checks for overrides
                if (CheckIfToStringIsOverriden(child) || child.Name == ElementNames.EmptyLine)
                    continue; // no checks for specific overrides yet
                else
                {
                    Assert.AreEqual($"test {child.Name} text", child.ToString());
                }
            }

            var serialized = container.ToXml();

            Assert.AreEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Container_Load_Attributes_Test(Type modelType)
        {
            if (!modelType.IsSubclassOf(typeof(Fb2Container)))
                throw new ArgumentException($"{nameof(modelType)} is of wrong type! Expected Fb2Container descendant!");

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

        private void CheckIfCorrectModelsAreLoaded(List<Type> modelTypes)
        {
            var instances = modelTypes.Select(mt => Utils.Instantiate<Fb2Node>(mt)).ToList();
            var instancesNames = instances.Select(instance => instance.Name).ToList();

            var names = new ElementNames();
            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            CollectionAssert.AreEquivalent(elementNames, instancesNames); // so we do know all models are loaded
        }

        private bool CheckIfToStringIsOverriden(Fb2Node node)
        {
            var nodeType = node.GetType();
            var methodInfo = nodeType.GetMethod("ToString");

            if (methodInfo == null)
            {
                throw new ApplicationException("No ToString metod info!");
            }

            var isOverriden = methodInfo.DeclaringType == nodeType;

            return isOverriden;
        }
    }
}

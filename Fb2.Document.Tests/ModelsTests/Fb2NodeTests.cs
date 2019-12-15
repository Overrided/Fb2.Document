using System;
using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Base;
using Fb2.Document.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests.ModelsTests
{
    [TestClass]
    public class Fb2NodeTests : TestBase
    {
        [TestMethod]
        public void Smoke_Tests()
        {
            var assembly = Fb2ElementFactory.Instance.GetType().Assembly;

            var modelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                       !type.IsAbstract && type.IsClass).ToList();

            CheckIfCorrectModelsAreLoaded(modelTypes);

            foreach (var modelType in modelTypes)
            {
                Fb2Node_Load_SkipsInvalidAttributes_Test(modelType);
                Fb2Node_Load_InvalidNode_ThrowsException(modelType);
            }
        }

        // owerall checks
        // TODO: add tests for HasAttribute, GetAttributeValue, etc.

        public void CheckIfCorrectModelsAreLoaded(List<Type> modelTypes)
        {
            var instances = modelTypes.Select(mt => Utils.Instantiate<Fb2Node>(mt)).ToList();
            var instancesNames = instances.Select(instance => instance.Name).ToList();

            var names = new ElementNames();
            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            CollectionAssert.AreEquivalent(elementNames, instancesNames); // so we do know all models are loaded
        }

        public void Fb2Node_Load_SkipsInvalidAttributes_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Node>(modelType);

            var containerElement = GetXElementWithInvalidAttribute(container.Name);

            container.Load(containerElement);

            Assert.IsNull(container.Attributes);

            var serialized = container.ToXml();

            Assert.AreNotEqual(containerElement.ToString(), serialized.ToString());
        }

        public void Fb2Node_Load_InvalidNode_ThrowsException(Type modelType)
        {
            var element = Utils.Instantiate<Fb2Node>(modelType);

            if (element.Name == ElementNames.EmptyLine)
                return; // EmptyLine has specific override for Load method, no validation is applied

            var invalidNode = GetInvalidXNode();

            var argumentExThrown = false;
            var exThrown = false;

            try
            {
                element.Load(invalidNode);
            }
            catch (ArgumentException agex)
            {
                argumentExThrown = true;
                Assert.AreEqual(agex.Message, $"Invalid element, local name {InvalidNodeName}, supposed name {element.Name}");
            }
            catch
            {
                exThrown = true;
            }

            Assert.IsTrue(argumentExThrown);
            Assert.IsFalse(exThrown);
        }
    }
}

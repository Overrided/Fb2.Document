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
                Fb2Node_Load_InvalidNode_ThrowsException(modelType);
                Fb2Node_Load_Attributes_Test(modelType);
                Fb2Node_AttributeAccessMethods_Test(modelType);
                Fb2Node_Load_SkipsInvalidAttributes_Test(modelType);
            }
        }

        public void CheckIfCorrectModelsAreLoaded(List<Type> modelTypes)
        {
            var instances = modelTypes.Select(mt => Utils.Instantiate<Fb2Node>(mt)).ToList();
            var instancesNames = instances.Select(instance => instance.Name).ToList();

            var names = new ElementNames();
            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            CollectionAssert.AreEquivalent(elementNames, instancesNames); // so we do know all models are loaded
        }

        public void Fb2Node_Load_InvalidNode_ThrowsException(Type modelType)
        {
            var element = Utils.Instantiate<Fb2Node>(modelType);

            var invalidNode = GetInvalidXNode();

            Assert.ThrowsException<ArgumentException>(() => { element.Load(invalidNode); });
        }

        public void Fb2Node_Load_Attributes_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Node>(modelType);

            var containerElement = GetXElementWithAttributes(container);

            container.Load(containerElement);

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

        public void Fb2Node_AttributeAccessMethods_Test(Type modelType)
        {
            var container = Utils.Instantiate<Fb2Node>(modelType);

            var containerElement = GetXElementWithAttributes(container);

            container.Load(containerElement);

            if (container.AllowedAttributes == null || !container.AllowedAttributes.Any())
                Assert.IsNull(container.Attributes);
            else
            {
                Assert.AreEqual(container.Attributes.Count, container.AllowedAttributes.Count);

                var casedAllowedAttrs = container.AllowedAttributes.Select(aa => string.Join(string.Empty,
                    aa.Select((char ch, int index) =>
                    {
                        return index % 2 == 0 ? char.ToUpper(ch) : ch;
                    })));

                foreach (var attr in casedAllowedAttrs)
                {
                    Assert.IsTrue(container.HasAttribute(attr, true));
                    Assert.IsFalse(container.HasAttribute(attr, false));

                    var attributeCaseInvariant = container.GetAttribute(attr, true);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(attributeCaseInvariant.Key));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(attributeCaseInvariant.Value));
                    Assert.AreEqual($"{attributeCaseInvariant.Key} test value", attributeCaseInvariant.Value);

                    var attributeCaseSencitive = container.GetAttribute(attr, false);
                    Assert.IsTrue(string.IsNullOrWhiteSpace(attributeCaseSencitive.Key));
                    Assert.IsTrue(string.IsNullOrWhiteSpace(attributeCaseSencitive.Value));

                    var tryCaseInvariant = container.TryGetAttribute(attr, true, out var resultInvariant);
                    Assert.IsTrue(tryCaseInvariant);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(resultInvariant.Key));
                    Assert.IsFalse(string.IsNullOrWhiteSpace(resultInvariant.Value));
                    Assert.AreEqual($"{resultInvariant.Key} test value", resultInvariant.Value);

                    var tryCaseSensitive = container.TryGetAttribute(attr, false, out var resultSensitive);
                    Assert.IsFalse(tryCaseSensitive);
                }
            }

            Assert.IsFalse(container.HasAttribute(InvalidAttributeName, true));
            Assert.IsFalse(container.HasAttribute(InvalidAttributeName, false));

            Assert.IsTrue(string.IsNullOrWhiteSpace(container.GetAttribute(InvalidAttributeName, true).Key));
            Assert.IsTrue(string.IsNullOrWhiteSpace(container.GetAttribute(InvalidAttributeName, false).Key));

            Assert.IsFalse(container.TryGetAttribute(InvalidAttributeName, true, out var invalidIgnoreCase));
            Assert.IsFalse(container.TryGetAttribute(InvalidAttributeName, false, out var invalid));

            Assert.ThrowsException<ArgumentNullException>(() => { container.HasAttribute(null, true); });
            Assert.ThrowsException<ArgumentNullException>(() => { container.GetAttribute(string.Empty, true); });
            Assert.ThrowsException<ArgumentNullException>(() => { container.TryGetAttribute(null, true, out var res); });

            var serialized = container.ToXml();

            Assert.AreEqual(containerElement.ToString(), serialized.ToString());
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
    }
}

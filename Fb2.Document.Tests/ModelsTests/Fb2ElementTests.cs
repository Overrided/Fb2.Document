using System;
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
    public class Fb2ElementTests : TestBase
    {
        [TestMethod]
        public void Fb2Element_Tests()
        {
            var assembly = Fb2ElementFactory.Instance.GetType().Assembly;

            var modelTypes = assembly.GetExportedTypes()
                .Where(type => type.FullName.StartsWith("Fb2.Document.Models.") &&
                       !type.IsAbstract && type.IsClass).ToList();

            var elementTypes = modelTypes.Where(et => et.IsSubclassOf(typeof(Fb2Element))).ToList();

            foreach (var modelType in elementTypes)
            {
                Fb2Element_Load_Content_Test(modelType);
                Fb2Element_Load_LinearizeMultilineContent_Test(modelType);
            }
        }

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
    }
}

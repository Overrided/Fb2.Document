using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Tests.Base;
using Fb2.Document.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests
{
    [TestClass]
    public class Fb2ElementFactoryTests : BaseTest
    {
        [TestMethod]
        public void Check_Known_Nodes_Test()
        {
            var names = new ElementNames();

            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            Assert.AreEqual(63, elementNames.Count); // no node will be added / removed without noticing

            foreach (var elemName in elementNames)
            {
                Assert.IsTrue(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(elemName));

                var node = Fb2ElementFactory.Instance.GetElementByNodeName(elemName);

                Assert.IsNotNull(node);

                node = null;
            }
        }

        [TestMethod]
        public void InvalidNodeName_ReturnsNull()
        {
            Assert.IsFalse(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(InvalidNodeName));

            var node = Fb2ElementFactory.Instance.GetElementByNodeName(InvalidNodeName);

            Assert.IsNull(node);
        }
    }
}

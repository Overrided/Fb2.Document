using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Tests.Base;
using Fb2.Document.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fb2.Document.Tests
{
    [TestClass]
    public class Fb2ElementFactoryTests : TestBase
    {
        [TestMethod]
        public void SmokeTest()
        {
            var names = new ElementNames();

            var elementNames = Utils.GetAllFieldsOfType<ElementNames, string>(names);

            Assert.AreEqual(63, elementNames.Count); // no node will be added / removed without noticing

            foreach (var elemName in elementNames)
            {
                Assert.IsTrue(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(elemName));

                var node = Fb2ElementFactory.Instance.GetElementByNodeName(elemName);

                Assert.IsNotNull(node);
                Assert.AreEqual(elemName, node.Name);
            }
        }

        [TestMethod]
        public void InvalidNodeName_ReturnsNull()
        {
            Assert.IsFalse(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(InvalidNodeName));

            var node = Fb2ElementFactory.Instance.GetElementByNodeName(InvalidNodeName);

            Assert.IsNull(node);
        }

        [TestMethod]
        public void CaseInvariantNodeName_ReturnsInstance()
        {
            var titleInfoCasedNodeName = "tItLe-iNFo";
            var strikethroughCasedNodeName = "sTrIkEtHrOuGh";

            Assert.IsFalse(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(titleInfoCasedNodeName));
            Assert.IsFalse(Fb2ElementFactory.Instance.KnownNodes.ContainsKey(strikethroughCasedNodeName));

            var titleInfo = Fb2ElementFactory.Instance.GetElementByNodeName(titleInfoCasedNodeName);
            var strikethrough = Fb2ElementFactory.Instance.GetElementByNodeName(strikethroughCasedNodeName);

            Assert.IsNotNull(titleInfo);
            Assert.IsNotNull(strikethrough);

            Assert.AreEqual(titleInfo.Name, ElementNames.TitleInfo);
            Assert.AreEqual(strikethrough.Name, ElementNames.Strikethrough);
        }
    }
}

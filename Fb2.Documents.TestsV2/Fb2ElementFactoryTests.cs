using Fb2.Document.Constants;
using Fb2.Document.Factories;
using FluentAssertions;
using Xunit;

namespace Fb2.Documents.TestsV2
{
    public class Fb2ElementFactoryTests
    {
        [Fact]
        public void IsKnownNode_InvalidNodeName_ReturnsFalse()
        {
            var invalidNodeName = "invalidNodeName";
            var node = Fb2ElementFactory.IsKnownNode(invalidNodeName);
            node.Should().BeFalse();
        }

        [Fact]
        public void GetElementByNodeName_InvalidNodeName_ReturnsNull()
        {
            var invalidNodeName = "invalidNodeName";
            var node = Fb2ElementFactory.GetNodeByName(invalidNodeName);
            node.Should().BeNull();
        }

        [Fact]
        public void ValidNodeName_CaseInvariantNodeName_ReturnsInstance()
        {
            var titleInfoCasedNodeName = "tItLe-iNFo";
            var strikethroughCasedNodeName = "sTrIkEtHrOuGh";

            Fb2ElementFactory.IsKnownNode(titleInfoCasedNodeName).Should().BeTrue();
            Fb2ElementFactory.IsKnownNode(strikethroughCasedNodeName).Should().BeTrue();

            var titleInfo = Fb2ElementFactory.GetNodeByName(titleInfoCasedNodeName);
            var strikethrough = Fb2ElementFactory.GetNodeByName(strikethroughCasedNodeName);

            titleInfo.Should().NotBeNull();
            strikethrough.Should().NotBeNull();

            titleInfo.Name.Should().Be(ElementNames.TitleInfo);
            strikethrough.Name.Should().Be(ElementNames.Strikethrough);
        }
    }
}

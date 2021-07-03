using System;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Factories;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests
{
    public class Fb2NodeFactoryTests
    {
        [Fact]
        public void IsKnownNode_InvalidNodeName_ReturnsFalse()
        {
            var invalidNodeName = "invalidNodeName";
            var node = Fb2NodeFactory.IsKnownNode(invalidNodeName);
            node.Should().BeFalse();
        }

        [Fact]
        public void GetElementByNodeName_InvalidNodeName_Throws()
        {
            var invalidNodeName = "invalidNodeName";

            Action act = () => { var node = Fb2NodeFactory.GetNodeByName(invalidNodeName); };

            act.Should().Throw<UnknownNodeNameException>()
                .WithMessage($"'{invalidNodeName}' is not valid Fb2 node name.");
        }

        [Fact]
        public void ValidNodeName_CaseInvariantNodeName_ReturnsInstance()
        {
            var titleInfoCasedNodeName = "tItLe-iNFo";
            var strikethroughCasedNodeName = "sTrIkEtHrOuGh";

            Fb2NodeFactory.IsKnownNode(titleInfoCasedNodeName).Should().BeTrue();
            Fb2NodeFactory.IsKnownNode(strikethroughCasedNodeName).Should().BeTrue();

            var titleInfo = Fb2NodeFactory.GetNodeByName(titleInfoCasedNodeName);
            var strikethrough = Fb2NodeFactory.GetNodeByName(strikethroughCasedNodeName);

            titleInfo.Should().NotBeNull();
            strikethrough.Should().NotBeNull();

            titleInfo.Name.Should().Be(ElementNames.TitleInfo);
            strikethrough.Name.Should().Be(ElementNames.Strikethrough);
        }
    }
}

using System;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using Fb2.Document.Tests.Common;
using Fb2.Document.Tests.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests;

public class Fb2NodeFactoryTests
{
    [Theory()]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void IsKnownNodeName_NullOrEmptyName_Throws(string? nodeName)
    {
        Action act = () => { var node = Fb2NodeFactory.IsKnownNodeName(nodeName!); };

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsKnownNode_NullNode_Throws()
    {
        Action act = () => { var node = Fb2NodeFactory.IsKnownNode(null!); };

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory()]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void GetNodeByName_NullOrEmptyName_Throws(string? nodeName)
    {
        Action act = () => { var node = Fb2NodeFactory.GetNodeByName(nodeName!); };

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsKnownNodeName_InvalidNodeName_ReturnsFalse()
    {
        var invalidNodeName = "invalidNodeName";
        Fb2NodeFactory.IsKnownNodeName(invalidNodeName).Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeNameCollection))]
    public void IsKnownNodeName_ValidNodeName_ReturnsTrue(string nodeName)
    {
        Fb2NodeFactory.IsKnownNodeName(nodeName).Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(Fb2NodeCollection))]
    public void IsKnownNode_ValidNode_ReturnsTrue(Fb2Node node)
    {
        Fb2NodeFactory.IsKnownNode(node).Should().BeTrue();
    }

    [Fact]
    public void IsKnownNode_ImpostorNode_ReturnsFalse()
    {
        var impostor = new ImpostorNode();
        Fb2NodeFactory.IsKnownNode(impostor).Should().BeFalse();

        var sneakyImpostor = new ImpostorNode(ElementNames.Paragraph);
        Fb2NodeFactory.IsKnownNode(sneakyImpostor).Should().BeFalse();
    }

    [Fact]
    public void GetElementByNodeName_InvalidNodeName_Throws()
    {
        var invalidNodeName = "invalidNodeName";

        Action act = () => { var node = Fb2NodeFactory.GetNodeByName(invalidNodeName); };

        act.Should().Throw<InvalidNodeException>()
            .WithMessage($"'{invalidNodeName}' is not known Fb2 node name.");
    }

    [Fact]
    public void ValidNodeName_CaseInvariantNodeName_Works()
    {
        var titleInfoCasedNodeName = "tItLe-iNFo";
        var strikethroughCasedNodeName = "sTrIkEtHrOuGh";

        Fb2NodeFactory.IsKnownNodeName(titleInfoCasedNodeName).Should().BeTrue();
        Fb2NodeFactory.IsKnownNodeName(strikethroughCasedNodeName).Should().BeTrue();

        var titleInfo = Fb2NodeFactory.GetNodeByName(titleInfoCasedNodeName);
        var strikethrough = Fb2NodeFactory.GetNodeByName(strikethroughCasedNodeName);

        titleInfo.Should().NotBeNull();
        strikethrough.Should().NotBeNull();

        titleInfo.Name.Should().Be(ElementNames.TitleInfo);
        strikethrough.Name.Should().Be(ElementNames.Strikethrough);
    }
}

using System.Collections.Generic;
using System.Linq;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using Fb2.Documents.TestsV2.DataCollections;
using FluentAssertions;
using Xunit;

namespace Fb2.Documents.TestsV2.ModelsTests
{
    public class Fb2NodeTests
    {
        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void EmptyNodes_EqualityNullTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var equals = instanceOne.Equals(null);
            equals.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void EmptyNodes_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var instanceTwo = Fb2ElementFactory.GetNodeByName(nodeName);

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeTrue();
            instanceOne.Should().BeEquivalentTo(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void CloneNode_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            var instanceTwo = instanceOne.Clone();

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeTrue();
            instanceOne.Should().BeEquivalentTo(instanceTwo);
        }

        [Theory]
        [ClassData(typeof(Fb2NodeNamesData))]
        public void CloneNode_WithAttribute_EqualityTest(string nodeName)
        {
            var instanceOne = Fb2ElementFactory.GetNodeByName(nodeName);
            if (instanceOne.AllowedAttributes == null || !instanceOne.AllowedAttributes.Any())
                return;

            var instanceTwo = instanceOne.Clone() as Fb2Node;
            instanceTwo.WithAttribute(() => new KeyValuePair<string, string>(instanceOne.AllowedAttributes.First(), "testValue"));

            var equals = instanceOne.Equals(instanceTwo);
            equals.Should().BeFalse();

            instanceOne.Attributes().Should().BeEmpty();
            instanceTwo.Attributes().Should().NotBeEmpty();
            instanceTwo.Attributes().Count.Should().Be(1);
        }
    }
}

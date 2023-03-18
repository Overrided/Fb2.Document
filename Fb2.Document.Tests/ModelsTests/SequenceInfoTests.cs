using System;
using System.Collections.Generic;
using System.Text;
using Fb2.Document.Constants;
using Fb2.Document.Factories;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ModelsTests
{
    public class SequenceInfoTests
    {
        [Fact]
        public void SequenceInfo_AddContent_Ignored()
        {
            var sequenceInfo = Fb2NodeFactory.GetNodeByName(ElementNames.Sequence) as Fb2Element;
            sequenceInfo.Should().NotBeNull();

            sequenceInfo!.Content.Should().BeEmpty();

            sequenceInfo.AddContent("hello world");

            sequenceInfo!.Content.Should().BeEmpty();

            sequenceInfo.ClearContent();

            sequenceInfo!.Content.Should().BeEmpty();
        }

        [Fact]
        public void SequenceInfo_ToString_NoAttributes_ReturnsEmptyString()
        {
            var sequenceInfo = Fb2NodeFactory.GetNodeByName(ElementNames.Sequence) as Fb2Element;
            sequenceInfo.Should().NotBeNull();

            sequenceInfo!.Content.Should().BeEmpty();

            var sequenceInfoString = sequenceInfo.ToString();
            sequenceInfoString.Should().NotBeNull();
            sequenceInfoString.Should().BeEmpty();
        }

        [Fact]
        public void SequenceInfo_ToString_NameAttributeOnly()
        {
            var sequenceInfo = Fb2NodeFactory.GetNodeByName(ElementNames.Sequence) as Fb2Element;
            sequenceInfo.Should().NotBeNull();

            sequenceInfo!.Content.Should().BeEmpty();
            sequenceInfo.HasAttributes.Should().BeFalse();

            var testSequenceName = "Test Sequence Name";
            sequenceInfo.AddAttribute(AttributeNames.Name, testSequenceName);

            sequenceInfo!.Content.Should().BeEmpty();
            sequenceInfo.HasAttributes.Should().BeTrue();

            var sequenceInfoString = sequenceInfo.ToString();
            sequenceInfoString.Should().NotBeNullOrEmpty();
            sequenceInfoString.Should().Be(testSequenceName);
        }

        [Fact]
        public void SequenceInfo_ToString_NumberAttributeOnly()
        {
            var sequenceInfo = Fb2NodeFactory.GetNodeByName(ElementNames.Sequence) as Fb2Element;
            sequenceInfo.Should().NotBeNull();

            sequenceInfo!.Content.Should().BeEmpty();
            sequenceInfo.HasAttributes.Should().BeFalse();

            var testSequenceNumber = 2;
            sequenceInfo.AddAttribute(AttributeNames.Number, testSequenceNumber.ToString());

            sequenceInfo!.Content.Should().BeEmpty();
            sequenceInfo.HasAttributes.Should().BeTrue();

            var sequenceInfoString = sequenceInfo.ToString();
            sequenceInfoString.Should().NotBeNullOrEmpty();
            sequenceInfoString.Should().Be(testSequenceNumber.ToString());
        }
    }
}

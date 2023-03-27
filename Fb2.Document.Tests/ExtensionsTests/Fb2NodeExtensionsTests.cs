﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Constants;
using Fb2.Document.Extensions;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;
using FluentAssertions;
using Xunit;

namespace Fb2.Document.Tests.ExtensionsTests
{
    public class Fb2NodeExtensionsTests
    {
        [Fact]
        public async Task AppendAttribute_Works()
        {
            var parag = new Paragraph();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            parag.AppendAttribute(new Fb2Attribute(AttributeNames.Id, "testId"));
            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(1);

            parag.EraseAttributes();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            parag.AppendAttribute(() => new Fb2Attribute(AttributeNames.Id, "testId"));
            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(1);

            parag.EraseAttributes();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            parag.AppendAttribute(AttributeNames.Id, "testId");
            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(1);

            parag.EraseAttributes();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            await parag.AppendAttributeAsync(() => Task.FromResult(new Fb2Attribute(AttributeNames.Id, "testId")));

            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(1);
        }

        [Fact]
        public async Task AppendAttributes_Works()
        {
            var parag = new Paragraph();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            parag.AppendAttributes(
                new Fb2Attribute(AttributeNames.Id, "testId"),
                new Fb2Attribute(AttributeNames.Language, "ua"));
            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(2);

            parag.EraseAttributes();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();

            parag.AppendAttributes(new List<Fb2Attribute> {
                new Fb2Attribute(AttributeNames.Id, "testId"),
                new Fb2Attribute(AttributeNames.Language, "ua")
            });
            parag.HasAttributes.Should().BeTrue();
            parag.Attributes.Should().NotBeEmpty().And.HaveCount(2);

            parag.EraseAttributes();
            parag.HasAttributes.Should().BeFalse();
            parag.Attributes.Should().BeEmpty();
        }
    }
}

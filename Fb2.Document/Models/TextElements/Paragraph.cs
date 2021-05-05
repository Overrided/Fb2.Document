﻿using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Paragraph : TextContainer
    {
        public override string Name => ElementNames.Paragraph;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(AttributeNames.Id, AttributeNames.Language);

        public override bool IsInline => false;
    }
}

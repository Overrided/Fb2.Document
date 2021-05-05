﻿using System.Collections.Immutable;
using Fb2.Document.Constants;

namespace Fb2.Document.Models.Base
{
    public abstract class TextContainer : Fb2Container
    {
        public override bool IsInline => true;

        public override bool CanContainText => true;

        public override ImmutableHashSet<string> AllowedElements =>
            ImmutableHashSet.Create(
                ElementNames.Strong,
                ElementNames.Emphasis,
                ElementNames.TextStyle,
                ElementNames.TextLink,
                ElementNames.Strikethrough,
                ElementNames.Subscript,
                ElementNames.Superscript,
                ElementNames.Code,
                ElementNames.Image);
    }
}

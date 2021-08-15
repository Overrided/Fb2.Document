﻿using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class CustomInfo : Fb2Element
    {
        public override string Name => ElementNames.CustomInfo;

        public override bool IsInline => false;

        public override ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet.Create(AttributeNames.InfoType);

        // `preserveWhitespace` ignored due to "Custom Info" part is text-only, so could be `\t` and `\r\n` formatted??
        public override void Load(
            [In] XNode node,
            bool preserveWhitespace = false,
            bool loadUnsafe = true) =>
            base.Load(node, true, loadUnsafe);
    }
}

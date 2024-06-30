﻿using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Stanza : Fb2Container
{
    public override string Name => ElementNames.Stanza;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements => [ElementNames.Title, ElementNames.SubTitle, ElementNames.StanzaV];
}

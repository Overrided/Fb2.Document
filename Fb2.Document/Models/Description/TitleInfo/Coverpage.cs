using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Coverpage : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedElements = [ElementNames.Image];

    public override string Name => ElementNames.Coverpage;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;
}

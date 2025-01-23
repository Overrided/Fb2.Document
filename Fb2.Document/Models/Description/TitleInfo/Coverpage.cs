using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Coverpage : Fb2Container
{
    public override string Name => ElementNames.Coverpage;

    public override bool CanContainText => false;

    public override FrozenSet<string> AllowedElements => [ElementNames.Image];
}

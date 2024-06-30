using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class FictionId : Fb2Element
{
    public override string Name => ElementNames.FictionId;

    public override bool IsInline => false;
}

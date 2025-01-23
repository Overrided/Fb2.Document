using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class LastName : Fb2Element
{
    public override string Name => ElementNames.LastName;

    public override bool IsInline => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Language];
}

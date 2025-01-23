using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class SubTitle : TextContainerBase
{
    public override string Name => ElementNames.SubTitle;

    public override bool IsInline => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Id, AttributeNames.Language];
}

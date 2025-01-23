using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextStyle : TextContainerBase
{
    public override string Name => ElementNames.TextStyle;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Name, AttributeNames.Language];
}

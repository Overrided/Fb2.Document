using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextStyle : TextContainerBase
{
    public override string Name => ElementNames.TextStyle;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.Name, AttributeNames.Language];
}

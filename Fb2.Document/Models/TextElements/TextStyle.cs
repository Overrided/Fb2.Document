using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextStyle : TextContainerBase
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Name, AttributeNames.Language];

    public override string Name => ElementNames.TextStyle;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

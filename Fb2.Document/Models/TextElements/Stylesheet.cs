using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Stylesheet : Fb2Element
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Type];

    public override string Name => ElementNames.Stylesheet;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

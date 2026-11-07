using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class ISBNInfo : Fb2Element
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Language];

    public override string Name => ElementNames.ISBN;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

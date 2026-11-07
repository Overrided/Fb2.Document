using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextAuthor : TextContainerBase
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Id];

    public override string Name => ElementNames.TextAuthor;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class SubTitle : TextContainerBase
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Id, AttributeNames.Language];

    public override string Name => ElementNames.SubTitle;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

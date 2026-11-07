using System.Collections.Immutable;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Publisher : CreatorBase
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Language];

    public override string Name => ElementNames.Publisher;

    public override bool CanContainText => true;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;
}

using System.Collections.Immutable;
using System.Text;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TextLink : TextContainerBase
{
    public override string Name => ElementNames.TextLink;

    public override ImmutableHashSet<string> AllowedAttributes =>
        ImmutableHashSet.Create(AttributeNames.XHref, AttributeNames.Type);

    public sealed override string ToString()
    {
        var builder = new StringBuilder(base.ToString());

        if (TryGetAttribute(AttributeNames.XHref, true, out var hrefAttr))
            builder.Append(builder.Length > 0 ? $" ({hrefAttr!.Value})" : hrefAttr!.Value);

        return builder.ToString();
    }
}

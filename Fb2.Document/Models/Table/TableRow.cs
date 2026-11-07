using System.Collections.Immutable;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TableRow : Fb2Container
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [AttributeNames.Align];

    private static readonly ImmutableHashSet<string> allowedElements = [ElementNames.TableHeader, ElementNames.TableCell];

    public override string Name => ElementNames.TableRow;

    public override bool CanContainText => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public override ImmutableHashSet<string> AllowedElements => allowedElements;

    public sealed override string ToString() => string.Join(Whitespace, Content.Select(cell => $"{cell}{Whitespace}|"));
}

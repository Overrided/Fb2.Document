using System.Collections.Frozen;
using System.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class TableRow : Fb2Container
{
    public override string Name => ElementNames.TableRow;

    public override bool CanContainText => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Align];

    public override FrozenSet<string> AllowedElements => [ElementNames.TableHeader, ElementNames.TableCell];

    public sealed override string ToString() => string.Join(Whitespace, Content.Select(cell => $"{cell}{Whitespace}|"));
}

using System.Collections.Frozen;
using System.Collections.Generic;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BookGenre : Fb2Element
{
    public override string Name => ElementNames.Genre;

    public override bool IsInline => false;

    public override FrozenSet<string> AllowedAttributes => new List<string> { AttributeNames.Match }.ToFrozenSet();
}

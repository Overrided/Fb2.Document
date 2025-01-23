using System.Collections.Frozen;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Title : Fb2Container
{
    public override string Name => ElementNames.Title;

    public override bool CanContainText => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Language];

    public override FrozenSet<string> AllowedElements => [ElementNames.Paragraph, ElementNames.EmptyLine];
}

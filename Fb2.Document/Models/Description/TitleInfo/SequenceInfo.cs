using System.Collections.Frozen;
using System.Text;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class SequenceInfo : Fb2Element
{
    public override string Name => ElementNames.Sequence;

    public override bool IsInline => false;

    public override FrozenSet<string> AllowedAttributes => [AttributeNames.Name, AttributeNames.Number, AttributeNames.Language];

    public sealed override Fb2Element AddContent(string newContent, string? separator = null) => this;

    public sealed override Fb2Element ClearContent() => this;

    public sealed override string ToString()
    {
        if (!HasAttributes)
            return string.Empty;

        var sb = new StringBuilder();

        if (TryGetAttribute(AttributeNames.Name, true, out var nameAttr))
            sb.Append(nameAttr!.Value);

        if (TryGetAttribute(AttributeNames.Number, true, out var numberAttr))
            sb.Append(sb.Length > 0 ? $" {numberAttr!.Value}" : numberAttr!.Value);

        return sb.ToString();
    }
}

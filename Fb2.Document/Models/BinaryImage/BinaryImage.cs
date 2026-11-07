using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BinaryImage : Fb2Element
{
    private static readonly ImmutableHashSet<string> allowedAttributes = [
        AttributeNames.ContentType,
        AttributeNames.Id
    ];

    public override string Name => ElementNames.BinaryImage;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => allowedAttributes;

    public sealed override void Load(
        [In] XNode node,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        // preserveWhitespace `true` to ommit regex usage in base.Load() - only one regex usage on line 28 here
        base.Load(node, parentNode, true, loadUnsafe, loadNamespaceMetadata);

        if (!string.IsNullOrEmpty(content))
            content = trimWhitespace.Replace(content, string.Empty);
    }
}

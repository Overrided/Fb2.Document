using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class BinaryImage : Fb2Element
{
    public override string Name => ElementNames.BinaryImage;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.ContentType, AttributeNames.Id];

    //public sealed override void Load(
    //    [In] XNode node,
    //    [In] Fb2Container? parentNode = null,
    //    bool preserveWhitespace = false,
    //    bool loadUnsafe = true,
    //    bool loadNamespaceMetadata = true)
    //{
    //    base.Load(node, parentNode, false, loadUnsafe, loadNamespaceMetadata);

    //    if (trimWhitespace.IsMatch(content))
    //        content = trimWhitespace.Replace(content, string.Empty);
    //}

    public sealed override async Task Load(
        [In] XmlReader reader,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        await base.Load(reader, parentNode, false, loadUnsafe, loadNamespaceMetadata);

        if (trimWhitespace.IsMatch(content))
            content = trimWhitespace.Replace(content, string.Empty);
    }
}

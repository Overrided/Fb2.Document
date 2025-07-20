using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class CustomInfo : Fb2Element
{
    public override string Name => ElementNames.CustomInfo;

    public override bool IsInline => false;

    public override ImmutableHashSet<string> AllowedAttributes => [AttributeNames.InfoType];

    /// <summary>
    /// Specific override to preserve original string content 
    /// with '\t', ' ', '\r\n' etc. formatting.
    /// </summary>
    //public sealed override void Load(
    //    [In] XNode node,
    //    [In] Fb2Container? parentNode = null,
    //    bool preserveWhitespace = false,
    //    bool loadUnsafe = true,
    //    bool loadNamespaceMetadata = true) =>
    //    base.Load(node, parentNode, true, loadUnsafe, loadNamespaceMetadata);


    public sealed override async Task Load(
        [In] XmlReader reader,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true) =>
        await base.Load(reader, parentNode, true, loadUnsafe, loadNamespaceMetadata);
}

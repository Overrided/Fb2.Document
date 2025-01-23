using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class Image : Fb2Element
{
    public override string Name => ElementNames.Image;

    public override FrozenSet<string> AllowedAttributes =>
    [
        AttributeNames.Id,
        AttributeNames.Alt,
        AttributeNames.Title,
        AttributeNames.XHref,
        AttributeNames.Type
    ];

    private static readonly HashSet<string> InlineParentNodes =
    [
        ElementNames.Paragraph,
        ElementNames.StanzaV,
        ElementNames.SubTitle,
        ElementNames.TableHeader,
        ElementNames.TableCell,
        ElementNames.TextAuthor
    ];

    private static readonly HashSet<string> NotInlineParentNodes =
    [
        ElementNames.BookBody,
        ElementNames.BookBodySection,
        ElementNames.Coverpage
    ];

    public sealed override string ToString()
    {
        var hasHref = TryGetAttribute(AttributeNames.XHref, true, out var result);
        if (!hasHref)
            return Name;

        return $"{Name} {result!.Value}";
    }

    public sealed override void Load(
        [In] XNode node,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        base.Load(node, parentNode, preserveWhitespace, loadUnsafe, loadNamespaceMetadata);
        IsInline = GetInline(Parent?.Name);
    }

    private static bool GetInline(string? parentNodeName)
    {
        if (string.IsNullOrEmpty(parentNodeName))
            return true;

        if (InlineParentNodes.Contains(parentNodeName))
            return true;
        else if (NotInlineParentNodes.Contains(parentNodeName))
            return false;

        return true;
    }
}

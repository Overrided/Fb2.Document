using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models;

public class EmptyLine : Fb2Element
{
    public override string Name => ElementNames.EmptyLine;

    public EmptyLine() => content = Environment.NewLine;

    public sealed override void Load(
        [In] XNode element,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        base.Load(element, parentNode, preserveWhitespace, loadUnsafe, loadNamespaceMetadata);
        content = Environment.NewLine; // double-check, just in case
    }

    public sealed override Fb2Element AddContent(string newContent, string? separator = null) => this;

    public sealed override Fb2Element ClearContent() => this;
}

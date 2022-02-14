﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Models
{
    public class Image : Fb2Element
    {
        public override string Name => ElementNames.Image;

        public override ImmutableHashSet<string> AllowedAttributes =>
            ImmutableHashSet.Create(
                AttributeNames.Id,
                AttributeNames.Alt,
                AttributeNames.Title,
                AttributeNames.XHref,
                AttributeNames.Type);

        private static readonly HashSet<string> InlineParentNodes = new HashSet<string>()
        {
            ElementNames.Paragraph,
            ElementNames.StanzaV,
            ElementNames.SubTitle,
            ElementNames.TableHeader,
            ElementNames.TableCell,
            ElementNames.TextAuthor
        };

        private static readonly HashSet<string> NotInlineParentNodes = new HashSet<string>()
        {
            ElementNames.BookBody,
            ElementNames.BookBodySection,
            ElementNames.Coverpage
        };

        public override string ToString()
        {
            var formattedAttributeString = TryGetAttribute(AttributeNames.XHref, out var result, true) ? $" {result!.Value}" : string.Empty;

            return $"{Name}{formattedAttributeString}";
        }

        public override void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            base.Load(node, parentNode, preserveWhitespace, loadUnsafe);
            IsInline = GetInline(Parent?.Name);
        }

        private bool GetInline(string? parentNodeName)
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
}

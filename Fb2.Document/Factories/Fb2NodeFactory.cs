﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Models;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Factories;

public static class Fb2NodeFactory
{
    private static readonly Dictionary<string, Type> KnownNodes = new()
    {
        { ElementNames.FictionBook, typeof(FictionBook) },
        { ElementNames.BinaryImage, typeof(BinaryImage) },
        { ElementNames.Description, typeof(BookDescription) },
        { ElementNames.TitleInfo, typeof(TitleInfo) },

        { ElementNames.SrcTitleInfo, typeof(SrcTitleInfo) },
        { ElementNames.DocumentInfo, typeof(DocumentInfo) },
        { ElementNames.PublishInfo, typeof(PublishInfo) },
        { ElementNames.CustomInfo, typeof(CustomInfo) },

        { ElementNames.Genre, typeof(BookGenre) },
        { ElementNames.Author, typeof(Author) },

        { ElementNames.FirstName, typeof(FirstName) },
        { ElementNames.MiddleName, typeof(MiddleName) },
        { ElementNames.LastName, typeof(LastName) },
        { ElementNames.NickName, typeof(Nickname) },
        { ElementNames.HomePage, typeof(HomePage) },
        { ElementNames.Email, typeof(Email) },
        { ElementNames.FictionId, typeof(FictionId) },
        { ElementNames.Lang, typeof(Lang) },
        { ElementNames.SrcLang, typeof(SrcLang) },

        { ElementNames.BookTitle, typeof(BookTitle) },
        { ElementNames.Annotation, typeof(Annotation) },
        { ElementNames.Paragraph, typeof(Paragraph) },
        { ElementNames.FictionText, typeof(TextItem) },
        { ElementNames.TextStyle, typeof(TextStyle) },
        { ElementNames.Emphasis, typeof(Emphasis) },
        { ElementNames.Strong, typeof(Strong) },
        { ElementNames.TextLink, typeof(TextLink) },
        { ElementNames.Image, typeof(Image) },
        { ElementNames.Superscript, typeof(Superscript) },
        { ElementNames.Subscript, typeof(Subscript) },
        { ElementNames.Code, typeof(Code) },
        { ElementNames.Date, typeof(Date) },
        { ElementNames.Strikethrough, typeof(Strikethrough) },

        { ElementNames.Table, typeof(Table) },
        { ElementNames.TableRow, typeof(TableRow) },
        { ElementNames.TableHeader, typeof(TableHeader) },
        { ElementNames.TableCell, typeof(TableCell) },

        { ElementNames.Poem, typeof(Poem) },
        { ElementNames.Title, typeof(Title) },
        { ElementNames.SubTitle, typeof(SubTitle) },
        { ElementNames.EmptyLine, typeof(EmptyLine) },
        { ElementNames.Epigraph, typeof(Epigraph) },
        { ElementNames.Quote, typeof(Quote) },
        { ElementNames.TextAuthor, typeof(TextAuthor) },
        { ElementNames.Stanza, typeof(Stanza) },
        { ElementNames.StanzaV, typeof(StanzaVerse) },

        { ElementNames.Keywords, typeof(Keywords) },
        { ElementNames.Coverpage, typeof(Coverpage) },
        { ElementNames.Translator, typeof(Translator) },
        { ElementNames.Sequence, typeof(SequenceInfo) },

        { ElementNames.ProgramUsed, typeof(ProgramUsed) },
        { ElementNames.SrcUrl, typeof(SrcUrl) },
        { ElementNames.SrcOcr, typeof(SrcOcr) },
        { ElementNames.Version, typeof(Models.Version) },
        { ElementNames.History, typeof(History) },
        { ElementNames.Publisher, typeof(Publisher) },

        { ElementNames.BookName, typeof(BookName) },
        { ElementNames.City, typeof(City) },
        { ElementNames.Year, typeof(Year) },
        { ElementNames.ISBN, typeof(ISBNInfo) },

        { ElementNames.BookBody, typeof(BookBody) },
        { ElementNames.BookBodySection, typeof(BodySection) },
        { ElementNames.Stylesheet, typeof(Stylesheet) }
    };

    /// <summary>
    /// Creates new <see cref="Fb2Node"/> by given name.
    /// </summary>
    /// <param name="nodeName">Name to create node by.</param>
    /// <returns>New instance of node created by given <paramref name="nodeName"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidNodeException"></exception>
    [UnconditionalSuppressMessage(
        "AssemblyLoadTrimming",
        "IL2072",
        Justification = "Everything referenced is in the same loaded assembly, so it's safe")]
    public static Fb2Node GetNodeByName([In] string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentNullException(nameof(nodeName));

        if (!IsKnownNodeName(nodeName))
            throw new InvalidNodeException(nodeName);

        var result = KnownNodes.First(kvp => kvp.Key.EqualsIgnoreCase(nodeName));
        var modelType = result.Value;

        var model = Activator.CreateInstance(modelType) as Fb2Node; // suppressed warning
        return model!;
    }

    /// <summary>
    /// Determines whether given <paramref name="node"/> is valid.
    /// </summary>
    /// <param name="node">Node to perform check on.</param>
    /// <returns><see langword="true"/> if node is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsKnownNode([In] Fb2Node node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        var hasKnownName = IsKnownNodeName(node.Name);
        var hasKnownType = KnownNodes.ContainsValue(node.GetType());

        return hasKnownName && hasKnownType;
    }

    /// <summary>
    /// Determines whether given <paramref name="nodeName"/> is valid.
    /// </summary>
    /// <param name="nodeName">Node name to perform check on.</param>
    /// <returns><see langword="true"/> if node name is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsKnownNodeName([In] string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentNullException(nameof(nodeName));

        var isKnownName = KnownNodes.Keys.Contains(nodeName, StringComparer.Create(CultureInfo.InvariantCulture, true));
        return isKnownName;
    }
}

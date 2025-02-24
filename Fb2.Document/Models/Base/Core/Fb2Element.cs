using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;

namespace Fb2.Document.Models.Base;

/// <summary>
/// Represents text Node of <see cref="Fb2Document"/>.
/// Any class derived from <see cref="Fb2Element"/> can contain text only.
/// </summary>
public abstract class Fb2Element : Fb2Node
{
    protected string? content;

    /// <summary>
    /// Content (value) of element. Available after Load(...) method call.
    /// </summary>
    public string Content => HasContent ? content! : string.Empty;

    /// <summary>
    /// <para>Indicates if content of an element should be written from a new line.</para>
    /// <para><see langword="true"/> if element is inline, otherwise - <see langword="false"/>.</para>
    /// <remarks>For most <see cref="Fb2Element"/> <see cref="Fb2Element.IsInline"/> is <see langword="true"/> by default, however, some models override this property.</remarks>
    /// </summary>
    public override bool IsInline { get; protected set; } = true;

    /// <summary>
    /// Indicates if element has any content.
    /// </summary>
    public override bool HasContent => !string.IsNullOrEmpty(content);

    /// <summary>
    /// Text node loading mechanism. Loads <see cref="Content"/> after formatting and removal of unwanted characters.
    /// </summary>
    /// <param name="node"><see cref="XNode"/> to load as <see cref="Fb2Element"/>.</param>
    /// <param name="parentNode">Parent node (<see cref="Fb2Container"/>). By default <see langword="null"/>.</param>
    /// <param name="preserveWhitespace">Indicates if whitespace characters (\t, \n, \r) should be preserved. By default <see langword="false"/>.</param>
    /// <param name="loadUnsafe">Indicates whether "Unsafe" children should be loaded. By default <see langword="true"/>. </param>
    /// <param name="loadNamespaceMetadata">Indicates whether XML Namespace Metadata should be preserved. By default <see langword="true"/>.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Fb2NodeLoadingException"></exception>
    /// <remarks>Original content of <see cref="XNode"/> is  <c>NOT preserved</c>  except for <see cref="Code" />.</remarks>
    public override void Load(
        [In] XNode node,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        base.Load(node, parentNode, preserveWhitespace, loadUnsafe, loadNamespaceMetadata);

        var rawContent = node.NodeType switch
        {
            XmlNodeType.Element => ((XElement)node).Value,
            XmlNodeType.Text => ((XText)node).Value,
            _ => throw new Fb2NodeLoadingException($"Unsupported nodeType: received {node.NodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
        };

        if (!preserveWhitespace && trimWhitespace.IsMatch(rawContent))
            content = trimWhitespace.Replace(rawContent, Whitespace);
        else
            content = rawContent;
    }

    public override async Task LoadFromReaderAsync([In] XmlReader reader)
    {
        await base.LoadFromReaderAsync(reader);

        var nodeType = reader.NodeType;

        if (reader.Name.ToLowerInvariant().Contains("impostor"))
        {
            var a = 1;
        }

        if (nodeType == XmlNodeType.None && reader.ReadState == ReadState.EndOfFile)
        {
            return;
        }

        var rawContent = nodeType switch
        {
            XmlNodeType.Element => reader.Value,
            XmlNodeType.Text => reader.Value,
            _ => throw new Fb2NodeLoadingException($"Unsupported nodeType: received {nodeType}, expected {XmlNodeType.Element} or {XmlNodeType.Text}"),
        };

        Console.WriteLine($"{new string(' ', reader.Depth)}{rawContent}");

        content = rawContent;
    }

    /// <summary>
    /// Appends new plain text to <see cref="Content"/> using provider function.
    /// </summary>
    /// <param name="contentProvider">Content provider function.</param>
    /// <param name="separator">Separator to split text from rest of the content.</param>
    /// <returns>Current element.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Element AddContent(Func<string> contentProvider, string? separator = null)
    {
        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        var content = contentProvider();

        return AddContent(content, separator);
    }

    /// <summary>
    /// Appends new plain text to <see cref="Content"/> using asynchronous content provider function.
    /// </summary>
    /// <param name="contentProvider">Asynchronous content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns>Current element.</returns>
    /// <remarks>
    /// If <paramref name="separator"/> contains <see cref="Environment.NewLine"/> - it will be replaced with " " (whitespace).
    /// <para>To insert new line use <see cref="EmptyLine"/> Fb2Element instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<Fb2Element> AddContentAsync(Func<Task<string>> contentProvider, string? separator = null)
    {
        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        var newContent = await contentProvider();

        return AddContent(newContent, separator);
    }

    /// <summary>
    /// Appends new plain text to <see cref="Content"/>.
    /// </summary>
    /// <param name="newContent">Plain text to append.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns>Current element.</returns>
    /// <remarks>
    /// If <paramref name="separator"/> contains <see cref="Environment.NewLine"/> - it will be replaced with " " (whitespace).
    /// <para>To insert new line use <see cref="EmptyLine"/> Fb2Element instead.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual Fb2Element AddContent(string newContent, string? separator = null)
    {
        if (string.IsNullOrEmpty(newContent))
            throw new ArgumentNullException(nameof(newContent));

        var normalizedSeparator = string.IsNullOrEmpty(separator) ?
            string.Empty :
            SecurityElement.Escape(separator.Replace(Environment.NewLine, Whitespace));

        var normalizedNewContent = newContent.Replace(Environment.NewLine, Whitespace);
        normalizedNewContent = SecurityElement.Escape(normalizedNewContent)!;

        content = string.Join(normalizedSeparator, content, normalizedNewContent);

        return this;
    }

    /// <summary>
    /// Clears <see cref="Content"/>.
    /// </summary>
    /// <returns>Current element.</returns>
    public virtual Fb2Element ClearContent()
    {
        if (HasContent)
            content = null;

        return this;
    }

    /// <summary>
    /// Converts <see cref="Fb2Element"/> to <see cref="XElement"/> with regards to all attributes.
    /// </summary>
    /// <param name="serializeUnsafeNodes">Indicates is "Unsafe" content should be serialized. By default <see langword="true"/>. </param>
    /// <returns><see cref="XElement"/> reflected from given <see cref="Fb2Element"/>.</returns>
    /// <remarks>
    /// Only formatted content is serialized.
    /// <para>Original symbols from string value of XNode passed to Load method can be replaced and/or removed during <see cref="Fb2Element.Load(XNode, bool, bool)"/>.</para>
    /// </remarks>
    public override XElement ToXml(bool serializeUnsafeNodes = true)
    {
        var element = base.ToXml(serializeUnsafeNodes);
        if (HasContent)
            element.Value = content!;

        return element;
    }

    public override string ToString() => Content;

    public override bool Equals(object? other)
    {
        if (!base.Equals(other))
            return false;

        if (other is not Fb2Element otherElement)
            return false;

        var otherContent = otherElement.content;

        var bothContensAreNull = content is null && otherContent is null;
        if (bothContensAreNull)
            return true;

        var bothContensAreEmpty = string.IsNullOrEmpty(content) && string.IsNullOrEmpty(otherContent);
        if (bothContensAreEmpty)
            return true;

        var bothContentsAreNotEmpty = !string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(otherContent);
        if (!bothContentsAreNotEmpty)
            return false;

        var result = content!.Equals(otherContent, StringComparison.InvariantCulture);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), content);

    /// <summary>
    /// Clones given <see cref="Fb2Element"/> creating new instance of same node, attaching attributes etc.
    /// </summary>
    /// <returns>New instance of given <see cref="Fb2Element"/>.</returns>
    public sealed override object Clone()
    {
        var clone = base.Clone() as Fb2Element;

        if (HasContent)
            clone!.content = new(content);

        return clone!;
    }
}

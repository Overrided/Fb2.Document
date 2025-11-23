using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;

namespace Fb2.Document.Models.Base;

/// <summary>
/// Represents container node, which can contain both text and other Fb2Node(s).
/// </summary>
public abstract class Fb2Container : Fb2Node
{
    private List<Fb2Node>? content;

    /// <summary>
    /// Actual value is available after <see cref="Load(XNode, Fb2Container?, bool, bool, bool)"/> method call.
    /// </summary>
    public ImmutableList<Fb2Node> Content => HasContent ? [.. content!] : [];

    /// <summary>
    /// Indicates if instance of type <see cref="Fb2Container"/> can contain text.
    /// <see langword="true"/> if element can contain text, otherwise - <see langword="false"/>.
    /// </summary>
    public virtual bool CanContainText { get; private set; }

    /// <summary>
    /// List of allowed descendant node's names.
    /// </summary>
    public abstract ImmutableHashSet<string> AllowedElements { get; }

    /// <summary>
    /// <para>Indicates if content of an element should be written from a new line.</para>
    /// <para><see langword="true"/> if element is inline, otherwise - <see langword="false"/>.</para>
    /// </summary>
    public override bool IsInline { get; protected set; } = false;

    /// <summary>
    /// Indicates if element has any content.
    /// </summary>
    public override bool HasContent => content is { Count: > 0 };

    /// <summary>
    /// Container Node loading mechanism. Loads <see cref="Content"/> and sequentially calls <see cref="Fb2Node.Load(XNode,Fb2Container?,bool, bool, bool)"/> on all child nodes.
    /// </summary>
    /// <param name="node"><see cref="XNode"/> to load as <see cref="Fb2Container"/>.</param>
    /// <param name="parentNode">Parent node (<see cref="Fb2Container"/>). By default <see langword="null"/>.</param>
    /// <param name="preserveWhitespace">Indicates if whitespace characters (\t, \n, \r) should be preserved. By default <see langword="false"/>.</param>
    /// <param name="loadUnsafe">Indicates whether "Unsafe" children should be loaded. By default <see langword="true"/>. </param>
    /// <param name="loadNamespaceMetadata">Indicates wheter XML Namespace Metadata should be preserved. By default <see langword="true"/>.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Fb2NodeLoadingException"></exception>
    public override void Load(
        [In] XNode node,
        [In] Fb2Container? parentNode = null,
        bool preserveWhitespace = false,
        bool loadUnsafe = true,
        bool loadNamespaceMetadata = true)
    {
        base.Load(node, parentNode, preserveWhitespace, loadUnsafe, loadNamespaceMetadata);

        if (node is not XElement element || element.IsEmpty)
            return;

        var nodes = element
            .Nodes()
            .Where(n =>
            {
                var nodeType = n.NodeType;

                if (nodeType == XmlNodeType.Text)
                    return loadUnsafe || CanContainText;

                var isElement = nodeType == XmlNodeType.Element;
                if (!isElement)
                    return false;

                var childNode = (XElement)n;
                var nodeLocalName = childNode.Name.LocalName.ToLowerInvariant();

                var isValid =
                    !nodeLocalName.EqualsIgnoreCase(ElementNames.FictionText) &&
                    Fb2NodeFactory.IsKnownNodeName(nodeLocalName);

                if (!isValid)
                    return false;

                return loadUnsafe || AllowedElements.Contains(nodeLocalName);
            })
            .ToArray();

        var nodesCount = nodes.Length;
        if (nodesCount == 0)
            return;

        EnsureContentInitialized(nodesCount);

        for (int i = 0; i < nodesCount; i++)
        {
            var validNode = nodes[i];

            string localName = validNode.NodeType == XmlNodeType.Element ?
                ((XElement)validNode).Name.LocalName.ToLowerInvariant() :
                ElementNames.FictionText;

            var isUnsafe = validNode.NodeType == XmlNodeType.Text ?
                !CanContainText :
                !AllowedElements.Contains(localName);

            var elem = Fb2NodeFactory.GetNodeByName(localName);
            elem.Load(validNode, this, preserveWhitespace, loadUnsafe, loadNamespaceMetadata);
            elem.IsUnsafe = isUnsafe;

            content!.Add(elem);
        }
    }

    public override string ToString()
    {
        if (!HasContent)
            return string.Empty;

        var builder = new StringBuilder();

        for (int i = 0; i < content!.Count; i++)
        {
            var child = content[i];
            var childContent = child.ToString();

            if (!string.IsNullOrEmpty(childContent))
                builder.Append(child.IsInline ? childContent : $"{Environment.NewLine}{childContent}");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts <see cref="Fb2Container"/> to <see cref="XElement"/> with regards to all attributes, 
    /// by calling <see cref="ToXml(bool)"/> on every descendant node in <see cref="Content"/> recursively.
    /// </summary>
    /// <param name="serializeUnsafeNodes">Indicates is "Unsafe" content should be serialized. By default <see langword="true"/>. </param>
    /// <returns><see cref="XElement"/> reflected from given <see cref="Fb2Container"/>.</returns>
    public override XElement ToXml(bool serializeUnsafeNodes = true)
    {
        var element = base.ToXml(serializeUnsafeNodes);

        if (HasContent)
        {
            var childrenToSerialize = serializeUnsafeNodes ?
                content :
                content!.Where(x => !x.IsUnsafe);

            if (childrenToSerialize == null || !childrenToSerialize.Any())
                return element;

            var serializedChildren = childrenToSerialize.Select(n => ToXmlInternal(n, serializeUnsafeNodes));
            element.Add(serializedChildren);
        }

        return element;
    }

    public override bool Equals(object? other)
    {
        if (!base.Equals(other))
            return false;

        if (other is not Fb2Container otherContainer)
            return false;

        var actualContent = content;
        var otherContent = otherContainer.content;
        var areBothContentsNull = actualContent is null && otherContent is null;

        var sameContent = areBothContentsNull ||
                          actualContent?.Count == otherContent?.Count &&
                          (actualContent?.SequenceEqual(otherContent ?? []) ?? false);

        var result = sameContent &&
            CanContainText == otherContainer.CanContainText &&
            AllowedElements.SequenceEqual(otherContainer.AllowedElements);

        return result;
    }

    public sealed override int GetHashCode() => HashCode.Combine(base.GetHashCode(), CanContainText, content, AllowedElements);

    /// <summary>
    /// Clones given <see cref="Fb2Container"/> creating new instance of same node, attaching attributes etc.
    /// </summary>
    /// <returns>New instance of given <see cref="Fb2Container"/>.</returns>
    public sealed override object Clone()
    {
        var clone = base.Clone() as Fb2Container;
        clone!.CanContainText = CanContainText;

        if (HasContent)
        {
            var clonedContent = content!.Select(c => (Fb2Node)c.Clone()).ToArray();
            clone.AddContent(clonedContent);
        }

        return clone;
    }

    #region Content editing

    /// <summary>
    /// <para> 
    /// This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.
    /// </para>
    /// Adds node to <see cref="Content"/> using asynchronous provider function.
    /// </summary>
    /// <param name="nodeProvider">Asynchronous node provider function.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="attributeProvider"/> is null.</exception>
    [Obsolete("This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.")]
    public async Task<Fb2Container> AddContentAsync(Func<Task<Fb2Node>> nodeProvider)
    {
        ArgumentNullException.ThrowIfNull(nodeProvider, nameof(nodeProvider));

        var newNode = await nodeProvider();
        return AddContent(newNode);
    }

    /// <summary>
    /// Adds node to <see cref="Content"/> using asynchronous provider function.
    /// </summary>
    /// <param name="nodeProvider">Asynchronous node provider function.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current container.</returns>
    /// <remarks>
    /// <see cref="OperationCanceledException"/> is not handled if cancellation is requested.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="attributeProvider"/> is null.</exception>
    /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
    public async Task<Fb2Container> AddContentAsync(
        Func<CancellationToken, Task<Fb2Node>> nodeProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nodeProvider, nameof(nodeProvider));

        cancellationToken.ThrowIfCancellationRequested();

        var newNode = await nodeProvider(cancellationToken);
        return AddContent(newNode);
    }

    /// <summary>
    /// Adds node to <see cref="Content"/> using provider function 
    /// </summary>
    /// <param name="nodeProvider">Provider function for a child node.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container AddContent(Func<Fb2Node> nodeProvider)
    {
        ArgumentNullException.ThrowIfNull(nodeProvider, nameof(nodeProvider));

        var node = nodeProvider();
        return AddContent(node);
    }

    /// <summary>
    /// Adds `params Fb2Node[]` nodes to <see cref="Content"/>.
    /// </summary>
    /// <param name="nodes">Nodes to add to <see cref="Content"/>.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container AddContent(params List<Fb2Node> nodes)
    {
        if (nodes is not { Count: > 0 } || nodes.All(n => n == null))
            throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

        EnsureContentInitialized(nodes.Count);

        foreach (var node in nodes)
            AddContent(node);

        return this;
    }

    /// <summary>
    /// Appends plain text node to <see cref="Content"/>.
    /// </summary>
    /// <param name="newContent">Plain text content to add.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="UnexpectedNodeException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container AddTextContent(string newContent, string? separator = null)
    {
        if (!CanContainText)
            throw new UnexpectedNodeException(Name, ElementNames.FictionText);

        if (string.IsNullOrEmpty(newContent))
            throw new ArgumentNullException(nameof(newContent));

        return TryMergeTextContent(newContent, separator);
    }

    /// <summary>
    /// Appends plain text node to <see cref="Content"/> using content provider function.
    /// </summary>
    /// <param name="contentProvider">Content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UnexpectedNodeException"></exception>
    public Fb2Container AddTextContent(Func<string> contentProvider, string? separator = null)
    {
        if (!CanContainText)
            throw new UnexpectedNodeException(Name, ElementNames.FictionText);

        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        var newContent = contentProvider();
        return AddTextContent(newContent, separator);
    }

    /// <summary>
    /// <para> 
    /// This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.
    /// </para>
    /// Appends plain text node to <see cref="Content"/> using asynchronous content provider function.
    /// </summary>
    /// <param name="contentProvider">Asynchronous content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UnexpectedNodeException"></exception>
    [Obsolete("This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.")]
    public async Task<Fb2Container> AddTextContentAsync(
        Func<Task<string>> contentProvider,
        string? separator = null)
    {
        if (!CanContainText)
            throw new UnexpectedNodeException(Name, ElementNames.FictionText);

        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        var newContent = await contentProvider();
        return AddTextContent(newContent, separator);
    }

    /// <summary>
    /// Appends plain text node to <see cref="Content"/> using asynchronous content provider function.
    /// </summary>
    /// <param name="contentProvider">Asynchronous content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UnexpectedNodeException"></exception>
    public async Task<Fb2Container> AddTextContentAsync(
        Func<CancellationToken, Task<string>> contentProvider,
        string? separator = null,
        CancellationToken cancellationToken = default)
    {
        if (!CanContainText)
            throw new UnexpectedNodeException(Name, ElementNames.FictionText);

        ArgumentNullException.ThrowIfNull(contentProvider, nameof(contentProvider));

        cancellationToken.ThrowIfCancellationRequested();

        var newContent = await contentProvider(cancellationToken);
        return AddTextContent(newContent, separator);
    }

    /// <summary>
    /// Adds set of Fb2Nodes to <see cref="Content"/>.
    /// </summary>
    /// <param name="nodes">Set of nodes to add to <see cref="Content"/>.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container AddContent(IEnumerable<Fb2Node> nodes)
    {
        if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
            throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

        AddContent([.. nodes]);

        return this;
    }

    /// <summary>
    /// Adds new node to <see cref="Content"/> using node's <see cref="Fb2Node.Name"/>.
    /// </summary>
    /// <param name="nodeName">Name to instantiate node by.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container AddContent(string nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
            throw new ArgumentNullException(nameof(nodeName));

        var node = Fb2NodeFactory.GetNodeByName(nodeName);
        return AddContent(node);
    }

    /// <summary>
    /// Adds given <paramref name="node"/> to <see cref="Content"/>.
    /// </summary>
    /// <param name="node">Child node to add to Content.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidNodeException"></exception>
    /// <exception cref="UnexpectedNodeException"></exception>
    public Fb2Container AddContent(Fb2Node node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        if (!Fb2NodeFactory.IsKnownNode(node))
            throw new InvalidNodeException(node);

        var nodeName = node.Name;
        var isTextNode = node is TextItem;

        if (isTextNode && !CanContainText)
            throw new UnexpectedNodeException(Name, nodeName);

        if (!AllowedElements.Contains(nodeName) && !isTextNode)
            throw new UnexpectedNodeException(Name, nodeName);

        if (isTextNode)
            return TryMergeTextContent((node as TextItem)!.Content);

        node.Parent = this;
        if (node.NodeMetadata == null && NodeMetadata != null) // copy parent default namespace to prevent serialization issues
            node.NodeMetadata = new Fb2NodeMetadata(NodeMetadata.DefaultNamespace);

        EnsureContentInitialized(1);

        content!.Add(node);
        return this;
    }

    /// <summary>
    /// Removes set of nodes from <see cref="Content"/>
    /// </summary>
    /// <param name="nodes">Set of nodes to remove.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container RemoveContent(IEnumerable<Fb2Node> nodes)
    {
        if (nodes == null || !nodes.Any())
            throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array.");

        foreach (var node in nodes)
            RemoveContent(node);

        return this;
    }

    /// <summary>
    /// Removes matching nodes from <see cref="Content"/> by given predicate.
    /// </summary>
    /// <param name="nodePredicate">Predicate to match node against.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container RemoveContent([In] Func<Fb2Node, bool> nodePredicate)
    {
        ArgumentNullException.ThrowIfNull(nodePredicate, nameof(nodePredicate));

        if (HasContent)
        {
            var nodesToRemove = GetChildren(nodePredicate).ToArray();
            foreach (var node in nodesToRemove)
                RemoveContent(node);
        }

        return this;
    }

    /// <summary>
    /// Removes given node from <see cref="Content"/>.
    /// </summary>
    /// <param name="node">Node to remove.</param>
    /// <returns>Current container.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Container RemoveContent(Fb2Node node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        if (HasContent && content!.Contains(node))
        {
            content.Remove(node);
            node.Parent = null;
        }

        return this;
    }

    /// <summary>
    /// Clears all <see cref="Content"/> of given container.
    /// </summary>
    /// <returns>Current container.</returns>
    public Fb2Container ClearContent()
    {
        if (HasContent)
            for (int i = content!.Count - 1; i >= 0; i--)
                RemoveContent(content[i]);

        return this;
    }

    #endregion

    #region Content querying

    /// <summary>
    /// Gets children of element by given name. Case insensitive.
    /// </summary>
    /// <param name="name">Name to select child elements by.</param>
    /// <returns>List of found child elements, if any.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidNodeException"></exception>
    public IEnumerable<Fb2Node> GetChildren(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (!Fb2NodeFactory.IsKnownNodeName(name))
            throw new InvalidNodeException(name);

        if (HasContent)
            return content!.Where(elem => elem.Name.EqualsIgnoreCase(name));

        return [];
    }

    /// <summary>
    /// Gets children of element that match given predicate.
    /// </summary>
    /// <param name="predicate">Predicate to match child node against.</param>
    /// <returns>List of found child elements, if any.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IEnumerable<Fb2Node> GetChildren(Func<Fb2Node, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (HasContent)
            return content!.Where(c => predicate(c));

        return [];
    }

    /// <summary>
    /// Gets first matching child of element by given name.
    /// </summary>
    /// <param name="name">Name to select child element by. Optional.</param>
    /// <returns>First matched child node or <see langword="null"/>.</returns>
    /// <exception cref="InvalidNodeException"></exception>
    public Fb2Node? GetFirstChild(string? name)
    {
        if (!string.IsNullOrEmpty(name) && !Fb2NodeFactory.IsKnownNodeName(name))
            throw new InvalidNodeException(name);

        if (HasContent)
            return string.IsNullOrWhiteSpace(name) ?
                content!.FirstOrDefault() :
                content!.FirstOrDefault(elem => elem.Name.EqualsIgnoreCase(name));

        return null;
    }

    /// <summary>
    /// Gets first child of element that matches given predicate.
    /// </summary>
    /// <param name="predicate">Predicate to match child node against.</param>
    /// <returns>First matched child node or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Node? GetFirstChild(Func<Fb2Node, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (HasContent)
            return content!.FirstOrDefault(predicate);

        return null;
    }

    /// <summary>
    /// Recursively gets all descendants of element by given name.
    /// </summary>
    /// <param name="name">Name to select descendants by.</param>
    /// <returns>List of found descendants, if any.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidNodeException"></exception>
    public IEnumerable<Fb2Node> GetDescendants(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (!Fb2NodeFactory.IsKnownNodeName(name))
            throw new InvalidNodeException(name);

        var result = new List<Fb2Node>();

        if (!HasContent)
            return result;

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (element.Name.EqualsIgnoreCase(name))
                result.Add(element);

            if (element is Fb2Container containerElement)
            {
                var desc = containerElement.GetDescendants(name);

                if (desc != null && desc.Any())
                    result.AddRange(desc);
            }
        }

        return result;
    }

    /// <summary>
    /// Recursively gets all descendants of element that match given predicate.
    /// </summary>
    /// <param name="predicate">Predicate to match descendant node against.</param>
    /// <returns>List of found descendants, if any.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IEnumerable<Fb2Node> GetDescendants(Func<Fb2Node, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        var result = new List<Fb2Node>();

        if (!HasContent)
            return result;

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (predicate(element))
                result.Add(element);

            if (element is Fb2Container containerElement)
            {
                var desc = containerElement.GetDescendants(predicate);

                if (desc != null && desc.Any())
                    result.AddRange(desc);
            }
        }

        return result;
    }

    /// <summary>
    /// Recursively looks for first matching descendant of element by given name.
    /// </summary>
    /// <param name="name">Name to select descendant by.</param>
    /// <returns>First matching descendant node or <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidNodeException"></exception>
    public Fb2Node? GetFirstDescendant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (!Fb2NodeFactory.IsKnownNodeName(name))
            throw new InvalidNodeException(name);

        if (!HasContent)
            return null;

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (element.Name.EqualsIgnoreCase(name))
                return element;

            if (element is Fb2Container containerElement)
            {
                var firstDesc = containerElement.GetFirstDescendant(name);

                if (firstDesc != null)
                    return firstDesc;
            }
        }

        return null;
    }

    /// <summary>
    /// Recursively looks for first descendant of element that matches given predicate.
    /// </summary>
    /// <param name="predicate">Predicate to match descendant node against.</param>
    /// <returns>First matching descendant node.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Fb2Node? GetFirstDescendant(Func<Fb2Node, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));

        if (!HasContent)
            return null;

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (predicate(element))
                return element;

            if (element is Fb2Container containerElement)
            {
                var firstDesc = containerElement.GetFirstDescendant(predicate);

                if (firstDesc != null)
                    return firstDesc;
            }
        }

        return null;
    }

    /// <summary>
    /// Recursively looks for first matching descendant of element by given name.
    /// </summary>
    /// <param name="name">Name to select descendant by.</param>
    /// <param name="node">Out param, actual result of a search.</param>
    /// <returns>Boolean value indicating if any matching node was actually found.</returns>
    public bool TryGetFirstDescendant(string name, out Fb2Node? node)
    {
        var firstDescendant = GetFirstDescendant(name);
        node = firstDescendant;
        return firstDescendant != null;
    }

    /// <summary>
    /// Recursively looks for first matching descendant of element by given predicate.
    /// </summary>
    /// <param name="predicate">Predicate to match nodes against.</param>
    /// <param name="node">Out param, actual result of a search.</param>
    /// <returns>Boolean value indicating if any matching node was actually found.</returns>
    public bool TryGetFirstDescendant(Func<Fb2Node, bool> predicate, out Fb2Node? node)
    {
        var firstDescendant = GetFirstDescendant(predicate);
        node = firstDescendant;
        return firstDescendant != null;
    }

    /// <summary>
    /// Gets children of element by given node type (Fb2Node-based nodes). 
    /// </summary>
    /// <typeparam name="T">Node type to select elements by.</typeparam>
    /// <returns>List of found child elements, if any.</returns>
    public IEnumerable<T> GetChildren<T>() where T : Fb2Node
    {
        if (!HasContent)
            return [];

        var predicate = GetPredicate<T>();
        var result = content!.Where(predicate);

        return result.Any() ? result.Cast<T>() : [];
    }

    /// <summary>
    /// Gets first matching child of element by given node type (Fb2Node-based nodes). 
    /// </summary>
    /// <param name="name">Node type to select child element by.</param>
    /// <returns>First matched child node</returns>
    public T? GetFirstChild<T>() where T : Fb2Node
    {
        if (!HasContent)
            return null;

        var predicate = GetPredicate<T>();
        var result = content!.FirstOrDefault(predicate);

        if (result == null)
            return null;

        return result as T;
    }

    /// <summary>
    /// Recursively gets all descendants of element by given <typeparamref name="T"/> where T : Fb2Node
    /// </summary>
    /// <typeparam name="T">Node type to select descendants by.</typeparam>
    /// <returns>List of found descendants, if any.</returns>
    public IEnumerable<T> GetDescendants<T>() where T : Fb2Node => GetDescendantsInternal<T>();

    /// <summary>
    /// Recursively looks for first matching descendant of element by given node type (Fb2Node-based nodes).
    /// </summary>
    /// <typeparam name="T">Node type to select descendant by.</typeparam>
    /// <returns>First matched descendant node.</returns>
    public T? GetFirstDescendant<T>() where T : Fb2Node => GetFirstDescendantInternal<T>();

    /// <summary>
    /// Recursively looks for first matching descendant of element by given node type (Fb2Node-based nodes).
    /// </summary>
    /// <typeparam name="T">Node type to select descendant by.</typeparam>
    /// <param name="node">Out param, actual result of a search.</param>
    /// <returns>Boolean value indicating if any node was actually found. Node itself is returned as out parameter.</returns>
    public bool TryGetFirstDescendant<T>(out T? node) where T : Fb2Node
    {
        var result = GetFirstDescendantInternal<T>();
        node = result;
        return result != null;
    }

    #endregion

    #region Private Methods

    private IEnumerable<T> GetDescendantsInternal<T>(Func<Fb2Node, bool>? predicate = null)
        where T : Fb2Node
    {
        if (!HasContent)
            return [];

        var result = new List<Fb2Node>();

        predicate ??= GetPredicate<T>();

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (predicate(element))
                result.Add(element);

            if (element is Fb2Container containerElement)
            {
                var desc = containerElement.GetDescendantsInternal<T>(predicate);
                if (desc.Any())
                    result.AddRange(desc);
            }
        }

        return result.Cast<T>();
    }

    private T? GetFirstDescendantInternal<T>(Func<Fb2Node, bool>? predicate = null)
        where T : Fb2Node
    {
        if (!HasContent)
            return null;

        predicate ??= GetPredicate<T>();

        for (int i = 0; i < content!.Count; i++)
        {
            var element = content[i];

            if (predicate(element))
                return (T)element;

            if (element is Fb2Container containerElement)
            {
                var desc = containerElement.GetFirstDescendantInternal<T>(predicate);
                if (desc != null)
                    return desc;
            }
        }

        return null;
    }

    private Fb2Container TryMergeTextContent(string newContent, string? separator = null)
    {
        EnsureContentInitialized(1);

        var lastChildNode = HasContent ? content!.LastOrDefault() : null;

        // empty or last item is not text, so cant append actual content nowhere
        if (lastChildNode == null ||
            lastChildNode is not TextItem lastTextItem)
        {
            var textNode = new TextItem { Parent = this }.AddContent(newContent, separator);
            content!.Add(textNode);
        }
        else
            lastTextItem.AddContent(newContent, separator);

        return this;
    }

    private void EnsureContentInitialized(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Should not be less then zero!");

        if (HasContent)
            return;

        content = new List<Fb2Node>(capacity);
    }

    private static XNode ToXmlInternal(Fb2Node element, bool serializeUnsafeElements) =>
    element is TextItem textItem ? (XNode)new XText(textItem.Content) : element.ToXml(serializeUnsafeElements);

    private static Func<Fb2Node, bool> GetPredicate<T>() where T : Fb2Node
    {
        var targetType = typeof(T);

        if (targetType.IsAbstract)
            return element => element.GetType().IsSubclassOf(targetType);

        return element => element.GetType().Equals(targetType);
    }

    #endregion
}

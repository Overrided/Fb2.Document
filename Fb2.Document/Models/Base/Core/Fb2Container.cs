﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;
using Fb2.Document.Resolver;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Represents container node, which can contain both text and other Fb2Node(s)
    /// </summary>
    public abstract class Fb2Container : Fb2Node
    {
        private List<Fb2Node> content = new List<Fb2Node>();

        /// <summary>
        /// Actual value is available after `Load()` method call.
        /// </summary>
        public ImmutableList<Fb2Node> Content => content.ToImmutableList();

        /// <summary>
        /// Indicates if instance of type Fb2Container can contain text.
        /// `true` if element can contain text, otherwise - `false`.
        /// </summary>
        public virtual bool CanContainText { get; private set; }

        /// <summary>
        /// List of allowed descendant node's names.
        /// </summary>
        public abstract ImmutableHashSet<string> AllowedElements { get; }

        /// <summary>
        /// Indicates whether element should start with a new line or be inline.
        /// </summary>
        public override bool IsInline { get; protected set; } = false;

        /// <summary>
        /// Indicates if element has any content.
        /// </summary>
        public override bool IsEmpty => content.Count == 0;

        /// <summary>
        /// Container node loading mechanism. Loads attributes and sequentially calls `Load` on all child nodes.
        /// </summary>
        /// <param name="node">Node to load as Fb2Container</param>
        /// <param name="preserveWhitespace">Indicates if whitespace chars (\t, \n, \r) should be preserved. By default `false`.</param>
        /// <param name="loadUnsafe">Indicates whether "Unsafe" children should be loaded. By default `true`. </param>
        public override void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            base.Load(node, parentNode, preserveWhitespace, loadUnsafe);

            var element = node as XElement;

            if (element == null || element.IsEmpty)
                return;

            var nodes = element.Nodes()
                .Where(n =>
                    {
                        if (n.NodeType == XmlNodeType.Text)
                            return true;

                        var isElement = n.NodeType == XmlNodeType.Element;
                        if (!isElement)
                            return false;

                        var elementNode = n as XElement;
                        var nodeLocalName = elementNode.Name.LocalName;
                        return !nodeLocalName.EqualsInvariant(ElementNames.FictionText) &&
                               Fb2NodeFactory.IsKnownNodeName(nodeLocalName);
                    })
                .ToList();

            var nodesCount = nodes.Count;

            if (nodesCount == 0)
                return;

            for (int i = 0; i < nodesCount; i++)
            {
                var validNode = nodes[i];

                string localName = validNode.NodeType == XmlNodeType.Element ?
                    ((XElement)validNode).Name.LocalName.ToLowerInvariant() :
                    ElementNames.FictionText;

                var isUnsafe = validNode.NodeType == XmlNodeType.Text ?
                    !CanContainText :
                    !AllowedElements.Contains(localName);

                if (isUnsafe && !loadUnsafe)
                    continue;

                var elem = Fb2NodeFactory.GetNodeByName(localName);
                elem.Load(validNode, this, preserveWhitespace, loadUnsafe);
                elem.IsUnsafe = isUnsafe;

                content.Add(elem);
            }
        }

        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            var builder = new StringBuilder();

            for (int i = 0; i < content.Count; i++)
            {
                var child = content[i];
                builder.Append(child.IsInline ? $"{child}" : $"{Environment.NewLine}{child}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Converts Fb2Container to XElement with regards to all attributes, 
        /// by calling `ToXml()` on every node in `Content`.
        /// </summary>
        /// <returns>XElement reflected from given Fb2Element</returns>
        public override XElement ToXml()
        {
            var element = base.ToXml();

            if (IsEmpty)
                return element;

            var children = content.Select(ToXmlInternal);
            element.Add(children);

            return element;
        }

        public override bool Equals(object? other)
        {
            if (other == null)
                return false;

            if (!(other is Fb2Container otherContainer))
                return false;

            if (!base.Equals(otherContainer))
                return false;

            var actualContent = content;
            var otherContent = otherContainer.content;
            var sameContent = actualContent.Count == otherContent.Count &&
                              actualContent.SequenceEqual(otherContent);

            var result = sameContent &&
                CanContainText == otherContainer.CanContainText &&
                AllowedElements.SequenceEqual(otherContainer.AllowedElements);

            return result;
        }

        public sealed override int GetHashCode() => HashCode.Combine(base.GetHashCode(), CanContainText, content, AllowedElements);

        public sealed override object Clone()
        {
            var container = base.Clone() as Fb2Container;
            container!.content = new List<Fb2Node>(content.Select(c => (Fb2Node)c.Clone()));
            container.CanContainText = CanContainText;

            return container;
        }

        #region Content editing

        /// <summary>
        /// Adds node to <see cref="Content"/> using asynchronous provider function.
        /// </summary>
        /// <param name="nodeProvider">Asynchronous node provider function.</param>
        /// <returns>Current container.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Fb2Container> AddContentAsync(Func<Task<Fb2Node>> nodeProvider)
        {
            if (nodeProvider == null)
                throw new ArgumentNullException(nameof(nodeProvider));

            var newNode = await nodeProvider().ConfigureAwait(false);
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
            if (nodeProvider == null)
                throw new ArgumentNullException(nameof(nodeProvider));

            var node = nodeProvider();
            return AddContent(node);
        }

        /// <summary>
        /// Adds `params Fb2Node[]` nodes to <see cref="Content"/>.
        /// </summary>
        /// <param name="nodes">Nodes to add to <see cref="Content"/>.</param>
        /// <returns>Current container.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Container AddContent(params Fb2Node[] nodes)
        {
            if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
                throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

            foreach (var node in nodes)
                AddContent(node);

            return this;
        }

        // TODO : add AddTextContentAsync method
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
                throw new ArgumentNullException(nameof(newContent), $"{nameof(newContent)} is null or empty string.");

            return TryMergeTextContent(newContent, separator);
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

            foreach (var node in nodes)
                AddContent(node);

            return this;
        }

        /// <summary>
        /// Adds new node to <see cref="Content"/> using node's `Name`.
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
        /// Adds given node to <see cref="Content"/>.
        /// </summary>
        /// <param name="node">Child node to add to Content.</param>
        /// <returns>Current container.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UnknownNodeException"></exception>
        /// <exception cref="UnexpectedNodeException"></exception>
        public Fb2Container AddContent(Fb2Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!Fb2NodeFactory.IsKnownNode(node))
                throw new UnknownNodeException(node);

            var nodeName = node.Name;
            var isTextNode = node is TextItem;

            if (isTextNode && !CanContainText)
                throw new UnexpectedNodeException(Name, nodeName);

            if (!AllowedElements.Contains(nodeName) && !isTextNode)
                throw new UnexpectedNodeException(Name, nodeName);

            if (isTextNode)
                return TryMergeTextContent((node as TextItem)!.Content);

            node.Parent = this;
            if (node.NodeMetadata == null) // copy parent default namespace to prevent serialization issues
                node.NodeMetadata = new Fb2NodeMetadata(this.NodeMetadata?.DefaultNamespace);

            content.Add(node);
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
            if (nodePredicate == null)
                throw new ArgumentNullException(nameof(nodePredicate));

            if (!IsEmpty)
            {
                var nodesToRemove = content.Where(n => nodePredicate(n)).ToList();
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
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (!IsEmpty && content.Contains(node))
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
            if (!IsEmpty)
                for (int i = content.Count - 1; i >= 0; i--)
                    RemoveContent(content[i]);

            return this;
        }

        #endregion

        #region Content querying

        /// <summary>
        /// Gets children of element by given name. 
        /// </summary>
        /// <param name="name">Name to select child elements by. Case insensitive.</param>
        /// <returns>List of found child elements, if any.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<Fb2Node> GetChildren(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (!Fb2NodeFactory.IsKnownNodeName(name))
                return Enumerable.Empty<Fb2Node>();

            return content.Where(elem => elem.Name.EqualsInvariant(name));
        }

        /// <summary>
        /// Gets children of element that match given predicate.
        /// </summary>
        /// <param name="predicate">Predicate to match child node against.</param>
        /// <returns>List of found child elements, if any.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<Fb2Node> GetChildren(Func<Fb2Node, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return content.Where(c => predicate(c));
        }

        /// <summary>
        /// Gets first matching child of element by given name.
        /// </summary>
        /// <param name="name">Name to select child element by.</param>
        /// <returns>First matched child node.</returns>
        public Fb2Node? GetFirstChild(string? name)
        {
            if (IsEmpty ||
                !string.IsNullOrWhiteSpace(name) &&
                !Fb2NodeFactory.IsKnownNodeName(name))
                return null;

            return string.IsNullOrWhiteSpace(name) ?
                content.FirstOrDefault() :
                content.FirstOrDefault(elem => elem.Name.EqualsInvariant(name));
        }

        /// <summary>
        /// Gets first child of element that matches given predicate.
        /// </summary>
        /// <param name="predicate">Predicate to match child node against.</param>
        /// <returns>First matched child node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node? GetFirstChild(Func<Fb2Node, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return content.FirstOrDefault(predicate);
        }

        /// <summary>
        /// Recursively gets all descendants of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<Fb2Node> GetDescendants(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var result = new List<Fb2Node>();

            if (IsEmpty || !Fb2NodeFactory.IsKnownNodeName(name))
                return result;

            for (int i = 0; i < content.Count; i++)
            {
                var element = content[i];

                if (element.Name.EqualsInvariant(name))
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
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var result = new List<Fb2Node>();

            if (IsEmpty)
                return result;

            for (int i = 0; i < content.Count; i++)
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
        /// <returns>First matching descendant node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node? GetFirstDescendant(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            if (IsEmpty || !Fb2NodeFactory.IsKnownNodeName(name))
                return null;

            for (int i = 0; i < content.Count; i++)
            {
                var element = content[i];

                if (element.Name.EqualsInvariant(name))
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
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (IsEmpty)
                return null;

            for (int i = 0; i < content.Count; i++)
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
            if (IsEmpty)
                return Enumerable.Empty<T>();

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = content.Where(predicate);

            return result.Any() ? result.Cast<T>() : Enumerable.Empty<T>();
        }

        /// <summary>
        /// Gets first matching child of element by given node type (Fb2Node-based nodes). 
        /// </summary>
        /// <param name="name">Node type to select child element by.</param>
        /// <returns>First matched child node</returns>
        public T? GetFirstChild<T>() where T : Fb2Node
        {
            if (IsEmpty)
                return null;

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = content.FirstOrDefault(predicate);

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
            if (IsEmpty)
                return Enumerable.Empty<T>();

            var result = new List<Fb2Node>();

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            for (int i = 0; i < content.Count; i++)
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
            if (!content.Any())
                return null;

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            for (int i = 0; i < content.Count; i++)
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
            var lastChildNode = IsEmpty ? null : content.LastOrDefault();

            // empty or last item is not text, so cant append actual content nowhere
            if (lastChildNode == null ||
                !(lastChildNode is TextItem lastTextItem))
            {
                var textNode = new TextItem { Parent = this }.AddContent(newContent, separator);
                content.Add(textNode);
            }
            else
                lastTextItem.AddContent(newContent, separator);

            return this;
        }

        private XNode ToXmlInternal(Fb2Node element) =>
            element is TextItem textItem ? (XNode)new XText(textItem.Content) : element.ToXml();

        #endregion
    }
}

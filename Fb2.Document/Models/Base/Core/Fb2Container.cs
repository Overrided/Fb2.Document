using System;
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
        /// Creates copy of actual content. 
        /// Actual value is available after `Load()` method call.
        /// </summary>
        /// <returns>`List<Fb2Node>` which reflects content of given `XNode`.</returns>
        // TODO: check if equality override is ok + check if cloning is done right
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
        /// `true` if element is inline, otherwise - `false`.
        /// </summary>
        public override bool IsInline { get; protected set; } = false;

        /// <summary>
        /// Container node loading mechanism. Loads attributes and sequentially calls `Load` on all child nodes.
        /// </summary>
        /// <param name="node">Node to load as Fb2Container</param>
        /// <param name="preserveWhitespace">Indicates if whitespace chars (\t, \n, \r) should be preserved. By default `false`.</param>
        /// <param name="loadUnsafe"> Indicates whether "Unsafe" children should be loaded. By default `true`. </param>
        public override void Load(
            [In] XNode node,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            base.Load(node, preserveWhitespace, loadUnsafe);

            var element = node as XElement;

            if (element == null || element.IsEmpty)
                return;

            var nodes = element.Nodes()
                .Where(n => (n.NodeType == XmlNodeType.Text) ||
                            ((n.NodeType == XmlNodeType.Element) &&
                            Fb2NodeFactory.IsKnownNode((n as XElement).Name.LocalName.ToLowerInvariant())));

            if (!nodes.Any())
                return;

            // TODO : implement `for` as micro-optimization?)
            foreach (var validNode in nodes)
            {
                string localName = validNode.NodeType == XmlNodeType.Element ?
                    ((XElement)validNode).Name.LocalName.ToLowerInvariant() :
                    ElementNames.FictionText;

                var isUnsafe = validNode.NodeType == XmlNodeType.Text ? !CanContainText : !AllowedElements.Contains(localName);

                if (isUnsafe && !loadUnsafe)
                    continue;

                var elem = Fb2NodeFactory.GetNodeByName(localName);
                elem.Load(validNode, preserveWhitespace, loadUnsafe);
                elem.IsUnsafe = isUnsafe;

                content.Add(elem);
            }
        }

        public override string ToString()
        {
            if (!content.Any()) // TODO test if .content > 0 is faster
                return string.Empty;

            var builder = new StringBuilder();

            foreach (var child in content)
                builder.Append(child.IsInline ? child.ToString() : $"{Environment.NewLine}{child}");

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

            if (!content.Any())
                return element;

            var children = content.Select(ToXmlInternal);
            element.Add(children);

            return element;
        }

        public override bool Equals(object other)
        {
            if (!(other is Fb2Container otherContainer))
                return false;

            if (!base.Equals(other))
                return false;

            var actualContent = content;
            var otherContent = otherContainer.content;
            var sameContent = actualContent.Count == otherContent.Count && actualContent.All(c => otherContent.Contains(c));

            var result = sameContent &&
                CanContainText == otherContainer.CanContainText &&
                AllowedElements.SequenceEqual(otherContainer.AllowedElements);

            return result;
        }

        public sealed override int GetHashCode() => HashCode.Combine(base.GetHashCode(), CanContainText, content, AllowedElements);

        public sealed override object Clone()
        {
            var container = base.Clone() as Fb2Container;
            container.content = new List<Fb2Node>(content.Select(c => (Fb2Node)c.Clone()));
            container.CanContainText = CanContainText;

            return container;
        }

        #region Content editing

        public async Task<Fb2Container> AddContentAsync(Func<Task<Fb2Node>> nodeProvider)
        {
            if (nodeProvider == null)
                throw new ArgumentNullException(nameof(nodeProvider));

            var newNode = await nodeProvider();
            return AddContent(newNode);
        }

        public Fb2Container AddContent(Func<Fb2Node> nodeProvider)
        {
            if (nodeProvider == null)
                throw new ArgumentNullException(nameof(nodeProvider));

            return AddContent(nodeProvider());
        }

        public Fb2Container AddContent(params Fb2Node[] nodes)
        {
            if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
                throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

            foreach (var node in nodes)
                AddContent(node);

            return this;
        }

        public Fb2Container AddTextContent(string content,
            string separator = null,
            bool preserveWhitespace = false)
        {
            if (!CanContainText)
                throw new UnexpectedNodeException(Name, "text");

            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException(nameof(content), $"{nameof(content)} is null or empty string.");

            var textItem = new TextItem().AddContent(content, separator, preserveWhitespace);

            return AddContent(textItem);
        }

        public Fb2Container AddContent(IEnumerable<Fb2Node> nodes)
        {
            if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
                throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

            foreach (var node in nodes)
                AddContent(node);

            return this;
        }

        public Fb2Container AddContent(string nodeName)
        {
            if (string.IsNullOrWhiteSpace(nodeName))
                throw new ArgumentNullException(nameof(nodeName));

            var node = Fb2NodeFactory.GetNodeByName(nodeName);

            return AddContent(node);
        }

        public Fb2Container AddContent(Fb2Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var nodeName = node.Name;

            if (!Fb2NodeFactory.IsKnownNode(nodeName)) // just for lulz
                throw new UnknownNodeException(nodeName);

            if (node.Name.Equals(ElementNames.FictionText) && !CanContainText)
                throw new UnexpectedNodeException(Name, nodeName);

            if (!AllowedElements.Contains(nodeName) && !nodeName.Equals(ElementNames.FictionText))
                throw new UnexpectedNodeException(Name, nodeName);

            content.Add(node);

            return this;
        }

        public Fb2Container RemoveContent(IEnumerable<Fb2Node> nodes)
        {
            if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
                throw new ArgumentNullException(nameof(nodes), $"{nameof(nodes)} is null or empty array, or contains only null's");

            foreach (var node in nodes)
                RemoveContent(node);

            return this;
        }

        public Fb2Container RemoveContent(Func<Fb2Node, bool> nodePredicate)
        {
            if (nodePredicate == null)
                throw new ArgumentNullException(nameof(nodePredicate));

            if (!content.Any())
                return this;

            content.RemoveAll(n => nodePredicate(n));

            return this;
        }

        public Fb2Container RemoveContent(Fb2Node node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (content.Any())
                content.Remove(node);

            return this;
        }

        public Fb2Container ClearContent()
        {
            if (content.Any())
                content.Clear();

            return this;
        }

        #endregion

        #region Content querying

        /// <summary>
        /// Gets children of element by given name. 
        /// </summary>
        /// <param name="name">Name to select child elements by. Case insensitive.</param>
        /// <returns>List of found child elements, if any. </returns>
        public IEnumerable<Fb2Node> GetChildren(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), $"{nameof(name)} is null or empty string. Use `{nameof(Content)}` instead.");

            return content.Where(elem => elem.Name.EqualsInvariant(name));
        }

        /// <summary>
        /// Gets first matching child of element by given name.
        /// </summary>
        /// <param name="name">Name to select child element by.</param>
        /// <returns>First matched child node</returns>
        public Fb2Node GetFirstChild(string name) =>
            string.IsNullOrWhiteSpace(name) ?
                content.FirstOrDefault() :
                content.FirstOrDefault(elem => elem.Name.EqualsInvariant(name));

        /// <summary>
        /// Recursively gets all descendants of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        public IEnumerable<Fb2Node> GetDescendants(string name)
        {
            if (!content.Any())
                return null;

            var result = new List<Fb2Node>();

            foreach (var element in content)
            {
                if (string.IsNullOrWhiteSpace(name) ||
                    element.Name.EqualsInvariant(name))
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
        /// Recursively looks for first matching descendant of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendant by.</param>
        /// <returns>First matched descendant node.</returns>
        public Fb2Node GetFirstDescendant(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), $"{nameof(name)} is null or empty string! Use {nameof(GetFirstChild)} method instead.");

            if (!content.Any())
                return null;

            foreach (var element in content)
            {
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
        /// Recursively looks for first matching descendant of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendant by.</param>
        /// <param name="node">Out param, actual result of a search.</param>
        /// <returns>Boolean value indicating if any node was actually found. Node itself is returned as out parameter.</returns>
        public bool TryGetFirstDescendant(string name, out Fb2Node node)
        {
            var firstDescendant = GetFirstDescendant(name);
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
            if (!content.Any())
                return null;

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = content.Where(predicate);

            if (result == null || !result.Any())
                return null;

            return result.Cast<T>();
        }

        /// <summary>
        /// Gets first matching child of element by given node type (Fb2Node-based nodes). 
        /// </summary>
        /// <param name="name">Node type to select child element by.</param>
        /// <returns>First matched child node</returns>
        public T GetFirstChild<T>() where T : Fb2Node
        {
            if (!content.Any())
                return null;

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = content.FirstOrDefault(predicate);

            if (result == null)
                return null;

            return result as T;
        }

        /// <summary>
        /// Recursively gets all descendants of element by given node type (Fb2Node-based nodes). 
        /// </summary>
        /// <param name="name">Node type to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        public IEnumerable<T> GetDescendants<T>() where T : Fb2Node => GetDescendantsInternal<T>();

        /// <summary>
        /// Recursively looks for first matching descendant of element by given node type (Fb2Node-based nodes).
        /// </summary>
        /// <param name="name">Node type to select descendant by.</param>
        /// <returns>First matched descendant node.</returns>
        public T GetFirstDescendant<T>() where T : Fb2Node => GetFirstDescendantInternal<T>();

        /// <summary>
        /// Recursively looks for first matching descendant of element by given node type (Fb2Node-based nodes).
        /// </summary>
        /// <param name="name">Node type to select descendant by.</param>
        /// <param name="node">Out param, actual result of a search.</param>
        /// <returns>Boolean value indicating if any node was actually found. Node itself is returned as out parameter.</returns>
        public bool TryGetFirstDescendant<T>(out T node) where T : Fb2Node
        {
            var result = GetFirstDescendantInternal<T>();
            node = result;
            return result != null;
        }

        #endregion

        #region Private Methods

        private IEnumerable<T> GetDescendantsInternal<T>(Func<Fb2Node, bool> predicate = null)
            where T : Fb2Node
        {
            if (!content.Any())
                return null;

            var result = new List<Fb2Node>();

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            foreach (var element in content)
            {
                if (predicate(element))
                    result.Add(element);

                if (element is Fb2Container containerElement)
                {
                    var desc = containerElement.GetDescendantsInternal<T>(predicate);
                    if (desc != null && desc.Any())
                        result.AddRange(desc);
                }
            }

            return result.Cast<T>();
        }

        private T GetFirstDescendantInternal<T>(Func<Fb2Node, bool> predicate = null)
            where T : Fb2Node
        {
            if (!content.Any())
                return null;

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            foreach (var element in content)
            {
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

        private XNode ToXmlInternal(Fb2Node element)
        {
            if (element.Name.Equals(ElementNames.FictionText))
            {
                var text = element as Fb2Element;
                return new XText(text.ToString());
            }

            return element.ToXml();
        }

        #endregion
    }
}

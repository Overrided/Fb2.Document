using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Constants;
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
        /// <summary>
        /// List of actual Content that is available after Load() method call
        /// Reflectes content of given XNode
        /// Dependant on CanContainText property can or can not contain text nodes
        /// </summary>
        public List<Fb2Node> Content { get; } = new List<Fb2Node>();

        /// <summary>
        /// Indicates if instance of type Fb2Container can contain text
        /// True if element can contain text, otherwise - false
        /// </summary>
        public abstract bool CanContainText { get; }

        /// <summary>
        /// List of allowed descendant node's names
        /// </summary>
        public abstract HashSet<string> AllowedElements { get; }

        /// <summary>
        /// Indicates whether element should start with a new line or be inline
        /// True if element is inline, otherwise - false
        /// </summary>
        public override bool IsInline { get; protected set; } = false;

        /// <summary>
        /// Container node loading mechanism. Loads attributes and sequentially calls `Load` on all child nodes.
        /// </summary>
        /// <param name="node">Node to load as Fb2Container</param>
        /// <param name="preserveWhitespace">Indicates if whitespace chars (\t, \n, \r) should be preserved. By default `false`</param>
        public override void Load(XNode node, bool preserveWhitespace = false)
        {
            base.Load(node, preserveWhitespace);

            var element = node as XElement;

            if (element == null || element.IsEmpty)
                return;

            var nodes = element.Nodes()
                .Where(n => (n.NodeType == XmlNodeType.Text) ||
                            ((n.NodeType == XmlNodeType.Element) &&
                            Fb2ElementFactory.Instance.KnownNodes.ContainsKey((n as XElement).Name.LocalName.ToLowerInvariant())));

            if (!nodes.Any())
                return;

            foreach (var validNode in nodes)
            {
                string localName = validNode.NodeType == XmlNodeType.Element ?
                    ((XElement)validNode).Name.LocalName.ToLowerInvariant() :
                    ElementNames.FictionText;

                var elem = Fb2ElementFactory.Instance.GetElementByNodeName(localName);
                elem.Load(validNode, preserveWhitespace);

                elem.Unsafe = validNode.NodeType == XmlNodeType.Text ? !CanContainText : !AllowedElements.Contains(localName);

                Content.Add(elem);
            }
        }

        public override string ToString()
        {
            if (Content == null || !Content.Any())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Content.Count; i++)
            {
                var child = Content[i];
                var childContent = child.ToString();

                sb.Append(child.IsInline ?
                    childContent :
                    $"{Environment.NewLine}{childContent}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts Fb2Container to XElement with regards to all attributes, 
        /// by calling `ToXml()` on every node in `Content`.
        /// </summary>
        /// <returns>XElement reflected from given Fb2Element</returns>
        public override XElement ToXml()
        {
            var element = base.ToXml();

            if (Content == null || !Content.Any())
                return element;

            var children = Content.Select(ToXmlInternal);
            element.Add(children);
            return element;
        }

        #region Node Actions Methods

        /// <summary>
        /// Gets children of element by given name. 
        /// </summary>
        /// <param name="name">Name to select child elements by. Case insensitive.</param>
        /// <returns>List of found child elements, if any. </returns>
        public List<Fb2Node> GetChildren(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} is null or empty string! Use `{nameof(Content)}` property directly instead.");

            return Content.Where(elem => elem.Name.EqualsInvariant(name)).ToList();
        }

        /// <summary>
        /// Gets first matching child of element by given name.
        /// </summary>
        /// <param name="name">Name to select child element by.</param>
        /// <returns>First matched child node</returns>
        public Fb2Node GetFirstChild(string name)
        {
            return string.IsNullOrWhiteSpace(name) ?
                            Content.FirstOrDefault() :
                            Content.FirstOrDefault(elem => elem.Name.EqualsInvariant(name));
        }

        /// <summary>
        /// Recursively gets all descendants of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        public List<Fb2Node> GetDescendants(string name)
        {
            if (Content == null || !Content.Any())
                return null;

            List<Fb2Node> result = new List<Fb2Node>();

            var isEmptyName = string.IsNullOrWhiteSpace(name);

            foreach (var element in Content)
            {
                if (isEmptyName || element.Name.EqualsInvariant(name))
                    result.Add(element);

                if (element is Fb2Container)
                {
                    var desc = (element as Fb2Container).GetDescendants(name);

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
                throw new ArgumentNullException($"{nameof(name)} is null or empty string! Use {nameof(GetFirstChild)} method instead.");

            if (Content == null || !Content.Any())
                return null;

            foreach (var element in Content)
            {
                if (element.Name.EqualsInvariant(name))
                    return element;

                if (element is Fb2Container)
                {
                    var firstDesc = (element as Fb2Container).GetFirstDescendant(name);

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
        public List<T> GetChildren<T>() where T : Fb2Node
        {
            if (Content == null || !Content.Any())
                return null;

            IEnumerable<Fb2Node> result = null;

            var predicate = PredicateResolver.Instance.GetPredicate<T>();

            result = Content.Where(predicate);

            if (result == null || !result.Any())
                return null;

            return result.Cast<T>().ToList();
        }

        /// <summary>
        /// Gets first matching child of element by given node type (Fb2Node-based nodes). 
        /// </summary>
        /// <param name="name">Node type to select child element by.</param>
        /// <returns>First matched child node</returns>
        public T GetFirstChild<T>() where T : Fb2Node
        {
            if (Content == null || !Content.Any())
                return null;

            Fb2Node result = null;

            var predicate = PredicateResolver.Instance.GetPredicate<T>();
            result = Content.FirstOrDefault(predicate);

            if (result == null)
                return null;

            return result as T;
        }

        /// <summary>
        /// Recursively gets all descendants of element by given node type (Fb2Node-based nodes). 
        /// </summary>
        /// <param name="name">Node type to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        public List<T> GetDescendants<T>() where T : Fb2Node => GetDescendantsInternal<T>().ToList();

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
            if (Content == null || !Content.Any())
                return null;

            List<Fb2Node> result = new List<Fb2Node>();

            if (predicate == null)
                predicate = PredicateResolver.Instance.GetPredicate<T>();

            foreach (var element in Content)
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
            if (Content == null || !Content.Any())
                return null;

            if (predicate == null)
                predicate = PredicateResolver.Instance.GetPredicate<T>();

            foreach (var element in Content)
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

﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
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
        private List<Fb2Node> content = new List<Fb2Node>();

        /// <summary>
        /// Creates copy of actual content. 
        /// Actual value is available after `Load()` method call.
        /// </summary>
        /// <returns>`List<Fb2Node>` which reflects content of given `XNode`.</returns>
        // TODO : return IEnumerable<FbNode>
        public List<Fb2Node> Content() // TODO: this is bad, need to fully clone stuff
        {
            return new List<Fb2Node>(content.Select(c => c.Clone() as Fb2Node));
        }

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
        public override void Load([In] XNode node, bool preserveWhitespace = false)
        {
            base.Load(node, preserveWhitespace);

            var element = node as XElement;

            if (element == null || element.IsEmpty)
                return;

            var nodes = element.Nodes()
                .Where(n => (n.NodeType == XmlNodeType.Text) ||
                            ((n.NodeType == XmlNodeType.Element) &&
                            Fb2ElementFactory.IsKnownNode((n as XElement).Name.LocalName.ToLowerInvariant())));

            if (!nodes.Any())
                return;

            //content = new List<Fb2Node>();

            foreach (var validNode in nodes)
            {
                string localName = validNode.NodeType == XmlNodeType.Element ?
                    ((XElement)validNode).Name.LocalName.ToLowerInvariant() :
                    ElementNames.FictionText;

                var elem = Fb2ElementFactory.GetNodeByName(localName);
                elem.Load(validNode, preserveWhitespace);

                elem.Unsafe = validNode.NodeType == XmlNodeType.Text ? !CanContainText : !AllowedElements.Contains(localName);

                content.Add(elem);
            }
        }

        public override string ToString()
        {
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return string.Empty;

            var builder = new StringBuilder();

            foreach (var child in actualContent)
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

            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return element;

            var children = actualContent.Select(ToXmlInternal);
            element.Add(children);

            return element;
        }

        #region Node editing

        public Fb2Container WithContent(params Func<Fb2Node>[] elementProviders)
        {
            if (elementProviders == null ||
                !elementProviders.Any() ||
                elementProviders.All(e => e == null))
                throw new ArgumentNullException($"No {nameof(elementProviders)} received");

            var notNullNodesProviders = elementProviders.Where(n => n != null); // check if needed

            WithContentSafe(() =>
            {
                foreach (var nodeProvider in notNullNodesProviders)
                    WithContent(nodeProvider);
            });

            return this;
        }

        public Fb2Container WithContent(Func<Fb2Node> elementProvider)
        {
            if (elementProvider == null)
                throw new ArgumentNullException($"{nameof(elementProvider)} is null");

            var element = elementProvider();
            return WithContent(element);
        }

        public Fb2Container WithContent(params Fb2Node[] nodes)
        {
            if (nodes == null || !nodes.Any() || nodes.All(n => n == null))
                throw new ArgumentNullException("No nodes received");

            var notNullNodes = nodes.Where(n => n != null); // check if needed

            WithContentSafe(() =>
            {
                foreach (var node in notNullNodes)
                    WithContent(node);
            });

            return this;
        }

        public Fb2Container WithTextContent(string content,
            string separator = null,
            bool preserveWhitespace = false)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentNullException($"{nameof(content)} is null or empty string.");

            if (!CanContainText)
                throw new ArgumentException($"Element {Name} is not designed to contain text (direct content). See {Name}.{nameof(CanContainText)}.");

            var textItem = new TextItem().WithContent(content, separator, preserveWhitespace);

            return WithContent(textItem);
        }

        public Fb2Container WithContent(Fb2Node element)
        {
            if (element == null)
                throw new ArgumentNullException($"{nameof(element)} is null.");

            if (element.Name.Equals(ElementNames.FictionText) && !CanContainText)
                throw new ArgumentException($"Element '{Name}' is not designed to contain text (direct content). See {Name}.{nameof(CanContainText)}.");

            if (!AllowedElements.Contains(element.Name) && !element.Name.Equals(ElementNames.FictionText))
                throw new ArgumentException($"'{element.Name}' is not valid child for '{Name}'. See {Name}.{nameof(AllowedElements)} for valid content elements.");

            content.Add(element);

            return this;
        }

        #endregion

        #region Node querying

        /// <summary>
        /// Gets children of element by given name. 
        /// </summary>
        /// <param name="name">Name to select child elements by. Case insensitive.</param>
        /// <returns>List of found child elements, if any. </returns>
        public List<Fb2Node> GetChildren(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException($"{nameof(name)} is null or empty string. Use `{nameof(Content)}` method instead.");

            return Content().Where(elem => elem.Name.EqualsInvariant(name)).ToList();
        }

        /// <summary>
        /// Gets first matching child of element by given name.
        /// </summary>
        /// <param name="name">Name to select child element by.</param>
        /// <returns>First matched child node</returns>
        public Fb2Node GetFirstChild(string name) =>
            string.IsNullOrWhiteSpace(name) ?
                Content().FirstOrDefault() :
                Content().FirstOrDefault(elem => elem.Name.EqualsInvariant(name));

        /// <summary>
        /// Recursively gets all descendants of element by given name.
        /// </summary>
        /// <param name="name">Name to select descendants by.</param>
        /// <returns>List of found descendants, if any.</returns>
        public List<Fb2Node> GetDescendants(string name)
        {
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            var result = new List<Fb2Node>();

            foreach (var element in actualContent)
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
                throw new ArgumentNullException($"{nameof(name)} is null or empty string! Use {nameof(GetFirstChild)} method instead.");

            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            foreach (var element in actualContent)
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
        public List<T> GetChildren<T>() where T : Fb2Node
        {
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = actualContent.Where(predicate);

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
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            var predicate = PredicateResolver.GetPredicate<T>();
            var result = actualContent.FirstOrDefault(predicate);

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
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            var result = new List<Fb2Node>();

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            foreach (var element in actualContent)
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
            var actualContent = Content();

            if (actualContent == null || !actualContent.Any())
                return null;

            if (predicate == null)
                predicate = PredicateResolver.GetPredicate<T>();

            foreach (var element in actualContent)
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

        private void WithContentSafe(Action contentUpdate)
        {
            var backupContent = Content();

            try
            {
                contentUpdate();
            }
            catch (Exception)
            {
                content.Clear(); // drop all changes if any
                content.AddRange(backupContent);

                throw; // rollback and throw - no changes saved, no side-effects etc
            }
        }

        #endregion

        public override bool Equals(object obj)
        {
            return obj is Fb2Container container &&
                   base.Equals(obj) &&
                   CanContainText == container.CanContainText &&
                   // TODO: check if EqualityComparer is fine
                   EqualityComparer<List<Fb2Node>>.Default.Equals(content, container.content) &&
                   EqualityComparer<ImmutableHashSet<string>>.Default.Equals(AllowedElements, container.AllowedElements);
        }

        public sealed override int GetHashCode() => HashCode.Combine(base.GetHashCode(), CanContainText, content, AllowedElements);

        public sealed override object Clone()
        {
            var container = base.Clone() as Fb2Container;
            container.content = Content();
            container.CanContainText = CanContainText;

            return container;
        }
    }
}

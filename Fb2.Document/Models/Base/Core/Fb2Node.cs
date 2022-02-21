using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;
using Fb2.Document.Models.Attributes;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Base class - describes basic node of fb2 document.
    /// Has Name, list of valid attributes and actual attribute values
    /// </summary>
    public abstract class Fb2Node : ICloneable
    {
        // backing field for `Attributes` property
        private List<Fb2Attribute> attributes = new List<Fb2Attribute>();

        protected static readonly Regex trimWhitespace = new Regex(@"\s+", RegexOptions.Multiline);
        protected const string Whitespace = " ";

        /// <summary>
        /// Node name, used during document parsing and validation.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Indicates if element has any content.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets actual element attributes in <c>ImmutableList&lt;Fb2Attribute&gt;</c> form.
        /// </summary>
        public ImmutableList<Fb2Attribute> Attributes => attributes.ToImmutableList();

        /// <summary>
        /// List of allowed attribure names for particular element.
        /// </summary>
        public virtual ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet<string>.Empty;

        /// <summary>
        /// Indicates if element sholud be inline or start from new line.
        /// </summary>
        public abstract bool IsInline { get; protected set; }

        /// <summary>
        /// Indicates if element is on a right place (has correct parent node etc.) accordingly to Fb2 standard.
        /// </summary>
        public bool IsUnsafe { get; internal set; }

        // logically only container can be parent as only container can have sub-nodes
        /// <summary>
        /// Returns Parent node for current node.
        /// </summary>
        public Fb2Container? Parent { get; internal set; } // as far as we can go to prevent public access to setter of Parent

        /// <summary>
        /// <para>Includes XML info: Default Namespace and namespace declarations attributes.</para>
        /// <para>Is applied during loading/serialization of Fb2Node.</para>
        /// <para>Is not used in Equals and GetHashCode overrides.</para>
        /// </summary>
        public Fb2NodeMetadata? NodeMetadata { get; set; } = null;

        /// <summary>
        /// Basic Load of node - validation and populating Attributes.
        /// </summary>
        /// <param name="node">XNode to load as <c>Fb2Node</c>.</param>
        /// <param name="parentNode">Parent node of node being loaded, can be <see langword="null"/>.</param>
        /// <param name="preserveWhitespace">Is ignored during <c>Fb2Node</c> loading.</param>
        /// <param name="loadUnsafe">Is ignored during <c>Fb2Node</c> loading.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void Load(
            [In] XNode node,
            [In] Fb2Container? parentNode = null,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Validate(node);

            Parent = parentNode;
            LoadMetadata(node);

            if (!AllowedAttributes.Any())
                return;

            if (!TryParseXAttributes(node, out var actualAttributes))
                return;

            var filteredAttributes = actualAttributes
                .Where(kvp => AllowedAttributes.Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase));

            if (!filteredAttributes.Any())
                return;

            attributes.AddRange(filteredAttributes);
        }

        /// <summary>
        /// Basic method to serialize Fb2Node back to XElement.
        /// </summary>
        /// <returns>XElement instance with attributes reflecting Attributes property.</returns>
        public virtual XElement ToXml()
        {
            var defaultNamespace = NodeMetadata?.DefaultNamespace;
            XName xNodeName = defaultNamespace != null ? defaultNamespace + Name : Name;

            var attributesToAdd = SerializeAttributes();

            var element = attributesToAdd.Count > 0 ?
                new XElement(xNodeName, attributesToAdd) :
                new XElement(xNodeName);

            return element;
        }

        #region Node querying

        /// <summary>
        /// Returns a collection of the ancestor elements of this <c>Fb2Node</c>.
        /// </summary>
        /// <returns><c>IEnumerable&lt;Fb2Container&gt;</c> of the ancestor elements of this <c>Fb2Node</c>.</returns>
        public IEnumerable<Fb2Container> GetAncestors()
        {
            var parent = this.Parent;
            if (parent == null)
                return Enumerable.Empty<Fb2Container>();

            var result = new List<Fb2Container> { parent };

            var parents = parent.GetAncestors();
            if (parents.Any())
                result.AddRange(parents);

            return result;
        }

        /// <summary>
        /// Checks if given node has particular <paramref name="fb2Attribute"/>.
        /// </summary>
        /// <param name="fb2Attribute"><c>Fb2Attribute</c> to look for.</param>
        /// <returns><see langword="true"/> if <c>Fb2Node</c> has given <paramref name="fb2Attribute"/>, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool HasAttribute(Fb2Attribute fb2Attribute)
        {
            if (fb2Attribute == null)
                throw new ArgumentNullException(nameof(fb2Attribute));

            if (!attributes.Any())
                return false;

            var hasAttribute = attributes.Contains(fb2Attribute);
            return hasAttribute;
        }

        /// <summary>
        /// Checks if node has attribute(s) with given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to search attribute by.</param>
        /// <param name="ignoreCase">Indicates if case-sensitive key comparison should be used.</param>
        /// <returns><see langword="true"/> if attribute with given <paramref name="key"/> found, otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool HasAttribute(string key, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!attributes.Any())
                return false;

            return ignoreCase ?
                attributes.Any(attr => attr.Key.EqualsInvariant(key)) :
                attributes.Any(attr => attr.Key.Equals(key, StringComparison.InvariantCulture));
        }

        /// <summary>
        /// Gets first matching <c>attribute</c> by given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to search attribute by.</param>
        /// <param name="ignoreCase">Indicates if case-sensitive <paramref name="key"/> comparison should be used.</param>
        /// <returns>
        /// Returns first matching attribute by given <paramref name="key"/> or <c>default(Fb2Attribute)</c> if no such element is found.
        /// </returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Attribute? GetAttribute(string key, bool ignoreCase = false)
        {
            if (!HasAttribute(key, ignoreCase))
                return null;

            var attribute = ignoreCase ?
                attributes.FirstOrDefault(attr => attr.Key.EqualsInvariant(key)) :
                attributes.FirstOrDefault(attr => attr.Key.Equals(key, StringComparison.InvariantCulture));

            return attribute;
        }

        /// <summary>
        /// Attempts to get first matching <c>Fb2Attribute</c> by given <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to match attribute by.</param>
        /// <param name="result">First matching <seealso cref="Fb2Attribute"/> if found, otherwise <seealso cref="default(Fb2Attribute)"/>.</param>
        /// <param name="ignoreCase">Indicates if case-sensitive <paramref name="key"/> comparison should be used.</param>
        /// <returns><see langword="true"/> if attribute with given <paramref name="key"/> found, otherwise <see langword="false"/>.</returns>
        public bool TryGetAttribute(string key, out Fb2Attribute? result, bool ignoreCase = false)
        {
            var attribute = GetAttribute(key, ignoreCase);

            result = attribute;
            return attribute != null;
        }

        #endregion

        #region Node editing

        /// <summary>
        /// Adds multiple attributes to node using <seealso cref="params Fb2Attribute[]"/>.
        /// </summary>
        /// <param name="attributes">Set of attributes to add.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node AddAttributes(params Fb2Attribute[] attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes));

            foreach (var attribute in attributes)
                AddAttribute(attribute);

            return this;
        }

        /// <summary>
        /// Adds multiple attributes using <seealso cref="IEnumerable{Fb2Attribute}." />
        /// </summary>
        /// <param name="attributes">Set of attributes to add.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node AddAttributes(IEnumerable<Fb2Attribute> attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes), $"{nameof(attributes)} is null or empty dictionary.");

            foreach (var attribute in attributes)
                AddAttribute(attribute);

            return this;
        }

        /// <summary>
        /// Adds single attribute to <see cref="Fb2Node.Attributes"/> using asynchronous <paramref name="attributeProvider"/> function.
        /// </summary>
        /// <param name="attributeProvider">Asynchronous attribute provider function.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Fb2Node> AddAttributeAsync(Func<Task<Fb2Attribute>> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = await attributeProvider();

            return AddAttribute(attribute);
        }

        /// <summary>
        /// Adds single attribute to <see cref="Fb2Node.Attributes"/> using <paramref name="attributeProvider"/> function.
        /// </summary>
        /// <param name="attributeProvider">Attribute provider function.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node AddAttribute(Func<Fb2Attribute> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = attributeProvider();

            return AddAttribute(attribute);
        }

        /// <summary>
        /// Adds single attribute using <paramref name="key"/>, <paramref name="value"/> and <paramref name="namespaceName"/>.
        /// </summary>
        /// <param name="key">Attribute key to add.</param>
        /// <param name="value">Attribute value to add.</param>
        /// <param name="namespaceName">
        /// <para>Optional, can be <see langword="null"/>.</para>
        /// <para>NamespaceName for attribute, used by <see cref="ToXml"/> serialization.</para></param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttribute(string key, string value, string? namespaceName = null)
        {
            var fb2Attribute = new Fb2Attribute(key, value, namespaceName);
            return AddAttribute(fb2Attribute);
        }

        /// <summary>
        /// Adds single attribute to given node.
        /// </summary>
        /// <param name="fb2Attribute">Attribute to add to given node.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NoAttributesAllowedException"></exception>
        /// <exception cref="UnexpectedAtrributeException"></exception>
        public Fb2Node AddAttribute(Fb2Attribute fb2Attribute)
        {
            if (fb2Attribute == null)
                throw new ArgumentNullException(nameof(fb2Attribute));

            if (!AllowedAttributes.Any())
                throw new NoAttributesAllowedException(Name);

            var key = fb2Attribute.Key;

            if (!AllowedAttributes.Contains(key))
                throw new UnexpectedAtrributeException(Name, key);

            // update or insert
            if (TryGetAttribute(key, out var existingAttribute, true))
            {
                var existingAttributeIndex = attributes.IndexOf(existingAttribute!);
                attributes[existingAttributeIndex] = fb2Attribute; // replace existing, should not be -1
            }
            else
                attributes.Add(fb2Attribute);

            return this;
        }

        /// <summary>
        /// Removes attribute from <see cref="Attributes"/> by given attribute key.
        /// </summary>
        /// <param name="key">Key to remove attribute by.</param>
        /// <param name="ignoreCase">Indicates if case-sensitive <paramref name="key"/> comparison should be used.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node RemoveAttribute(string key, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (!attributes.Any())
                return this;

            var attributesToDelete = attributes
                .Where(existingAttr => ignoreCase ? existingAttr.Key.EqualsInvariant(key) : existingAttr.Key.Equals(key))
                .ToList();

            foreach (var attributeToRemove in attributesToDelete)
                RemoveAttribute(attributeToRemove);

            return this;
        }

        /// <summary>
        /// Removes attributes matching given <paramref name="attributePredicate"/>.
        /// </summary>
        /// <param name="attributePredicate">Predicate function to match attributes against.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node RemoveAttribute(Func<Fb2Attribute, bool> attributePredicate)
        {
            if (attributePredicate == null)
                throw new ArgumentNullException(nameof(attributePredicate));

            if (!attributes.Any())
                return this;

            var attrsToRemove = attributes.Where(attributePredicate).ToList();

            foreach (var attributeToRemove in attrsToRemove)
                RemoveAttribute(attributeToRemove);

            return this;
        }

        /// <summary>
        /// Removes <paramref name="fb2Attribute"/> from given node.
        /// </summary>
        /// <param name="fb2Attribute">Attribute to remove.</param>
        /// <returns>Current node.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Fb2Node RemoveAttribute(Fb2Attribute fb2Attribute)
        {
            if (fb2Attribute == null)
                throw new ArgumentNullException(nameof(fb2Attribute));

            if (attributes.Contains(fb2Attribute))
                attributes.Remove(fb2Attribute);

            return this;
        }

        /// <summary>
        /// Clears <see cref="Attributes"/>
        /// </summary>
        /// <returns>Current node.</returns>
        public Fb2Node ClearAttributes()
        {
            if (attributes.Any())
                attributes.Clear();

            return this;
        }

        #endregion

        private void LoadMetadata([In] XNode node)
        {
            if (!(node is XElement element))
                return;

            var defaultNodeNamespace = element.GetDefaultNamespace();

            var namespacesAttrs = element
                .Attributes()
                .Where(a => a.IsNamespaceDeclaration);

            NodeMetadata = new Fb2NodeMetadata(defaultNodeNamespace, namespacesAttrs);
        }

        private static bool TryParseXAttributes([In] XNode node, out IEnumerable<Fb2Attribute> result)
        {
            if (!(node is XElement element))
            {
                result = Enumerable.Empty<Fb2Attribute>();
                return false;
            }

            var actualAttrs = element.Attributes();
            if (!actualAttrs.Any())
            {
                result = Enumerable.Empty<Fb2Attribute>();
                return false;
            }

            result = actualAttrs.Select(attr => new Fb2Attribute(attr.Name.LocalName, attr.Value, attr.Name.Namespace?.NamespaceName));
            return true;
        }

        private List<XAttribute> SerializeAttributes()
        {
            var result = new List<XAttribute>();

            var nodeNamespaceDeclarations = NodeMetadata?.NamespaceDeclarations;
            if (nodeNamespaceDeclarations != null && nodeNamespaceDeclarations.Any()) // namespaces
                result.AddRange(nodeNamespaceDeclarations);

            if (attributes.Any()) // regular attributes
            {
                var convertedAttributes = attributes.Select(attr =>
                {
                    if (string.IsNullOrWhiteSpace(attr.NamespaceName))
                        return new XAttribute(attr.Key, attr.Value); // no prefix - id attribute for example
                    else
                    {
                        XNamespace attrNamespace = attr.NamespaceName;
                        XName attributeName = attrNamespace + attr.Key;

                        return new XAttribute(attributeName, attr.Value); // attribute with namespace prefix - like l:href
                    }
                });

                result.AddRange(convertedAttributes);
            }

            return result;
        }

        protected void Validate(XNode node)
        {
            if (node.NodeType != XmlNodeType.Element)
                return;

            if (node is XElement element && !element.Name.LocalName.EqualsInvariant(Name))
                throw new Fb2NodeLoadingException($"Invalid element, element name is {element.Name.LocalName}, expected {Name}");
        }

        public override bool Equals(object? other)
        {
            if (other == null)
                return false;

            if (!(other is Fb2Node otherNode))
                return false;

            if (ReferenceEquals(this, otherNode))
                return true;

            var result = Name == otherNode.Name &&
                AllowedAttributes.SequenceEqual(otherNode.AllowedAttributes) &&
                AreAttributesEqual(otherNode.attributes) &&
                IsInline == otherNode.IsInline &&
                IsUnsafe == otherNode.IsUnsafe;

            return result;
        }

        private bool AreAttributesEqual(List<Fb2Attribute> otherAttributes)
        {
            if (ReferenceEquals(attributes, otherAttributes))
                return true;

            return attributes.Count == otherAttributes.Count &&
                   attributes.All(k => otherAttributes.Contains(k));
        }

        public override int GetHashCode() => HashCode.Combine(Name, attributes, AllowedAttributes, IsInline, IsUnsafe);

        public virtual object Clone()
        {
            var node = Fb2NodeFactory.GetNodeByName(Name);

            if (attributes.Any())
                node.attributes = new List<Fb2Attribute>(attributes);

            node.IsInline = IsInline;
            node.IsUnsafe = IsUnsafe;
            node.Parent = Parent;
            node.NodeMetadata = NodeMetadata;

            return node;
        }
    }
}

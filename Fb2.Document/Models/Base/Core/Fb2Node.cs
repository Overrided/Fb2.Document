using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;

namespace Fb2.Document.Models.Base
{
    // TODO : add `Fb2Node Parent` get-only property to traverse tree better

    /// <summary>
    /// Base class - describes basic node of fb2 document.
    /// Has Name, list of valid attributes and actual attribute values
    /// </summary>
    public abstract class Fb2Node : ICloneable
    {
        protected static readonly Regex trimWhitespace = new(@"\s+", RegexOptions.Multiline);

        private Dictionary<string, string> attributes = new();

        /// <summary>
        /// Node name, used during document parsing and validation.
        /// </summary>
        public abstract string Name { get; }

        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets actual element attributes in key - value (Dictionary) form.
        /// </summary>
        public ImmutableDictionary<string, string> Attributes => attributes.ToImmutableDictionary();

        /// <summary>
        /// List of allowed attribures for particular element.
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

        /// <summary>
        /// Basic Load of element - validation and populating Attributes.
        /// </summary>
        /// <param name="node">XNode to load as Fb2Node</param>
        /// <param name="preserveWhitespace"> Is ignored by Fb2Node loading.</param>
        /// <param name="loadUnsafe"> Is ignored by Fb2Node loading.</param>
        public virtual void Load(
            [In] XNode node,
            bool preserveWhitespace = false,
            bool loadUnsafe = true)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Validate(node);

            if (!AllowedAttributes.Any())
                return;

            if (!TryGetXNodeAttributes(node, out Dictionary<string, string> actualAttributes))
                return;

            var filteredAttributes = actualAttributes
                .Where(kvp => AllowedAttributes.Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase));

            if (!filteredAttributes.Any())
                return;

            foreach (var kvp in filteredAttributes)
                attributes.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Basic method to serialize Fb2Node back to XElement
        /// </summary>
        /// <returns>XElement instance with attributes reflecting Attributes property </returns>
        public virtual XElement ToXml()
        {
            var element = attributes.Any() ?
                new XElement(Name, attributes.Select(attr => new XAttribute(attr.Key, attr.Value))) :
                new XElement(Name);

            return element;
        }

        #region Node querying

        public bool HasAttribute(string key, string value, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            if (!attributes.Any())
                return false;

            var hasAttribute = ignoreCase ?
                attributes.Any(a => a.Key.EqualsInvariant(key) && a.Value.EqualsInvariant(value)) :
                attributes.Any(a => a.Key.Equals(key, StringComparison.InvariantCulture) && a.Value.Equals(value, StringComparison.InvariantCulture));

            return hasAttribute;
        }

        /// <summary>
        /// Checks if node has attribute(s) with given key
        /// </summary>
        /// <param name="key">Key to search attribute by</param>
        /// <param name="ignoreCase">true to ignore case; false to consider case in key comparison</param>
        /// <returns>True if attribute found, otherwise false</returns>
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
        /// Returns the first element of Attributes list that matches given key or a default value if no such element is found.
        /// </summary>
        /// <param name="key">Key to search attribute by</param>
        /// <param name="ignoreCase">true to ignore case; false to consider case in key comparison</param>
        /// <returns>
        /// Returns the first element of Attributes list that matches given key or a default value if no such element is found.
        /// </returns>
        public KeyValuePair<string, string> GetAttribute(string key, bool ignoreCase = false)
        {
            if (!HasAttribute(key, ignoreCase))
                return default;

            var attribute = ignoreCase ?
                attributes.FirstOrDefault(attr => attr.Key.EqualsInvariant(key)) :
                attributes.FirstOrDefault(attr => attr.Key.Equals(key, StringComparison.InvariantCulture));

            return attribute;
        }

        /// <summary>
        /// Looks for attribute by given `attributeName`
        /// </summary>
        /// <param name="key">Key to search attribute by</param>
        /// <param name="ignoreCase">true to ignore case; false to consider case in key comparison</param>
        /// <param name="result">Attribute value if any found, otherwise default. </param>
        /// <returns>True if attribute found by given key, otherwise false</returns>
        public bool TryGetAttribute(string key, out KeyValuePair<string, string> result, bool ignoreCase = false)
        {
            var attribute = GetAttribute(key, ignoreCase);

            result = attribute;
            return !string.IsNullOrWhiteSpace(attribute.Key) && !string.IsNullOrWhiteSpace(attribute.Value);
        }

        #endregion

        #region Node editing

        /// <summary>
        /// Adds multiple attributes in `params KeyValuePair<string, string>[]` form to <see cref="Attributes"/>.
        /// </summary>
        /// <param name="attributes">Attributes to add.</param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttributes(params KeyValuePair<string, string>[] attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes));

            foreach (var attribute in attributes)
                AddAttribute(attribute.Key, attribute.Value);

            return this;
        }

        /// <summary>
        /// Adds multiple attributes in `IDictionary<string, string>` form to <see cref="Attributes"/>.
        /// </summary>
        /// <param name="attributes">Attributes to add.</param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttributes(IDictionary<string, string> attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes), $"{nameof(attributes)} is null or empty dictionary.");

            foreach (var attribute in attributes)
                AddAttribute(attribute.Key, attribute.Value);

            return this;
        }

        /// <summary>
        /// Adds single attribute to <see cref="Attributes"/> using asynchronous provider function.
        /// </summary>
        /// <param name="attributeProvider">Asynchronous attribute provider function.</param>
        /// <returns>Current node.</returns>
        public async Task<Fb2Node> AddAttributeAsync(Func<Task<KeyValuePair<string, string>>> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = await attributeProvider();

            return AddAttribute(attribute.Key, attribute.Value);
        }

        /// <summary>
        /// Adds single attribute to <see cref="Attributes"/> using provider function.
        /// </summary>
        /// <param name="attributeProvider">Attribute provider function.</param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttribute(Func<KeyValuePair<string, string>> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = attributeProvider();

            return AddAttribute(attribute.Key, attribute.Value);
        }

        /// <summary>
        /// Adds single attribute to <see cref="Attributes"/>.
        /// </summary>
        /// <param name="attribute">Attribute to add to <see cref="Attribute"/>.</param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttribute(KeyValuePair<string, string> attribute) =>
            AddAttribute(attribute.Key, attribute.Value);

        /// <summary>
        /// Adds single attribute to <see cref="Attributes"/> using separate key & value.
        /// </summary>
        /// <param name="attributeName">Attribute key.</param>
        /// <param name="attributeValue">Attribute value.</param>
        /// <returns>Current node.</returns>
        public Fb2Node AddAttribute(string attributeName, string attributeValue)
        {
            if (!AllowedAttributes.Any())
                throw new NoAttributesAllowedException(Name);

            if (string.IsNullOrWhiteSpace(attributeValue))
                throw new InvalidAttributeException(nameof(attributeValue));

            if (string.IsNullOrWhiteSpace(attributeName) ||
                trimWhitespace.IsMatch(attributeName))
                throw new InvalidAttributeException(nameof(attributeName));

            var escapedAttrName = SecurityElement.Escape(attributeName);

            if (!AllowedAttributes.Contains(escapedAttrName))
                throw new UnexpectedAtrributeException(Name, escapedAttrName);

            var escapedAttrValue = SecurityElement.Escape(attributeValue);

            attributes[escapedAttrName] = escapedAttrValue;
            return this;
        }

        /// <summary>
        /// Removes attribute from <see cref="Attributes"/> by given attribute key.
        /// </summary>
        /// <param name="attributeName">Name to remove attribute by.</param>
        /// <param name="ignoreCase">Indicates if matching attributes against <paramref name="attributeName"/> should be case-sensitive.</param>
        /// <returns>Current node.</returns>
        public Fb2Node RemoveAttribute(string attributeName, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException(nameof(attributeName));

            if (!attributes.Any())
                return this;

            var attributeKeysToDelete = attributes.Keys
                .Where(key => ignoreCase ?
                    key.EqualsInvariant(attributeName) :
                    key.Equals(attributeName));

            foreach (var attrKey in attributeKeysToDelete)
                attributes.Remove(attrKey);

            return this;
        }

        /// <summary>
        /// Removes attributes matching given predicate.
        /// </summary>
        /// <param name="attributePredicate">Predicate function to match attributes against.</param>
        /// <returns>Current node.</returns>
        public Fb2Node RemoveAttribute(Func<KeyValuePair<string, string>, bool> attributePredicate)
        {
            if (attributePredicate == null)
                throw new ArgumentNullException(nameof(attributePredicate));

            if (!attributes.Any())
                return this;

            var attrsToRemove = attributes.Where(attributePredicate);

            foreach (var attributeToRemove in attrsToRemove)
                attributes.Remove(attributeToRemove.Key);

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

        private static bool TryGetXNodeAttributes([In] XNode node, out Dictionary<string, string> result)
        {
            if (node is not XElement element)
            {
                result = new(0);
                return false;
            }

            var actualAttrs = element.Attributes();
            if (!actualAttrs.Any())
            {
                result = new(0);
                return false;
            }

            result = actualAttrs.ToDictionary(attr => attr.Name.LocalName, attr => attr.Value);
            return true;
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

            if (other is not Fb2Node otherNode)
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

        private bool AreAttributesEqual(Dictionary<string, string> otherAttributes)
        {
            if (ReferenceEquals(attributes, otherAttributes))
                return true;

            var sameAttrs = attributes.Count == otherAttributes.Count &&
                attributes.Keys.All(k => otherAttributes.ContainsKey(k) &&
                attributes[k].Equals(otherAttributes[k], StringComparison.InvariantCulture));

            return sameAttrs;
        }

        public override int GetHashCode() => HashCode.Combine(Name, attributes, AllowedAttributes, IsInline, IsUnsafe);

        public virtual object Clone()
        {
            var node = Fb2NodeFactory.GetNodeByName(Name);

            // TODO : chech if it saves references to keyValuePair's strings
            // if it does, it's bad ))
            if (attributes.Any())
                node.attributes = new Dictionary<string, string>(attributes);

            node.IsInline = IsInline;
            node.IsUnsafe = IsUnsafe;

            return node;
        }
    }
}

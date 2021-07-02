using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Fb2.Document.Exceptions;
using Fb2.Document.Extensions;
using Fb2.Document.Factories;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Base class - describes basic node of fb2 document.
    /// Has Name, list of valid attributes and actual attribute values
    /// </summary>
    public abstract class Fb2Node : ICloneable
    {
        protected static readonly Regex trimWhitespace = new Regex(@"\s+", RegexOptions.Multiline);

        private Dictionary<string, string> attributes = new Dictionary<string, string>();

        /// <summary>
        /// Node name, used during document parsing and validation.
        /// </summary>
        public abstract string Name { get; }

        // /// <summary>
        // /// Gets actual element attributes in key - value (Dictionary) form.
        // /// </summary>
        // public Dictionary<string, string> Attributes { get; private set; }

        // TODO: check for reference-safety
        public ImmutableDictionary<string, string> Attributes => attributes.ToImmutableDictionary();

        /// <summary>
        /// List of allowed attribures for particular element.
        /// </summary>
        public virtual ImmutableHashSet<string> AllowedAttributes => ImmutableHashSet.Create<string>();

        /// <summary>
        /// Indicates if element sholud be inline or start from new line.
        /// </summary>
        public abstract bool IsInline { get; protected set; }

        /// <summary>
        /// Indicates if element is on a right place (has correct parent node etc.) accordingly to Fb2 standard.
        /// </summary>
        public bool Unsafe { get; internal set; }

        /// <summary>
        /// Basic Load of element - validation and populating Attributes.
        /// </summary>
        /// <param name="node">XNode to load as Fb2Node</param>
        public virtual void Load([In] XNode node, bool preserveWhitespace = false)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            node.Validate(Name);

            if (!AllowedAttributes.Any())
                return;

            if (!TryGetAttributesInternal(node, out Dictionary<string, string> actualAttributes))
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

            return attributes.Any(attr => ignoreCase ? attr.Key.EqualsInvariant(key) : attr.Key.Equals(key));
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

            var attribute = attributes.FirstOrDefault(attr => ignoreCase ? attr.Key.EqualsInvariant(key) : attr.Key.Equals(key));

            return attribute;
        }

        /// <summary>
        /// Looks for attribute by given `attributeName`
        /// </summary>
        /// <param name="key">Key to search attribute by</param>
        /// <param name="ignoreCase">true to ignore case; false to consider case in key comparison</param>
        /// <param name="result">Attribute value if any found, otherwise default. </param>
        /// <returns>True if attribute found by given key, otherwise false</returns>
        public bool TryGetAttribute(string key, bool ignoreCase, out KeyValuePair<string, string> result)
        {
            var attribute = GetAttribute(key, ignoreCase);

            result = attribute;
            return !string.IsNullOrWhiteSpace(attribute.Key) && !string.IsNullOrWhiteSpace(attribute.Value);
        }

        #endregion

        #region Node editing

        public Fb2Node AddAttributes(params KeyValuePair<string, string>[] attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes));

            foreach (var attribute in attributes)
                AddAttribute(attribute.Key, attribute.Value);

            return this;
        }

        public Fb2Node AddAttributes(IDictionary<string, string> attributes)
        {
            if (attributes == null || !attributes.Any())
                throw new ArgumentNullException(nameof(attributes), $"{nameof(attributes)} is null or empty dictionary.");

            foreach (var attribute in attributes)
                AddAttribute(attribute.Key, attribute.Value);

            return this;
        }

        public async Task<Fb2Node> AddAttributeAsync(Func<Task<KeyValuePair<string, string>>> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = await attributeProvider();

            return AddAttribute(attribute.Key, attribute.Value);
        }

        public Fb2Node AddAttribute(Func<KeyValuePair<string, string>> attributeProvider)
        {
            if (attributeProvider == null)
                throw new ArgumentNullException(nameof(attributeProvider));

            var attribute = attributeProvider();

            return AddAttribute(attribute.Key, attribute.Value);
        }

        public Fb2Node AddAttribute(KeyValuePair<string, string> attribute) =>
            AddAttribute(attribute.Key, attribute.Value);

        public Fb2Node AddAttribute(string attributeName, string attributeValue)
        {
            if (!AllowedAttributes.Any())
                throw new NoAttributesAllowedException($"{Name} has no allowed attributes.");

            if (string.IsNullOrWhiteSpace(attributeName) ||
                trimWhitespace.IsMatch(attributeName))
                throw new InvalidAttributeException($"{nameof(attributeName)} is null or empty string, or contains invalid characters.");

            if (string.IsNullOrWhiteSpace(attributeValue) ||
                trimWhitespace.IsMatch(attributeValue))
                throw new InvalidAttributeException($"{nameof(attributeValue)} is null or empty string, or contains invalid characters.");

            var escapedAttrName = SecurityElement.Escape(attributeName);

            if (!AllowedAttributes.Contains(escapedAttrName))
                throw new UnexpectedAtrributeException($"Attribute {attributeName} is not allowed for {Name} node.");

            var escapedAttrValue = SecurityElement.Escape(attributeValue);

            attributes[attributeName] = escapedAttrValue;
            return this;
        }

        public Fb2Node RemoveAttribute(string attributeName, bool ignoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException(nameof(attributeName));

            if (!TryGetAttribute(attributeName, ignoreCase, out var result))
                return this;

            attributes.Remove(result.Key);
            return this;
        }

        public Fb2Node RemoveAttribute(Func<KeyValuePair<string, string>, bool> attributePredicate)
        {
            if (attributePredicate == null)
                throw new ArgumentNullException(nameof(attributePredicate));

            if (!attributes.Any())
                return this;

            var attrsToRemove = attributes.Where(a => attributePredicate(a));

            foreach (var attributeToRemove in attrsToRemove)
                attributes.Remove(attributeToRemove.Key);

            return this;
        }

        public Fb2Node ClearAttributes()
        {
            if (attributes.Any())
                attributes.Clear();

            return this;
        }

        #endregion

        private static bool TryGetAttributesInternal([In] XNode node, out Dictionary<string, string> result)
        {
            var element = node as XElement;

            if (element == null || !element.Attributes().Any())
            {
                result = null;
                return false;
            }

            result = element.Attributes()
                            .ToDictionary(attr => attr.Name.LocalName, attr => attr.Value);
            return true;
        }

        public override bool Equals(object other)
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
                        Unsafe == otherNode.Unsafe;

            return result;
        }

        private bool AreAttributesEqual(Dictionary<string, string> otherAttributes)
        {
            if (ReferenceEquals(attributes, otherAttributes))
                return true;

            var sameAttrs = attributes.Count == otherAttributes.Count &&
                attributes.Keys.All(k => otherAttributes.ContainsKey(k) &&
                string.Equals(attributes[k], otherAttributes[k], StringComparison.InvariantCultureIgnoreCase));

            return sameAttrs;
        }

        public override int GetHashCode() => HashCode.Combine(Name, attributes, AllowedAttributes, IsInline, Unsafe);

        public virtual object Clone()
        {
            var node = Fb2ElementFactory.GetNodeByName(Name);

            // TODO : chech if it saves references to keyValuePair's strings
            // if it does, it's bad ))
            if (attributes.Any())
                node.attributes = new Dictionary<string, string>(attributes);

            node.IsInline = IsInline;
            node.Unsafe = Unsafe;

            return node;
        }
    }
}

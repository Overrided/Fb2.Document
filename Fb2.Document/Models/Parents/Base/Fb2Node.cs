using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Fb2.Document.Extensions;

namespace Fb2.Document.Models.Base
{
    /// <summary>
    /// Base class - describes basic node of fb2 document.
    /// Has Name, list of valid attributes and actual attribute values
    /// </summary>
    public abstract class Fb2Node
    {
        /// <summary>
        /// Node name, used during document parsing and validation
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets actual element attributes in key - value (Dictionary) form
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// List of allowed attribures for particular element
        /// </summary>
        public virtual HashSet<string> AllowedAttributes => null;

        /// <summary>
        /// Indicates if element sholud be inline or start from new line
        /// </summary>
        public abstract bool IsInline { get; protected set; }

        /// <summary>
        /// Indicates if element is on a right place (has correct parent node etc.) accordingly to Fb2 standard
        /// </summary>
        public bool Unsafe { get; internal set; }

        /// <summary>
        /// Basic Load of element - validation and populating Attributes
        /// </summary>
        /// <param name="node">XNode to load as Fb2Node</param>
        public virtual void Load([In] XNode node, bool preserveWhitespace = false)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)} is null!");

            node.Validate(Name);

            if (AllowedAttributes == null || !AllowedAttributes.Any())
                return;

            if (!node.GetAllAttributesOrDefault(out Dictionary<string, string> actualAttributes))
                return;

            var filteredAttributes = actualAttributes
                .Where(kvp => AllowedAttributes.Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase));

            if (!filteredAttributes.Any())
                return;

            if (Attributes == null)
                Attributes = new Dictionary<string, string>();

            foreach (var kvp in filteredAttributes)
                Attributes.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Basic method to serialize Fb2Node back to XElement
        /// </summary>
        /// <returns>XElement instance with attributes reflecting Attributes property </returns>
        public virtual XElement ToXml()
        {
            XElement element = null;

            if (Attributes != null && Attributes.Any())
                element = new XElement(Name, Attributes.Select(attr => new XAttribute(attr.Key, attr.Value)));
            else
                element = new XElement(Name);

            return element;
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
                throw new ArgumentNullException($"{nameof(key)} is null or empty string.");

            if (Attributes == null || !Attributes.Any())
                return false;

            return Attributes.Any(attr => ignoreCase ? attr.Key.EqualsInvariant(key) : attr.Key.Equals(key));
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

            var attribute = Attributes.FirstOrDefault(attr => ignoreCase ? attr.Key.EqualsInvariant(key) : attr.Key.Equals(key));

            return attribute;
        }

        /// <summary>
        /// Returns true if attribute found by given key, otherwise false.
        /// If none attribute found, result contains default value.
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
    }
}

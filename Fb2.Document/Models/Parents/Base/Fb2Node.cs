using System;
using System.Collections.Generic;
using System.Linq;
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
        public virtual void Load(XNode node, bool preserveWhitespace = false)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)} is null!");

            node.Validate(this.Name);

            if (this.AllowedAttributes == null || !this.AllowedAttributes.Any())
                return;

            if (!node.GetAllAttributesOrDefault(out Dictionary<string, string> actualAttributes))
                return;

            var filteredAttributes = actualAttributes
                .Where(kvp => this.AllowedAttributes
                                  .Contains(kvp.Key, StringComparer.InvariantCultureIgnoreCase));

            if (!filteredAttributes.Any())
                return;

            if (this.Attributes == null)
                this.Attributes = new Dictionary<string, string>();

            foreach (var kvp in filteredAttributes)
            {
                this.Attributes.Add(kvp.Key, kvp.Value);
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions
{
    /// <summary>
    /// Implements "type accurate" extensions for any `Fb2Node` descendant - any Fb2Node there is, literally.
    /// </summary>
    public static class Fb2NodeExtensions
    {
        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributes(KeyValuePair{string, string}[])"/> method.
        /// <para>Adds set of attributes to node using params <seealso cref="KeyValuePair{string,string}"/></para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributes">Set of attributes to add to given <paramref name="fb2Node"/>.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T AppendAttributes<T>(
            this T fb2Node,
            params KeyValuePair<string, string>[] attributes) where T : Fb2Node =>
            (T)fb2Node.AddAttributes(attributes);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributes(IDictionary{string, string})"/> method.
        /// <para> Adds multiple attributes using <seealso cref="IDictionary{string, string}" />.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributes">Set of attributes to add to given <paramref name="fb2Node"/>.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T AppendAttributes<T>(
            this T fb2Node,
            IDictionary<string, string> attributes) where T : Fb2Node =>
            (T)fb2Node.AddAttributes(attributes);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributeAsync(Func{Task{KeyValuePair{string, string}}})"/> method.
        /// <para> Adds single attribute to <see cref="Attributes"/> using asynchronous provider function.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributeProvider">Asynchronous attribute provider function.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static async Task<T> AppendAttributeAsync<T>(
            this T fb2Node,
            Func<Task<KeyValuePair<string, string>>> attributeProvider) where T : Fb2Node
        {
            var result = await fb2Node.AddAttributeAsync(attributeProvider);
            return (T)result;
        }

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(Func{KeyValuePair{string, string}})"/> method.
        /// <para> Adds single attribute to <see cref="Fb2Node.Attributes"/> using provider function.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributeProvider">Attribute provider function.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T AppendAttribute<T>(
            this T fb2Node,
            Func<KeyValuePair<string, string>> attributeProvider) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attributeProvider);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(KeyValuePair{string, string})"/> method.
        /// <para> Adds single attribute to <see cref="Attributes"/>.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attribute">Attribute to add to <paramref name="fb2Node"/>.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T AppendAttribute<T>(this T fb2Node, KeyValuePair<string, string> attribute) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attribute);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(string, string)"/> method.
        /// <para> Adds single attribute using separate key and value.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="attributeValue">Attribute value.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T AppendAttribute<T>(
            this T fb2Node,
            string attributeName,
            string attributeValue) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attributeName, attributeValue);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.RemoveAttribute(string, bool)"/> method.
        /// <para> Removes attribute from <see cref="Attributes"/> by given attribute key.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributeName">Attribute name.</param>
        /// <param name="ignoreCase">Indicates if matching attributes against <paramref name="attributeName"/> should be case-sensitive.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T DeleteAttribute<T>(
            this T fb2Node,
            string attributeName,
            bool ignoreCase = false) where T : Fb2Node =>
            (T)fb2Node.RemoveAttribute(attributeName, ignoreCase);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.RemoveAttribute(Func{KeyValuePair{string, string}, bool})"/> method.
        /// <para> Removes attributes matching given predicate.</para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <param name="attributePredicate">Predicate function to match attributes against.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T DeleteAttribute<T>(
            this T fb2Node,
            Func<KeyValuePair<string, string>, bool> attributePredicate) where T : Fb2Node =>
            (T)fb2Node.RemoveAttribute(attributePredicate);

        /// <summary>
        /// "Type-accurate" wrapper for <see cref="Fb2Node.ClearAttributes()"/> method.
        /// <para> Erases all attributes of given <paramref name="fb2Node"/></para>
        /// </summary>
        /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
        /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
        /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
        public static T EraseAttributes<T>(this T fb2Node) where T : Fb2Node =>
            (T)fb2Node.ClearAttributes();
    }
}

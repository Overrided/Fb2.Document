using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions;

/// <summary>
/// Implements "type accurate" extensions for any `Fb2Node` descendant - any Fb2Node there is, literally.
/// </summary>
public static class Fb2NodeExtensions
{
    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributes(Fb2Attribute[])"/> method.
    /// <para> Adds multiple attributes to <see cref="Fb2Node.Attributes"/> using <seealso cref="params Fb2Attribute[]"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attributes">Set of attributes to add to given <paramref name="fb2Node"/>.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T AppendAttributes<T>(this T fb2Node, params Fb2Attribute[] attributes) where T : Fb2Node =>
        (T)fb2Node.AddAttributes(attributes);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributes(IEnumerable{Fb2Attribute})"/> method.
    /// <para> Adds multiple attributes to <see cref="Fb2Node.Attributes"/> using <seealso cref="IEnumerable{Fb2Attribute}."/></para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attributes">Set of attributes to add to given <paramref name="fb2Node"/>.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T AppendAttributes<T>(this T fb2Node, IEnumerable<Fb2Attribute> attributes) where T : Fb2Node =>
        (T)fb2Node.AddAttributes(attributes);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttributeAsync(Func{Task{Fb2Attribute}})"/> method.
    /// <para> Adds single attribute to <see cref="Fb2Node.Attributes"/> using asynchronous <paramref name="attributeProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attributeProvider">Asynchronous attribute provider function.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static async Task<T> AppendAttributeAsync<T>(
        this T fb2Node,
        Func<Task<Fb2Attribute>> attributeProvider) where T : Fb2Node
    {
        var result = await fb2Node.AddAttributeAsync(attributeProvider);
        return (T)result;
    }

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(Func{Fb2Attribute})"/> method.
    /// <para> Adds single attribute to <see cref="Fb2Node.Attributes"/> using <paramref name="attributeProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attributeProvider">Attribute provider function.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T AppendAttribute<T>(this T fb2Node, Func<Fb2Attribute> attributeProvider) where T : Fb2Node =>
        (T)fb2Node.AddAttribute(attributeProvider);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(Fb2Attribute)"/> method.
    /// <para> Adds single attribute to <see cref="Fb2Node.Attributes"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attribute">Attribute to add to <paramref name="fb2Node"/>.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T AppendAttribute<T>(this T fb2Node, Fb2Attribute attribute) where T : Fb2Node =>
        (T)fb2Node.AddAttribute(attribute);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.AddAttribute(string, string, string?)"/> method.
    /// <para> Adds single attribute to <see cref="Fb2Node.Attributes"/>.</para> 
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="key">Attribute key to add.</param>
    /// <param name="value">Attribute value to add.</param>
    /// <param name="namespaceName">
    /// <para>Optional, can be <see langword="null"/>.</para>
    /// <para>NamespaceName for attribute, used by <see cref="ToXml"/> serialization.</para>
    /// </param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T AppendAttribute<T>(this T fb2Node, string key, string value, string? namespaceName = null) where T : Fb2Node =>
        (T)fb2Node.AddAttribute(key, value, namespaceName);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.RemoveAttribute(string, bool)"/> method.
    /// <para> Removes attribute from <see cref="Fb2Node.Attributes"/> by given attribute key.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="key">Attribute name.</param>
    /// <param name="ignoreCase">Indicates if matching attributes against <paramref name="key"/> should be case-sensitive.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T DeleteAttribute<T>(
        this T fb2Node,
        string key,
        bool ignoreCase = false) where T : Fb2Node => (T)fb2Node.RemoveAttribute(key, ignoreCase);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.RemoveAttribute(Func{Fb2Attribute, bool})"/> method.
    /// <para> Removes attributes matching given predicate from <see cref="Fb2Node.Attributes"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attributePredicate">Predicate function to match attributes against.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T DeleteAttribute<T>(
        this T fb2Node,
        Func<Fb2Attribute, bool> attributePredicate) where T : Fb2Node => (T)fb2Node.RemoveAttribute(attributePredicate);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.RemoveAttribute(Fb2Attribute)"/> method.
    /// <para> Removes given <paramref name="attribute"/> from <see cref="Fb2Node.Attributes"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <param name="attribute">Attribute to remove.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T DeleteAttribute<T>(
        this T fb2Node,
        Fb2Attribute attribute) where T : Fb2Node => (T)fb2Node.RemoveAttribute(attribute);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Node.ClearAttributes()"/> method.
    /// <para> Erases all attributes of given <paramref name="fb2Node"/></para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Node">Fb2Node instance to use extension on.</param>
    /// <returns><paramref name="fb2Node"/> with it's original type.</returns>
    public static T EraseAttributes<T>(this T fb2Node) where T : Fb2Node => (T)fb2Node.ClearAttributes();
}

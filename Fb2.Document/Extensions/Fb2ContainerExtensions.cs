using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions;

/// <summary>
/// Implements "type accurate" extensions for any <see cref="Fb2Container"/> class descendant.
/// </summary>
public static class Fb2ContainerExtensions
{
    /// <summary>
    /// <para> 
    /// This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.
    /// </para>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContentAsync(Func{Task{Fb2Node}})"/> method.
    /// <para> Adds node to given <paramref name="fb2Container"/> using async <paramref name="nodeProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodeProvider">Async provider function for child node.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    [Obsolete("This extension method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.")]
    public static async Task<T> AppendContentAsync<T>(
        this T fb2Container,
        Func<Task<Fb2Node>> nodeProvider) where T : Fb2Container
    {
        var result = await fb2Container.AddContentAsync(nodeProvider);
        return (T)result;
    }

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContentAsync(Func{CancellationToken, Task{Fb2Node}}, CancellationToken)"/> method.
    /// <para> Adds node to given <paramref name="fb2Container"/> using async <paramref name="nodeProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodeProvider">Async provider function for child node.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static async Task<T> AppendContentAsync<T>(
        this T fb2Container,
        Func<CancellationToken, Task<Fb2Node>> nodeProvider,
        CancellationToken cancellationToken = default) where T : Fb2Container
    {
        var result = await fb2Container.AddContentAsync(nodeProvider, cancellationToken);
        return (T)result;
    }

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContent(Func{Fb2Node})"/> method.
    /// <para> Adds node to given <paramref name="fb2Container"/> using <paramref name="nodeProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodeProvider">Provider function for child node.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendContent<T>(this T fb2Container, Func<Fb2Node> nodeProvider) where T : Fb2Container =>
        (T)fb2Container.AddContent(nodeProvider);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContent(Fb2Node[])"/> method.
    /// <para> Adds <paramref name="nodes"/> to given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodes">Set of nodes to add to given <paramref name="fb2Container"/>.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendContent<T>(this T fb2Container, params List<Fb2Node> nodes) where T : Fb2Container =>
        (T)fb2Container.AddContent(nodes);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddTextContent(string, string?)"/> method.
    /// <para> Adds new text node to given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="content">String content to add to given <paramref name="fb2Container"/>.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendTextContent<T>(
        this T fb2Container,
        string content,
        string? separator = null) where T : Fb2Container => (T)fb2Container.AddTextContent(content, separator);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddTextContent(Func{string}, string?)"/> method.
    /// <para> Adds new text node to given <paramref name="fb2Container"/> using content provider function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="contentProvider">Content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendTextContent<T>(
        this T fb2Container,
        Func<string> contentProvider,
        string? separator = null) where T : Fb2Container =>
            (T)fb2Container.AddTextContent(contentProvider, separator);

    /// <summary>
    /// <para> 
    /// This method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.
    /// </para>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddTextContentAsync(Func{Task{string}}, string?)"/> method.
    /// <para> Adds new text node to given <paramref name="fb2Container"/> using async content provider function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="contentProvider">Async content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    [Obsolete("This extension method is obsolete and will be removed in next release. Please use new implementation that supports cancellation.")]
    public static async Task<T> AppendTextContentAsync<T>(
        this T fb2Container,
        Func<Task<string>> contentProvider,
        string? separator = null) where T : Fb2Container =>
            (T)(await fb2Container.AddTextContentAsync(contentProvider, separator));

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddTextContentAsync(Func{CancellationToken, Task{string}}, string?, CancellationToken)"/> method.
    /// <para> Adds new text node to given <paramref name="fb2Container"/> using async content provider function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="contentProvider">Async content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static async Task<T> AppendTextContentAsync<T>(
       this T fb2Container,
       Func<CancellationToken, Task<string>> contentProvider,
       string? separator = null,
       CancellationToken cancellationToken = default) where T : Fb2Container =>
           (T)(await fb2Container.AddTextContentAsync(contentProvider, separator, cancellationToken));

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContent(IEnumerable{Fb2Node})"/> method.
    /// <para> Adds <paramref name="nodes"/> to given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodes">Set of nodes to add to given <paramref name="fb2Container"/>.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendContent<T>(this T fb2Container, IEnumerable<Fb2Node> nodes) where T : Fb2Container =>
        (T)fb2Container.AddContent(nodes);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContent(string)"/> method.
    /// <para> Adds new node to given <paramref name="fb2Container"/> using <paramref name="nodeName"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodeName">Name to add node by to given <paramref name="fb2Container"/>.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendContent<T>(this T fb2Container, string nodeName) where T : Fb2Container =>
        (T)fb2Container.AddContent(nodeName);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.AddContent(Fb2Node)"/> method.
    /// <para> Adds <paramref name="node"/> to given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="node">Node to add to given <paramref name="fb2Container"/>.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exceptions.InvalidNodeException"></exception>
    /// <exception cref="Exceptions.UnexpectedNodeException"></exception>
    public static T AppendContent<T>(this T fb2Container, Fb2Node node) where T : Fb2Container =>
        (T)fb2Container.AddContent(node);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.RemoveContent(IEnumerable{Fb2Node})"/> method.
    /// <para> Removes set of <paramref name="nodes"/> from given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodes">Set of nodes to remove from given <paramref name="fb2Container"/>.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T DeleteContent<T>(this T fb2Container, IEnumerable<Fb2Node> nodes) where T : Fb2Container =>
        (T)fb2Container.RemoveContent(nodes);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.RemoveContent(Func{Fb2Node, bool})"/> method.
    /// <para> Removes matching nodes from given <paramref name="fb2Container"/> by <paramref name="nodePredicate"/>. </para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="nodePredicate"></param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T DeleteContent<T>(this T fb2Container, Func<Fb2Node, bool> nodePredicate) where T : Fb2Container =>
        (T)fb2Container.RemoveContent(nodePredicate);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.RemoveContent(Fb2Node)"/> method.
    /// <para> Removes particular <paramref name="node"/> from given <paramref name="fb2Container"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <param name="node"></param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T DeleteContent<T>(this T fb2Container, Fb2Node node) where T : Fb2Container =>
        (T)fb2Container.RemoveContent(node);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Container.ClearContent"/> method.
    /// <para> Erases all content from given <paramref name="fb2Container"/> node.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Container"><see cref="Fb2Container"/> node instance to use extension on.</param>
    /// <returns><paramref name="fb2Container"/> with it's original type.</returns>
    public static T EraseContent<T>(this T fb2Container) where T : Fb2Container =>
        (T)fb2Container.ClearContent();
}

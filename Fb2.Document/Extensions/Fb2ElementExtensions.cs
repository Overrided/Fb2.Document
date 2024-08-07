﻿using System;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions;

/// <summary>
/// Implements "type accurate" extensions for any `Fb2Element` descendant.
/// </summary>
public static class Fb2ElementExtensions
{
    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Element.AddContent(string, string?)"/> method.
    /// <para> Appends new plain text to given <paramref name="fb2Element"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Element"><see cref="Fb2Element"/> node instance to use extension on.</param>
    /// <param name="newContent">Plain text to append to given <paramref name="fb2Element"/>.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Element"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T AppendContent<T>(
        this T fb2Element,
        string newContent,
        string? separator = null) where T : Fb2Element => (T)fb2Element.AddContent(newContent, separator);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Element.AddContent(Func{string}, string?)"/> method.
    /// <para> Appends new plain text to given <paramref name="fb2Element"/> using <paramref name="contentProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Element"><see cref="Fb2Element"/> node instance to use extension on.</param>
    /// <param name="contentProvider">Content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Element"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T AppendContent<T>(
        this T fb2Element,
        Func<string> contentProvider,
        string? separator = null) where T : Fb2Element => (T)fb2Element.AddContent(contentProvider, separator);

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Element.AddContentAsync(Func{Task{string}}, string?)"/> method.
    /// <para> Appends new plain text to given <paramref name="fb2Element"/> using async <paramref name="contentProvider"/> function.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Element"><see cref="Fb2Element"/> node instance to use extension on.</param>
    /// <param name="contentProvider">Asynchronous content provider function.</param>
    /// <param name="separator">Separator string used to join new text with existing content.</param>
    /// <returns><paramref name="fb2Element"/> with it's original type.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<T> AppendContentAsync<T>(
        this T fb2Element,
        Func<Task<string>> contentProvider,
        string? separator = null) where T : Fb2Element
    {
        var result = await fb2Element.AddContentAsync(contentProvider, separator);
        return (T)result;
    }

    /// <summary>
    /// "Type-accurate" wrapper for <see cref="Fb2Element.ClearContent()"/> method.
    /// <para> Clears content of given <paramref name="fb2Element"/>.</para>
    /// </summary>
    /// <typeparam name="T">Type of node, inferred from usage implicitly.</typeparam>
    /// <param name="fb2Element"><see cref="Fb2Element"/> node instance to use extension on.</param>
    /// <returns><paramref name="fb2Element"/> with it's original type.</returns>
    public static T EraseContent<T>(this T fb2Element) where T : Fb2Element => (T)fb2Element.ClearContent();
}

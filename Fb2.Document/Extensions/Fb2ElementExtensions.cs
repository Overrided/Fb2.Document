using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions
{
    // "type accurate" extensions
    // returns original type of child node instead of basic Fb2Element
    public static class Fb2ElementExtensions
    {
        public static T WithContent<T>(this T fb2Element,
            string newContent,
            string? separator = null) where T : Fb2Element => (T)fb2Element.AddContent(newContent, separator);

        public static T WithContent<T>(this T fb2Element,
            Func<string> contentProvider,
            string? separator = null) where T : Fb2Element => (T)fb2Element.AddContent(contentProvider, separator);
    }
}

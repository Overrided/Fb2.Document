using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions
{
    // "strong typed" extensions
    // returns original type of child node instead of basic Fb2Container
    // TODO : add tests?
    public static class Fb2ContainerExtensions
    {
        public static Task<T> WithContentAsync<T>(this T fb2Container, Func<Task<Fb2Node>> nodeProvider) where T : Fb2Container =>
            fb2Container.AddContentAsync(nodeProvider) as Task<T>;

        public static T WithContent<T>(this T fb2Container, Func<Fb2Node> nodeProvider) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodeProvider);

        public static T WithContent<T>(this T fb2Container, params Fb2Node[] nodes) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodes);

        public static T WithTextContent<T>(this T fb2Container,
            string content,
            string separator = null,
            bool preserveWhitespace = false) where T : Fb2Container =>
            (T)fb2Container.AddTextContent(content, separator, preserveWhitespace);

        public static T WithContent<T>(this T fb2Container, IEnumerable<Fb2Node> nodes) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodes);

        public static T WithContent<T>(this T fb2Container, string nodeName) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodeName);

        public static T WithContent<T>(this T fb2Container, Fb2Node node) where T : Fb2Container =>
            (T)fb2Container.AddContent(node);
    }
}

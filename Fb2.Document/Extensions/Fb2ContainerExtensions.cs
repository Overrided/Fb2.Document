using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions
{
    // "type accurate" extensions
    // returns original type of child node instead of basic Fb2Container
    public static class Fb2ContainerExtensions
    {
        public static async Task<T> AppendContentAsync<T>(this T fb2Container, Func<Task<Fb2Node>> nodeProvider) where T : Fb2Container
        {
            var result = await fb2Container.AddContentAsync(nodeProvider);
            return (T)result;
        }

        public static T AppendContent<T>(this T fb2Container, Func<Fb2Node> nodeProvider) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodeProvider);

        public static T AppendContent<T>(this T fb2Container, params Fb2Node[] nodes) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodes);

        public static T AppendTextContent<T>(this T fb2Container,
            string content,
            string? separator = null) where T : Fb2Container =>
            (T)fb2Container.AddTextContent(content, separator);

        public static T AppendContent<T>(this T fb2Container, IEnumerable<Fb2Node> nodes) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodes);

        public static T AppendContent<T>(this T fb2Container, string nodeName) where T : Fb2Container =>
            (T)fb2Container.AddContent(nodeName);

        public static T AppendContent<T>(this T fb2Container, Fb2Node node) where T : Fb2Container =>
            (T)fb2Container.AddContent(node);

        public static T DeleteContent<T>(this T fb2Container, IEnumerable<Fb2Node> nodes) where T : Fb2Container =>
            (T)fb2Container.RemoveContent(nodes);

        public static T DeleteContent<T>(this T fb2Container, Func<Fb2Node, bool> nodePredicate) where T : Fb2Container =>
            (T)fb2Container.RemoveContent(nodePredicate);

        public static T DeleteContent<T>(this T fb2Container, Fb2Node node) where T : Fb2Container =>
            (T)fb2Container.RemoveContent(node);

        public static T EraseContent<T>(this T fb2Container) where T : Fb2Container =>
            (T)fb2Container.ClearContent();
    }
}

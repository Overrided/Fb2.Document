﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Extensions
{
    // "type accurate" extensions
    // returns original type of child node instead of basic Fb2Node
    public static class Fb2NodeExtensions
    {
        public static T WithAttributes<T>(
            this T fb2Node,
            params KeyValuePair<string, string>[] attributes) where T : Fb2Node =>
            (T)fb2Node.AddAttributes(attributes);

        public static T WithAttributes<T>(
            this T fb2Node,
            IDictionary<string, string> attributes) where T : Fb2Node =>
            (T)fb2Node.AddAttributes(attributes);

        public static Task<T> WithAttributeAsync<T>(
            this T fb2Node,
            Func<Task<KeyValuePair<string, string>>> attributeProvider) where T : Fb2Node =>
            fb2Node.AddAttributeAsync(attributeProvider) as Task<T>;

        public static T WithAttribute<T>(
            this T fb2Node,
            Func<KeyValuePair<string, string>> attributeProvider) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attributeProvider);

        public static T WithAttribute<T>(this T fb2Node, KeyValuePair<string, string> attribute) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attribute);

        public static T WithAttribute<T>(
            this T fb2Node,
            string attributeName,
            string attributeValue) where T : Fb2Node =>
            (T)fb2Node.AddAttribute(attributeName, attributeValue);
    }
}
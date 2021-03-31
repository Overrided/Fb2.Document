using System;

namespace Fb2.Document.Extensions
{
    internal static class StringExtensions
    {
        public static bool EqualsInvariant(this string left, string right) =>
            left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
    }
}

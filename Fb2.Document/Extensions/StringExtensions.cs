using System;

namespace Fb2.Document.Extensions;

internal static class StringExtensions
{
    internal static bool EqualsIgnoreCase(this string left, string right) =>
        left.Equals(right, StringComparison.InvariantCultureIgnoreCase);
}

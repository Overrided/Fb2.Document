using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Resolver;

internal static class PredicateResolver
{
    private static Func<Fb2Node, bool> GetAbstractClassPredicate(Type targetType)
        => element => element.GetType().IsSubclassOf(targetType);

    private static Func<Fb2Node, bool> GetClassPredicate(Type targetType)
        => element => element.GetType().Equals(targetType);

    public static Func<Fb2Node, bool> GetPredicate<T>() where T : Fb2Node
    {
        var targetType = typeof(T);

        if (targetType.IsAbstract)
            return GetAbstractClassPredicate(targetType);

        return GetClassPredicate(targetType);
    }
}

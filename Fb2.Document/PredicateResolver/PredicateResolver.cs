using System;
using Fb2.Document.Models.Base;

namespace Fb2.Document.Resolver
{
    internal class PredicateResolver
    {
        private static PredicateResolver instance = null;

        private PredicateResolver() { }

        public static PredicateResolver Instance
        {
            get
            {
                if (instance == null)
                    instance = new PredicateResolver();

                return instance;
            }
        }

        private Func<Fb2Node, bool> GetAbstractClassPredicate(Type targetType)
            => element => element.GetType().IsSubclassOf(targetType);

        private Func<Fb2Node, bool> GetClassPredicate(Type targetType)
            => element => element.GetType().Equals(targetType);

        public Func<Fb2Node, bool> GetPredicate<T>() where T : Fb2Node
        {
            var targetType = typeof(T);

            if (targetType.IsAbstract)
                return GetAbstractClassPredicate(targetType);

            return GetClassPredicate(targetType);
        }
    }
}

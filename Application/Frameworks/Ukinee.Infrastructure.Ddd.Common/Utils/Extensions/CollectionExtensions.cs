namespace Ukinee.Infrastructure.Ddd.Common.Utils.Extensions;

public static class AsCollectionExtensions
{
    extension<T>(IEnumerable<T> collection)
    {
        public IReadOnlyCollection<T> AsCollection()
        {
            return collection as IReadOnlyCollection<T> ?? collection.ToList();
        }

        public ILookup<TKey, TElement> ToLookupMany<TKey, TElement>(
            Func<T, IEnumerable<TKey>> keysSelector,
            Func<T, TElement> elementSelector,
            IEqualityComparer<TKey>? comparer = null
        )
        {
            ArgumentNullException.ThrowIfNull(collection);
            ArgumentNullException.ThrowIfNull(keysSelector);
            ArgumentNullException.ThrowIfNull(elementSelector);

            return collection
                .SelectMany(
                    keysSelector,
                    (item, key) => (Key: key, Element: elementSelector(item))
                )
                .ToLookup(x => x.Key, x => x.Element, comparer);
        }

        public ILookup<TKey, T> ToLookupMany<TKey>(
            Func<T, IEnumerable<TKey>> keysSelector,
            IEqualityComparer<TKey>? comparer = null
        )
        {
            return collection.ToLookupMany(keysSelector, item => item, comparer);
        }
    }
}

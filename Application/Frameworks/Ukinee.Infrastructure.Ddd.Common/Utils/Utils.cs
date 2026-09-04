namespace Ukinee.Infrastructure.Ddd.Common.Utils;

internal static class Utils
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public IReadOnlyCollection<T> AsCollection()
        {
            return enumerable as IReadOnlyCollection<T> ?? enumerable.ToList();
        }
    }
}

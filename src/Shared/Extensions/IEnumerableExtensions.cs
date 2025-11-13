namespace Shared.Extensions
{
    public static class IEnumerableExtensions
    {
        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            return source == null ? throw new ArgumentNullException(nameof(source)) : new List<TSource>(source);
        }

        public static ICollection<TSource> ToICollection<TSource>(this IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            ICollection<TSource> data = source.ToList();
            return data;
        }
    }
}

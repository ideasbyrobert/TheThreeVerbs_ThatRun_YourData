using TheaterSales.Extended.MapFilterReduce.Strategies.Basic;

namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

public static class FunctionalSort
{
    public static List<T> SortBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keyExtractor,
        bool descendingOrder = false) where TKey : IComparable<TKey>
    {
        var elements = source.ToList();
        if (elements.Count <= 1) return elements;

        return QuickSortAlgorithm.Sort(elements, keyExtractor, descendingOrder);
    }
}
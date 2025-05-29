namespace FunctionalSQL.Strategy.MapFilterReduce.Infrastructure.Sorting;

public static class FunctionalSort
{
    public static List<T> SortBy<T, TKey>(
        IEnumerable<T> source,
        Func<T, TKey> keyExtractor,
        bool descendingOrder = false) where TKey : IComparable<TKey>
    {
        return descendingOrder 
            ? source.OrderByDescending(keyExtractor).ToList()
            : source.OrderBy(keyExtractor).ToList();
    }
}
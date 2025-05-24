namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

internal static class ElementPartitioner
{
    public static ElementPartitions<T> Partition<T, TKey>(
        List<T> elements,
        Func<T, TKey> keyExtractor,
        TKey pivotKey,
        bool descendingOrder) where TKey : IComparable<TKey>
    {
        var less = new List<T>();
        var equal = new List<T>();
        var greater = new List<T>();

        foreach (var element in elements)
        {
            var comparison = keyExtractor(element).CompareTo(pivotKey);
            
            if (comparison == 0)
                equal.Add(element);
            else if (ShouldGoToLessPartition(comparison, descendingOrder))
                less.Add(element);
            else
                greater.Add(element);
        }

        return new ElementPartitions<T>(less, equal, greater);
    }

    private static bool ShouldGoToLessPartition(int comparison, bool descendingOrder) =>
        descendingOrder ? comparison > 0 : comparison < 0;
}

internal class ElementPartitions<T>
{
    public List<T> Less { get; }
    public List<T> Equal { get; }
    public List<T> Greater { get; }

    public ElementPartitions(List<T> less, List<T> equal, List<T> greater)
    {
        Less = less;
        Equal = equal;
        Greater = greater;
    }
}
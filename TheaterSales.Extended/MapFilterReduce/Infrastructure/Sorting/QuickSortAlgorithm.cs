namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

internal static class QuickSortAlgorithm
{
    public static List<T> Sort<T, TKey>(
        List<T> elements,
        Func<T, TKey> keyExtractor,
        bool descendingOrder) where TKey : IComparable<TKey>
    {
        if (elements.Count <= 1) return elements;

        var pivot = PivotSelector.SelectMiddleElement(elements);
        var pivotKey = keyExtractor(pivot);

        var partitions = ElementPartitioner.Partition(
            elements, keyExtractor, pivotKey, descendingOrder);
        
        var sortedLess = partitions.Less.Count > 0 
            ? Sort(partitions.Less, keyExtractor, descendingOrder) 
            : partitions.Less;
            
        var sortedGreater = partitions.Greater.Count > 0 
            ? Sort(partitions.Greater, keyExtractor, descendingOrder) 
            : partitions.Greater;

        return PartitionCombiner.Combine(sortedLess, partitions.Equal, sortedGreater);
    }
}
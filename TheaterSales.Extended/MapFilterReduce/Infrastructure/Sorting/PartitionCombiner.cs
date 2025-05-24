namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

internal static class PartitionCombiner
{
    public static List<T> Combine<T>(
        List<T> less,
        List<T> equal,
        List<T> greater)
    {
        var totalCapacity = less.Count + equal.Count + greater.Count;
        var result = new List<T>(totalCapacity);
        
        result.AddRange(less);
        result.AddRange(equal);
        result.AddRange(greater);
        
        return result;
    }
}
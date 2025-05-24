namespace TheaterSales.Extended.MapFilterReduce.Strategies.Basic;

public static class FilterOperation
{
    public static IEnumerable<T> Filter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition)
    {
        return source.Where(condition);
    }
}
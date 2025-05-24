namespace TheaterSales.Extended.MapFilterReduce.Strategies.Basic;

public static class MapOperation
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation)
    {
        return source.Select(transformation);
    }
}
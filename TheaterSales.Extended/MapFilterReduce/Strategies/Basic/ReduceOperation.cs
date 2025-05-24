namespace TheaterSales.Extended.MapFilterReduce.Strategies.Basic;

public static class ReduceOperation
{
    public static TAccumulate Reduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner)
    {
        return source.Aggregate(initialValue, combiner);
    }
}
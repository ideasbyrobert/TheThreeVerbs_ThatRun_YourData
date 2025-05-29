namespace TrinitySQL.Strategy.MapFilterReduce.Strategies.Basic;

public static class BasicOperations
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, TResult> transformation) =>
        source.Select(transformation);

    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> source,
        Func<T, bool> condition) =>
        source.Where(condition);

    public static TAccumulate Reduce<TSource, TAccumulate>(
        IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner) =>
        source.Aggregate(initialValue, combiner);
}
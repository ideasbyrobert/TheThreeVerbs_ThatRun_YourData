using TheaterSales.Extended.MapFilterReduce.Strategies.Batch;
using TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;
using TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Composite;

public static class CompositeTransformations
{
    public static IEnumerable<TResult> LazyBatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100)
    {
        return source.BatchMap(transformation, batchSize);
    }

    public static IEnumerable<T> MemoLazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null)
    {
        var keyGenerator = cacheKeyGenerator ?? DefaultKeyGenerator;
        return new MemoizedFilterIterator<T>(source, condition, keyGenerator);
    }

    private static object DefaultKeyGenerator<T>(T item) => item!;
}
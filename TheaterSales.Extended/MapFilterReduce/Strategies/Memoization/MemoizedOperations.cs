using System.Collections.Concurrent;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;

public static class MemoizedOperations
{
    private static readonly ConcurrentDictionary<object, object> Cache = new();

    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        Func<TSource, object>? cacheKeyGenerator = null)
    {
        var keyGenerator = cacheKeyGenerator ?? (item => item!);
        
        foreach (var item in source)
        {
            var cacheKey = CreateCacheKey<TResult>(keyGenerator(item), "map");
            yield return GetOrCompute(cacheKey, () => transformation(item));
        }
    }

    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null)
    {
        var keyGenerator = cacheKeyGenerator ?? (item => item!);
        
        foreach (var item in source)
        {
            var cacheKey = CreateCacheKey<bool>(keyGenerator(item), "filter");
            if (GetOrCompute(cacheKey, () => condition(item)))
                yield return item;
        }
    }

    public static TAccumulate Reduce<TSource, TAccumulate>(
        IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        object? cacheIdentifier = null)
    {
        var cacheKey = CreateCacheKey<TAccumulate>(cacheIdentifier ?? source, "reduce");
        return GetOrCompute(cacheKey, () => source.Aggregate(initialValue, combiner));
    }

    public static void ClearCache() => Cache.Clear();

    private static TResult GetOrCompute<TResult>(object cacheKey, Func<TResult> computation) =>
        (TResult)Cache.GetOrAdd(cacheKey, _ => computation()!);

    private static object CreateCacheKey<T>(object key, string operation) =>
        (typeof(T), key, operation);
}
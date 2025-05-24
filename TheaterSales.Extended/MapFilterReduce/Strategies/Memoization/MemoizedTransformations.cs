using TheaterSales.Extended.MapFilterReduce.Strategies.Basic;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;

public static class MemoizedTransformations
{
    public static IEnumerable<TResult> MemoMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        Func<TSource, object>? cacheKeyGenerator = null)
    {
        var keyGenerator = cacheKeyGenerator ?? DefaultKeyGenerator;
        return MapOperation.Map(source, element => 
            CachedTransformation(element, transformation, keyGenerator));
    }

    private static TResult CachedTransformation<TSource, TResult>(
        TSource element,
        Func<TSource, TResult> transformation,
        Func<TSource, object> keyGenerator)
    {
        var cacheKey = ComputationCache.CreateKey<TResult>(
            keyGenerator(element), "map");
        return ComputationCache.GetOrCompute(cacheKey, 
            () => transformation(element));
    }

    public static IEnumerable<T> MemoFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null)
    {
        var keyGenerator = cacheKeyGenerator ?? DefaultKeyGenerator;
        return FilterOperation.Filter(source, element => 
            CachedCondition(element, condition, keyGenerator));
    }

    private static bool CachedCondition<T>(
        T element,
        Func<T, bool> condition,
        Func<T, object> keyGenerator)
    {
        var cacheKey = ComputationCache.CreateKey<T>(
            keyGenerator(element), "filter");
        return ComputationCache.GetOrCompute(cacheKey, 
            () => condition(element));
    }

    public static TAccumulate MemoReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        object? cacheIdentifier = null)
    {
        var cacheKey = ComputationCache.CreateKey<TAccumulate>(
            cacheIdentifier ?? source, "reduce");
        return ComputationCache.GetOrCompute(cacheKey, 
            () => ReduceOperation.Reduce(source, initialValue, combiner));
    }

    private static object DefaultKeyGenerator<T>(T item) => item!;

    public static void ClearComputationCache() => ComputationCache.Clear();
}
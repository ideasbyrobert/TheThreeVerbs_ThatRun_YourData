using TheaterSales.Extended.MapFilterReduce.Strategies.Basic;
using TheaterSales.Extended.MapFilterReduce.Strategies.Lazy;
using TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;
using TheaterSales.Extended.MapFilterReduce.Strategies.Batch;
using TheaterSales.Extended.MapFilterReduce.Strategies.Composite;
using TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

namespace TheaterSales.Extended;

public static class MapFilterReduceExtensions
{
    #region Core Map-Filter-Reduce Operations

    public static IEnumerable<TResult> Map<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation) =>
        MapOperation.Map(source, transformation);

    public static IEnumerable<T> Filter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition) =>
        FilterOperation.Filter(source, condition);

    public static TAccumulate Reduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner) =>
        ReduceOperation.Reduce(source, initialValue, combiner);

    #endregion

    #region Queryable Map-Filter-Reduce Operations

    public static IQueryable<TResult> Map<TSource, TResult>(
        this IQueryable<TSource> source,
        System.Linq.Expressions.Expression<Func<TSource, TResult>> transformation) =>
        QueryableMapFilterReduce.Map(source, transformation);

    public static IQueryable<T> Filter<T>(
        this IQueryable<T> source,
        System.Linq.Expressions.Expression<Func<T, bool>> condition) =>
        QueryableMapFilterReduce.Filter(source, condition);

    public static TAccumulate Reduce<TSource, TAccumulate>(
        this IQueryable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner) =>
        QueryableMapFilterReduce.Reduce(source, initialValue, combiner);

    #endregion

    #region Lazy Evaluation Strategy

    public static IEnumerable<TResult> LazyMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation) =>
        LazyTransformations.LazyMap(source, transformation);

    public static IEnumerable<T> LazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition) =>
        LazyTransformations.LazyFilter(source, condition);

    public static TAccumulate LazyReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        Func<TAccumulate, bool>? shouldTerminateEarly = null) =>
        LazyTransformations.LazyReduce(source, initialValue, combiner, shouldTerminateEarly);

    #endregion

    #region Memoization Strategy

    public static IEnumerable<TResult> MemoMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        Func<TSource, object>? cacheKeyGenerator = null) =>
        MemoizedTransformations.MemoMap(source, transformation, cacheKeyGenerator);

    public static IEnumerable<T> MemoFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null) =>
        MemoizedTransformations.MemoFilter(source, condition, cacheKeyGenerator);

    public static TAccumulate MemoReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        object? cacheIdentifier = null) =>
        MemoizedTransformations.MemoReduce(source, initialValue, combiner, cacheIdentifier);

    public static void ClearComputationCache() =>
        MemoizedTransformations.ClearComputationCache();

    #endregion

    #region Batch Processing Strategy

    public static IEnumerable<TResult> BatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100) =>
        BatchTransformations.BatchMap(source, transformation, batchSize);

    public static IEnumerable<T> BatchFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        int batchSize = 100) =>
        BatchTransformations.BatchFilter(source, condition, batchSize);

    public static TAccumulate BatchReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        int batchSize = 100) =>
        BatchTransformations.BatchReduce(source, initialValue, combiner, batchSize);

    #endregion

    #region Composite Strategies

    public static IEnumerable<TResult> LazyBatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100) =>
        CompositeTransformations.LazyBatchMap(source, transformation, batchSize);

    public static IEnumerable<T> MemoLazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null) =>
        CompositeTransformations.MemoLazyFilter(source, condition, cacheKeyGenerator);

    #endregion

    #region Sorting Using Map-Filter-Reduce

    public static List<T> SortBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keyExtractor,
        bool descendingOrder = false) where TKey : IComparable<TKey> =>
        FunctionalSort.SortBy(source, keyExtractor, descendingOrder);

    #endregion
}
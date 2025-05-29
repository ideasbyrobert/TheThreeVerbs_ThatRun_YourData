using TheaterSales.Extended.MapFilterReduce.Strategies.Basic;
using TheaterSales.Extended.MapFilterReduce.Strategies.Lazy;
using TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;
using TheaterSales.Extended.MapFilterReduce.Strategies.Batch;
using TheaterSales.Extended.MapFilterReduce.Strategies.Composite;
using TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

namespace TheaterSales.Extended.MapFilterReduce;

public static class MapFilterReduceExtensions
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation) =>
        BasicOperations.Map(source, transformation);

    public static IEnumerable<T> Filter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition) =>
        BasicOperations.Filter(source, condition);

    public static TAccumulate Reduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner) =>
        BasicOperations.Reduce(source, initialValue, combiner);

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

    public static IEnumerable<TResult> LazyMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation) =>
        LazyOperations.Map(source, transformation);

    public static IEnumerable<T> LazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition) =>
        LazyOperations.Filter(source, condition);

    public static TAccumulate LazyReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        Func<TAccumulate, bool>? shouldTerminateEarly = null) =>
        LazyOperations.Reduce(source, initialValue, combiner, shouldTerminateEarly);

    public static IEnumerable<TResult> MemoMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        Func<TSource, object>? cacheKeyGenerator = null) =>
        MemoizedOperations.Map(source, transformation, cacheKeyGenerator);

    public static IEnumerable<T> MemoFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null) =>
        MemoizedOperations.Filter(source, condition, cacheKeyGenerator);

    public static TAccumulate MemoReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        object? cacheIdentifier = null) =>
        MemoizedOperations.Reduce(source, initialValue, combiner, cacheIdentifier);

    public static void ClearComputationCache() =>
        MemoizedOperations.ClearCache();

    public static IEnumerable<TResult> BatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100) =>
        BatchOperations.Map(source, transformation, batchSize);

    public static IEnumerable<T> BatchFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        int batchSize = 100) =>
        BatchOperations.Filter(source, condition, batchSize);

    public static TAccumulate BatchReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        int batchSize = 100) =>
        BatchOperations.Reduce(source, initialValue, combiner, batchSize);

    public static IEnumerable<TResult> LazyBatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100) =>
        CompositeOperations.LazyBatchMap(source, transformation, batchSize);

    public static IEnumerable<T> MemoLazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null) =>
        CompositeOperations.MemoLazyFilter(source, condition, cacheKeyGenerator);

    public static List<T> SortBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keyExtractor,
        bool descendingOrder = false) where TKey : IComparable<TKey> =>
        FunctionalSort.SortBy(source, keyExtractor, descendingOrder);
}
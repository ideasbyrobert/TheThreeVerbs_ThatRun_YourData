using System.Linq.Expressions;

namespace TheaterSales.Strategy.MapFilterReduce.Strategies.Basic;

public static class QueryableMapFilterReduce
{
    public static IQueryable<TResult> Map<TSource, TResult>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TResult>> transformation)
    {
        return source.Select(transformation);
    }

    public static IQueryable<T> Filter<T>(
        this IQueryable<T> source,
        Expression<Func<T, bool>> condition)
    {
        return source.Where(condition);
    }

    public static TAccumulate Reduce<TSource, TAccumulate>(
        this IQueryable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner)
    {
        return source.Aggregate(initialValue, combiner);
    }
}
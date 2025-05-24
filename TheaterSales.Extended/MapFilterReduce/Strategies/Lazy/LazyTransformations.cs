using TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Lazy;

public static class LazyTransformations
{
    public static IEnumerable<TResult> LazyMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation)
    {
        return new TransformationIterator<TSource, TResult>(source, transformation);
    }

    public static IEnumerable<T> LazyFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition)
    {
        return new FilterIterator<T>(source, condition);
    }

    public static TAccumulate LazyReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        Func<TAccumulate, bool>? shouldTerminateEarly = null)
    {
        var accumulator = initialValue;
        
        foreach (var element in source)
        {
            accumulator = combiner(accumulator, element);
            if (shouldTerminateEarly?.Invoke(accumulator) == true)
                break;
        }
        
        return accumulator;
    }
}
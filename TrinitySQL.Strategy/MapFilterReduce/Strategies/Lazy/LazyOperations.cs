namespace TrinitySQL.Strategy.MapFilterReduce.Strategies.Lazy;

public static class LazyOperations
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, TResult> transformation)
    {
        foreach (var item in source)
        {
            yield return transformation(item);
        }
    }

    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> source,
        Func<T, bool> condition)
    {
        foreach (var item in source)
        {
            if (condition(item))
                yield return item;
        }
    }

    public static TAccumulate Reduce<TSource, TAccumulate>(
        IEnumerable<TSource> source,
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
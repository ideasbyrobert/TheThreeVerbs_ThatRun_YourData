using System.Collections.Concurrent;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;

public static class ComputationCache
{
    private static readonly ConcurrentDictionary<object, object> Cache = new();

    public static TResult GetOrCompute<TResult>(
        object cacheKey, 
        Func<TResult> computation)
    {
        return (TResult)Cache.GetOrAdd(cacheKey, _ => computation()!);
    }

    public static void Clear() => Cache.Clear();

    public static object CreateKey<T>(object key, string operation) =>
        (typeof(T), key, operation);
}
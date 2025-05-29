namespace TheaterSales.Extended.MapFilterReduce.Strategies.Composite;

public static class CompositeOperations
{
    public static IEnumerable<TResult> LazyBatchMap<TSource, TResult>(
        IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100)
    {
        var batch = new List<TSource>(batchSize);
        
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                foreach (var batchItem in batch)
                    yield return transformation(batchItem);
                batch.Clear();
            }
        }
        
        foreach (var remaining in batch)
            yield return transformation(remaining);
    }

    public static IEnumerable<T> MemoLazyFilter<T>(
        IEnumerable<T> source,
        Func<T, bool> condition,
        Func<T, object>? cacheKeyGenerator = null)
    {
        var cache = new Dictionary<object, bool>();
        var keyGenerator = cacheKeyGenerator ?? (item => item!);
        
        foreach (var item in source)
        {
            var key = keyGenerator(item);
            if (!cache.TryGetValue(key, out var result))
            {
                result = condition(item);
                cache[key] = result;
            }
            
            if (result)
                yield return item;
        }
    }
}
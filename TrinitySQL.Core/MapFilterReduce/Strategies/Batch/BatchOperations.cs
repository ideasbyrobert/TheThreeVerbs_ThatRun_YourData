namespace TrinitySQL.Core.MapFilterReduce.Strategies.Batch;

public static class BatchOperations
{
    public static IEnumerable<TResult> Map<TSource, TResult>(
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

    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> source,
        Func<T, bool> condition,
        int batchSize = 100)
    {
        var batch = new List<T>(batchSize);
        
        foreach (var item in source)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                foreach (var batchItem in batch.Where(condition))
                    yield return batchItem;
                batch.Clear();
            }
        }
        
        foreach (var remaining in batch.Where(condition))
            yield return remaining;
    }

    public static TAccumulate Reduce<TSource, TAccumulate>(
        IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        int batchSize = 100)
    {
        var accumulator = initialValue;
        var batch = new List<TSource>(batchSize);

        foreach (var element in source)
        {
            batch.Add(element);
            if (batch.Count >= batchSize)
            {
                accumulator = batch.Aggregate(accumulator, combiner);
                batch.Clear();
            }
        }

        return batch.Aggregate(accumulator, combiner);
    }
}
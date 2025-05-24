using TheaterSales.Extended.MapFilterReduce.Strategies.Basic;
using TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

namespace TheaterSales.Extended.MapFilterReduce.Strategies.Batch;

public static class BatchTransformations
{
    public static IEnumerable<TResult> BatchMap<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> transformation,
        int batchSize = 100)
    {
        return new BatchIterator<TSource, TResult>(source, transformation, batchSize);
    }

    public static IEnumerable<T> BatchFilter<T>(
        this IEnumerable<T> source,
        Func<T, bool> condition,
        int batchSize = 100)
    {
        return new BatchFilterIterator<T>(source, condition, batchSize);
    }

    public static TAccumulate BatchReduce<TSource, TAccumulate>(
        this IEnumerable<TSource> source,
        TAccumulate initialValue,
        Func<TAccumulate, TSource, TAccumulate> combiner,
        int batchSize = 100)
    {
        var accumulator = initialValue;
        var currentBatch = new List<TSource>(batchSize);

        foreach (var element in source)
        {
            currentBatch.Add(element);
            if (IsBatchFull(currentBatch, batchSize))
            {
                accumulator = ReduceBatch(currentBatch, accumulator, combiner);
                currentBatch.Clear();
            }
        }

        return ReduceBatch(currentBatch, accumulator, combiner);
    }

    private static bool IsBatchFull<T>(List<T> batch, int batchSize) =>
        batch.Count >= batchSize;

    private static TAccumulate ReduceBatch<TSource, TAccumulate>(
        List<TSource> batch,
        TAccumulate accumulator,
        Func<TAccumulate, TSource, TAccumulate> combiner)
    {
        return ReduceOperation.Reduce(batch, accumulator, combiner);
    }
}
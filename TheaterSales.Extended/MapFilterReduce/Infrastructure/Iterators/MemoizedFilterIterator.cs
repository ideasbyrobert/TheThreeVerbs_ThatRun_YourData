using TheaterSales.Extended.MapFilterReduce.Strategies.Memoization;

namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public class MemoizedFilterIterator<T> : FunctionalIteratorBase<T>
{
    private readonly IEnumerator<T> _sourceEnumerator;
    private readonly Func<T, bool> _condition;
    private readonly Func<T, object> _keyGenerator;

    public MemoizedFilterIterator(
        IEnumerable<T> source, 
        Func<T, bool> condition,
        Func<T, object> keyGenerator)
    {
        _sourceEnumerator = source.GetEnumerator();
        _condition = condition;
        _keyGenerator = keyGenerator;
    }

    protected override bool ComputeNext(out T nextValue)
    {
        while (_sourceEnumerator.MoveNext())
        {
            var current = _sourceEnumerator.Current;
            if (ShouldIncludeElement(current))
            {
                nextValue = current;
                return true;
            }
        }

        nextValue = default!;
        return false;
    }

    private bool ShouldIncludeElement(T element)
    {
        var cacheKey = ComputationCache.CreateKey<T>(
            _keyGenerator(element), "memolazyfilter");
        return ComputationCache.GetOrCompute(cacheKey, 
            () => _condition(element));
    }

    protected override void OnReset()
    {
        _sourceEnumerator.Reset();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sourceEnumerator.Dispose();
        }
    }
}
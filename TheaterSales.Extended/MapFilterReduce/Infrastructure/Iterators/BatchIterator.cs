namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public class BatchIterator<TSource, TResult> : FunctionalIteratorBase<TResult>
{
    private readonly IEnumerator<TSource> _sourceEnumerator;
    private readonly Func<TSource, TResult> _transformation;
    private readonly int _batchSize;
    private readonly List<TSource> _currentBatch;
    private IEnumerator<TResult>? _batchResultsEnumerator;

    public BatchIterator(
        IEnumerable<TSource> source, 
        Func<TSource, TResult> transformation, 
        int batchSize)
    {
        _sourceEnumerator = source.GetEnumerator();
        _transformation = transformation;
        _batchSize = batchSize;
        _currentBatch = new List<TSource>(batchSize);
    }

    protected override bool ComputeNext(out TResult nextValue)
    {
        if (_batchResultsEnumerator != null && _batchResultsEnumerator.MoveNext())
        {
            nextValue = _batchResultsEnumerator.Current;
            return true;
        }

        _currentBatch.Clear();
        var hasMoreElements = false;

        while (_currentBatch.Count < _batchSize && _sourceEnumerator.MoveNext())
        {
            _currentBatch.Add(_sourceEnumerator.Current);
            hasMoreElements = true;
        }

        if (hasMoreElements)
        {
            var transformedBatch = _currentBatch.Select(_transformation);
            _batchResultsEnumerator?.Dispose();
            _batchResultsEnumerator = transformedBatch.GetEnumerator();
            
            if (_batchResultsEnumerator.MoveNext())
            {
                nextValue = _batchResultsEnumerator.Current;
                return true;
            }
        }

        nextValue = default!;
        return false;
    }

    protected override void OnReset()
    {
        _sourceEnumerator.Reset();
        _batchResultsEnumerator?.Dispose();
        _batchResultsEnumerator = null;
        _currentBatch.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sourceEnumerator.Dispose();
            _batchResultsEnumerator?.Dispose();
        }
    }
}
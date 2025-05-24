namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public class BatchFilterIterator<T> : FunctionalIteratorBase<T>
{
    private readonly IEnumerator<T> _sourceEnumerator;
    private readonly Func<T, bool> _condition;
    private readonly int _batchSize;
    private readonly List<T> _currentBatch;
    private IEnumerator<T>? _filteredBatchEnumerator;

    public BatchFilterIterator(
        IEnumerable<T> source, 
        Func<T, bool> condition, 
        int batchSize)
    {
        _sourceEnumerator = source.GetEnumerator();
        _condition = condition;
        _batchSize = batchSize;
        _currentBatch = new List<T>(batchSize);
    }

    protected override bool ComputeNext(out T nextValue)
    {
        if (_filteredBatchEnumerator != null && _filteredBatchEnumerator.MoveNext())
        {
            nextValue = _filteredBatchEnumerator.Current;
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
            var filteredBatch = _currentBatch.Where(_condition);
            _filteredBatchEnumerator?.Dispose();
            _filteredBatchEnumerator = filteredBatch.GetEnumerator();
            
            if (_filteredBatchEnumerator.MoveNext())
            {
                nextValue = _filteredBatchEnumerator.Current;
                return true;
            }
        }

        nextValue = default!;
        return false;
    }

    protected override void OnReset()
    {
        _sourceEnumerator.Reset();
        _filteredBatchEnumerator?.Dispose();
        _filteredBatchEnumerator = null;
        _currentBatch.Clear();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sourceEnumerator.Dispose();
            _filteredBatchEnumerator?.Dispose();
        }
    }
}
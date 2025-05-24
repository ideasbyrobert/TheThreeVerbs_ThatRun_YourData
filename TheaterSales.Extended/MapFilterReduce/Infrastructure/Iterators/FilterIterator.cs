namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public class FilterIterator<T> : FunctionalIteratorBase<T>
{
    private readonly IEnumerator<T> _sourceEnumerator;
    private readonly Func<T, bool> _condition;

    public FilterIterator(IEnumerable<T> source, Func<T, bool> condition)
    {
        _sourceEnumerator = source.GetEnumerator();
        _condition = condition;
    }

    protected override bool ComputeNext(out T nextValue)
    {
        while (_sourceEnumerator.MoveNext())
        {
            var current = _sourceEnumerator.Current;
            if (_condition(current))
            {
                nextValue = current;
                return true;
            }
        }

        nextValue = default!;
        return false;
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
namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public class TransformationIterator<TSource, TResult> : FunctionalIteratorBase<TResult>
{
    private readonly IEnumerator<TSource> _sourceEnumerator;
    private readonly Func<TSource, TResult> _transformation;

    public TransformationIterator(
        IEnumerable<TSource> source, 
        Func<TSource, TResult> transformation)
    {
        _sourceEnumerator = source.GetEnumerator();
        _transformation = transformation;
    }

    protected override bool ComputeNext(out TResult nextValue)
    {
        if (_sourceEnumerator.MoveNext())
        {
            nextValue = _transformation(_sourceEnumerator.Current);
            return true;
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
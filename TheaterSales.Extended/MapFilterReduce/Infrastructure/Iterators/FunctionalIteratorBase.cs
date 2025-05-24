using System.Collections;

namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public abstract class FunctionalIteratorBase<T> : IFunctionalIterator<T>
{
    private bool _hasStarted;
    private bool _isCompleted;
    private T _current = default!;

    public T Current => _current;
    object IEnumerator.Current => Current!;

    public bool MoveNext()
    {
        if (_isCompleted) return false;

        if (!_hasStarted)
        {
            _hasStarted = true;
            Initialize();
        }

        var hasNext = ComputeNext(out var nextValue);
        if (hasNext)
        {
            _current = nextValue;
        }
        else
        {
            _isCompleted = true;
            _current = default!;
        }

        return hasNext;
    }

    protected virtual void Initialize() { }
    protected abstract bool ComputeNext(out T nextValue);

    public void Reset()
    {
        _hasStarted = false;
        _isCompleted = false;
        _current = default!;
        OnReset();
    }

    protected virtual void OnReset() { }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }

    public IFunctionalIterator<T> GetEnumerator() => this;
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this;
}
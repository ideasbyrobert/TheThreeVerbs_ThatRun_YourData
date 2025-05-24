namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Iterators;

public interface IFunctionalIterator<T> : IEnumerable<T>, IEnumerator<T>
{
    new IFunctionalIterator<T> GetEnumerator();
}
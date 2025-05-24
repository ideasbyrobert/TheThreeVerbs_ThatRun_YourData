namespace TheaterSales.DotNet.Core.SharedKernel;

public interface IQuery<TResult>
{
    string QueryName => GetType().Name;
}
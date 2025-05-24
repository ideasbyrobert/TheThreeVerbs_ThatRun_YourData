namespace TheaterSales.DotNet.Core.SharedKernel;

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query);
}
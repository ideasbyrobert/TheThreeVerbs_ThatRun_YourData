namespace TrinitySQL.Server.Core.SharedKernel;

public interface IQueryDispatcher
{
    TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>;
}
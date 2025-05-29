using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Strategy.MapFilterReduce;

namespace TheaterSales.Server.Infrastructure;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly Dictionary<Type, object> _handlers = new();

    public void RegisterHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> handler) 
        where TQuery : IQuery<TResult>
    {
        _handlers[typeof(TQuery)] = handler;
    }

    public TResult Dispatch<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
    {
        var queryType = typeof(TQuery);
        
        if (!_handlers.ContainsKey(queryType))
        {
            throw new InvalidOperationException($"No handler registered for query type {queryType.Name}");
        }

        var handler = (IQueryHandler<TQuery, TResult>)_handlers[queryType];
        return handler.Handle(query);
    }

    public IEnumerable<Type> GetRegisteredQueryTypes() =>
        _handlers.Keys
            .Map(type => type)
            .SortBy(type => type.Name);
}
using TheaterSales.DotNet.Data;
using TheaterSales.DotNet.Domain;
using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.Infrastructure.EventBus;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.QueryHandlers;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.DotNet.Infrastructure;

public class ServiceConfiguration
{
    private readonly TheaterSalesContext _context;
    private readonly IEventBus _eventBus;
    private readonly IQueryDispatcher _queryDispatcher;

    public ServiceConfiguration(TheaterSalesContext context)
    {
        _context = context;
        _eventBus = new InMemoryEventBus();
        _queryDispatcher = new QueryDispatcher();
        
        RegisterQueryHandlers();
    }

    private void RegisterQueryHandlers()
    {
        var dispatcher = (QueryDispatcher)_queryDispatcher;
        
        // Theater Performance bounded context
        dispatcher.RegisterHandler(new GetTheatersByDateQueryHandler(_context, _eventBus));
        dispatcher.RegisterHandler(new GetTopPerformingTheatersQueryHandler(_context, _eventBus));
        dispatcher.RegisterHandler(new GetUnderperformingTheatersQueryHandler(_context, _eventBus));
    }

    public IQueryDispatcher QueryDispatcher => _queryDispatcher;
    public IEventBus EventBus => _eventBus;
    
    // Example usage
    public Theater? FindHighestSalesTheater(DateOnly date)
    {
        var query = new GetTheatersByDateQuery(date);
        var results = _queryDispatcher.Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query);
        
        return results
            .OrderByDescending(r => r.TotalRevenue)
            .FirstOrDefault()
            ?.Theater;
    }
}
using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.Data;
using TheaterSales.DotNet.Domain;
using TheaterSales.Extended;
using TheaterSales.DotNet.Infrastructure.EventBus;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

public class GetTheatersByDateQueryHandler : IQueryHandler<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>
{
    private readonly TheaterSalesContext _context;
    private readonly IEventBus _eventBus;

    public GetTheatersByDateQueryHandler(TheaterSalesContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public IEnumerable<TheaterPerformanceResult> Handle(GetTheatersByDateQuery query)
    {
        var results = _context.Theaters
            .Reduce(new List<Theater>(), (acc, t) => { acc.Add(t); return acc; })
            .LazyMap(theater => new TheaterSalesAggregate(
                theater,
                _context.Sales
                    .Filter(sale => sale.SaleDate == query.Date)
                    .Filter(sale => sale.TheaterId == theater.Id)
                    .Reduce(0m, (sum, sale) => sum + sale.Amount)))
            .MemoMap(
                aggregate => aggregate,
                aggregate => (aggregate.Theater.Id, query.Date))
            .SortBy(aggregate => aggregate.TotalSales, descendingOrder: true)
            .Map(aggregate => TheaterPerformanceResult.FromAggregate(aggregate, query.Date))
            .Reduce(new List<TheaterPerformanceResult>(), (acc, result) => 
            { 
                acc.Add(result); 
                return acc; 
            });

        var highestRevenue = results
            .LazyReduce(0m, (max, result) => result.TotalRevenue > max ? result.TotalRevenue : max);

        _eventBus.Publish(new TheaterPerformanceQueriedEvent(
            query.Date,
            results.Count,
            highestRevenue));

        return results;
    }
}
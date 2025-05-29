using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.Data;
using TrinitySQL.Server.Domain;
using TrinitySQL.Core.MapFilterReduce;
using TrinitySQL.Server.Infrastructure.EventBus;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
        var theaters = _context.Theaters.ToList();
        var sales = _context.Sales.ToList();

        var salesOnDate = sales
            .Filter(sale => sale.SaleDate == query.Date);

        var results = theaters
            .MemoMap(
                theater => new TheaterSalesAggregate(
                    theater,
                    salesOnDate
                        .Filter(sale => sale.TheaterId == theater.Id)
                        .Reduce(0m, (sum, sale) => sum + sale.Amount)),
                theater => (theater.Id, query.Date))
            .SortBy(aggregate => aggregate.TotalSales, descendingOrder: true)
            .Map(aggregate => TheaterPerformanceResult.FromAggregate(aggregate, query.Date))
            .Reduce(new List<TheaterPerformanceResult>(), (acc, result) => 
            { 
                acc.Add(result); 
                return acc; 
            });

        var highestRevenue = results
            .Map(result => result.TotalRevenue)
            .Reduce(0m, (max, revenue) => revenue > max ? revenue : max);

        _eventBus.Publish(new TheaterPerformanceQueriedEvent(
            query.Date,
            results.Count,
            highestRevenue));

        return results;
    }
}
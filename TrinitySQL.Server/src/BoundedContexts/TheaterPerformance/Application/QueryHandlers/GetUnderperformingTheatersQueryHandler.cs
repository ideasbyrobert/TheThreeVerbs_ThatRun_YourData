using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.Data;
using TrinitySQL.Server.Domain;
using TrinitySQL.Strategy.MapFilterReduce;
using TrinitySQL.Server.Infrastructure.EventBus;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

public class GetUnderperformingTheatersQueryHandler : IQueryHandler<GetUnderperformingTheatersQuery, IEnumerable<TheaterPerformanceResult>>
{
    private readonly TheaterSalesContext _context;
    private readonly IEventBus _eventBus;

    public GetUnderperformingTheatersQueryHandler(TheaterSalesContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public IEnumerable<TheaterPerformanceResult> Handle(GetUnderperformingTheatersQuery query)
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
            .Filter(aggregate => aggregate.TotalSales <= query.Threshold)
            .SortBy(aggregate => aggregate.TotalSales, descendingOrder: true)
            .Map(aggregate => TheaterPerformanceResult.FromAggregate(aggregate, query.Date))
            .Reduce(new List<TheaterPerformanceResult>(), (acc, result) => 
            { 
                acc.Add(result); 
                return acc; 
            });

        _eventBus.Publish(new TheaterPerformanceQueriedEvent(
            query.Date,
            results.Count,
            0m));

        return results;
    }
}
using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.Data;
using TrinitySQL.Server.Domain;
using TrinitySQL.Core.MapFilterReduce;
using TrinitySQL.Server.Infrastructure.EventBus;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

public class GetTopPerformingTheatersQueryHandler : IQueryHandler<GetTopPerformingTheatersQuery, IEnumerable<TheaterPerformanceResult>>
{
    private readonly TheaterSalesContext _context;
    private readonly IEventBus _eventBus;

    public GetTopPerformingTheatersQueryHandler(TheaterSalesContext context, IEventBus eventBus)
    {
        _context = context;
        _eventBus = eventBus;
    }

    public IEnumerable<TheaterPerformanceResult> Handle(GetTopPerformingTheatersQuery query)
    {
        var theaters = _context.Theaters.ToList();
        var sales = _context.Sales.ToList();

        var salesInDateRange = sales
            .Filter(sale => sale.SaleDate >= query.DateRange.StartDate && sale.SaleDate <= query.DateRange.EndDate);

        var results = theaters
            .Map(theater => new TheaterSalesAggregate(
                theater,
                salesInDateRange
                    .Filter(sale => sale.TheaterId == theater.Id)
                    .Reduce(0m, (sum, sale) => sum + sale.Amount)))
            .SortBy(aggregate => aggregate.TotalSales, descendingOrder: true)
            .Reduce(
                new List<TheaterPerformanceResult>(),
                (acc, aggregate) =>
                {
                    if (acc.Count < query.TopCount)
                        acc.Add(TheaterPerformanceResult.FromAggregate(aggregate));
                    return acc;
                });

        _eventBus.Publish(new TopTheatersQueriedEvent(
            query.DateRange.StartDate,
            query.DateRange.EndDate,
            query.TopCount,
            results.Count));

        return results;
    }
}
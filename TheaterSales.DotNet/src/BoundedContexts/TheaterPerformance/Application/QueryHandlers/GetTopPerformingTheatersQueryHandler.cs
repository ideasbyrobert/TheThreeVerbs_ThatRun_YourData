using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.Data;
using TheaterSales.DotNet.Domain;
using TheaterSales.Extended;
using TheaterSales.DotNet.Infrastructure.EventBus;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
        var results = _context.Theaters
            .Reduce(new List<Theater>(), (acc, t) => { acc.Add(t); return acc; })
            .LazyMap(theater => new TheaterSalesAggregate(
                theater,
                _context.Sales
                    .Filter(sale => sale.SaleDate >= query.DateRange.StartDate && sale.SaleDate <= query.DateRange.EndDate)
                    .Filter(s => s.TheaterId == theater.Id)
                    .Reduce(0m, (sum, sale) => sum + sale.Amount)))
            .SortBy(aggregate => aggregate.TotalSales, descendingOrder: true)
            .LazyReduce((new List<TheaterSalesAggregate>(), 0), (acc, item) =>
            {
                if (acc.Item2 < query.TopCount)
                {
                    acc.Item1.Add(item);
                    return (acc.Item1, acc.Item2 + 1);
                }
                return acc;
            },
            acc => acc.Item2 >= query.TopCount).Item1
            .Map(aggregate => TheaterPerformanceResult.FromAggregate(aggregate))
            .Reduce(new List<TheaterPerformanceResult>(), (acc, result) => 
            { 
                acc.Add(result); 
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
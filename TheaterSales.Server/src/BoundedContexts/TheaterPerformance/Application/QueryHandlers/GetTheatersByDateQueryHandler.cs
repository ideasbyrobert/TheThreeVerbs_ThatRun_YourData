using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Server.Data;
using TheaterSales.Server.Domain;
using TheaterSales.Strategy.MapFilterReduce;
using TheaterSales.Server.Infrastructure.EventBus;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
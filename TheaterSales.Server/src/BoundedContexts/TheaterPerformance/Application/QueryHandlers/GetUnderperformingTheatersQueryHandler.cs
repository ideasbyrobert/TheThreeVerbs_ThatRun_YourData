using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Server.Data;
using TheaterSales.Server.Domain;
using TheaterSales.Extended.MapFilterReduce;
using TheaterSales.Server.Infrastructure.EventBus;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
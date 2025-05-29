using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.Data;
using FunctionalSQL.Server.Domain;
using FunctionalSQL.Strategy.MapFilterReduce;
using FunctionalSQL.Server.Infrastructure.EventBus;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

namespace FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
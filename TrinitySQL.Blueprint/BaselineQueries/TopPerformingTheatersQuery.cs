using TrinitySQL.Server.Data;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Blueprint.BaselineQueries;

public class TopPerformingTheatersQuery
{
    private readonly TheaterSalesContext _context;

    public TopPerformingTheatersQuery(TheaterSalesContext context)
    {
        _context = context;
    }

    public List<TheaterPerformanceResult> Execute(DateRange dateRange, int topCount)
    {
        if (topCount == 0)
        {
            return new List<TheaterPerformanceResult>();
        }

        var salesInRange = _context.Sales
            .Where(sale => sale.SaleDate >= dateRange.StartDate &&
                          sale.SaleDate <= dateRange.EndDate)
            .GroupBy(sale => sale.TheaterId)
            .Select(group => new
            {
                TheaterId = group.Key,
                TotalRevenue = group.Sum(sale => sale.Amount)
            })
            .ToList();

        var allTheaters = _context.Theaters.ToList();

        var results = allTheaters
            .Select(theater =>
            {
                var revenue = salesInRange
                    .FirstOrDefault(s => s.TheaterId == theater.Id)?.TotalRevenue ?? 0m;

                return new TheaterPerformanceResult(theater, revenue);
            })
            .OrderByDescending(result => result.TotalRevenue)
            .Take(topCount)
            .ToList();

        return results;
    }
}
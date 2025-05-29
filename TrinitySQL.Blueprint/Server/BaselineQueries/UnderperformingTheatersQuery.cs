using TrinitySQL.Server.Data;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Blueprint.Server.BaselineQueries;

public class UnderperformingTheatersQuery
{
    private readonly TheaterSalesContext _context;

    public UnderperformingTheatersQuery(TheaterSalesContext context)
    {
        _context = context;
    }

    public List<TheaterPerformanceResult> Execute(DateOnly date, decimal revenueThreshold)
    {
        var salesByTheater = _context.Sales
            .Where(sale => sale.SaleDate == date)
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
                var revenue = salesByTheater
                    .FirstOrDefault(s => s.TheaterId == theater.Id)?.TotalRevenue ?? 0m;

                return new TheaterPerformanceResult(theater, revenue);
            })
            .Where(result => result.TotalRevenue <= revenueThreshold)
            .OrderByDescending(result => result.TotalRevenue)
            .ToList();

        return results;
    }
}
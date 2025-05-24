using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.Domain;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

public record TheaterPerformanceResult : ValueObject
{
    public Theater Theater { get; }
    public decimal TotalRevenue { get; }
    public DateOnly? PerformanceDate { get; }

    public TheaterPerformanceResult(Theater theater, decimal totalRevenue, DateOnly? performanceDate = null)
    {
        if (totalRevenue < 0)
            throw new ArgumentException("Total revenue cannot be negative");

        Theater = theater ?? throw new ArgumentNullException(nameof(theater));
        TotalRevenue = totalRevenue;
        PerformanceDate = performanceDate;
    }

    public static TheaterPerformanceResult FromAggregate(TheaterSalesAggregate aggregate, DateOnly? date = null) =>
        new(aggregate.Theater, aggregate.TotalSales, date);
}
using TheaterSales.DotNet.Core.SharedKernel;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

public record DateRange : ValueObject
{
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public DateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date");

        StartDate = startDate;
        EndDate = endDate;
    }

    public bool Contains(DateOnly date) =>
        date >= StartDate && date <= EndDate;

    public int TotalDays =>
        (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
}
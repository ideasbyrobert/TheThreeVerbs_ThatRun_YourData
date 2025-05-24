using TheaterSales.DotNet.Core.SharedKernel;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.Events;

public record TheaterPerformanceQueriedEvent(
    DateOnly Date,
    int TheaterCount,
    decimal HighestRevenue) : DomainEvent
{
    public override string EventType => "TheaterPerformanceQueried";
}
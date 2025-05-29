using TheaterSales.Server.Core.SharedKernel;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.Events;

public record TheaterPerformanceQueriedEvent(
    DateOnly Date,
    int TheaterCount,
    decimal HighestRevenue) : DomainEvent
{
    public override string EventType => "TheaterPerformanceQueried";
}
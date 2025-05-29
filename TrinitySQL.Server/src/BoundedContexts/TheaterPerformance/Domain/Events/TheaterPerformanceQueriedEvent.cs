using TrinitySQL.Server.Core.SharedKernel;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

public record TheaterPerformanceQueriedEvent(
    DateOnly Date,
    int TheaterCount,
    decimal HighestRevenue) : DomainEvent
{
    public override string EventType => "TheaterPerformanceQueried";
}
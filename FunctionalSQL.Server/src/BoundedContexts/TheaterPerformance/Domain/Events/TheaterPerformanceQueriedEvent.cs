using FunctionalSQL.Server.Core.SharedKernel;

namespace FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

public record TheaterPerformanceQueriedEvent(
    DateOnly Date,
    int TheaterCount,
    decimal HighestRevenue) : DomainEvent
{
    public override string EventType => "TheaterPerformanceQueried";
}
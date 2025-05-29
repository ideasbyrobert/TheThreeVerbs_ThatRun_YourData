using TrinitySQL.Server.Core.SharedKernel;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.Events;

public record TopTheatersQueriedEvent(
    DateOnly StartDate,
    DateOnly EndDate,
    int TopCount,
    int ResultCount) : DomainEvent
{
    public override string EventType => "TopTheatersQueried";
}
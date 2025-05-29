using TheaterSales.Server.Core.SharedKernel;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.Events;

public record TopTheatersQueriedEvent(
    DateOnly StartDate,
    DateOnly EndDate,
    int TopCount,
    int ResultCount) : DomainEvent
{
    public override string EventType => "TopTheatersQueried";
}
using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTopPerformingTheatersQuery(
    DateRange DateRange,
    int TopCount) : IQuery<IEnumerable<TheaterPerformanceResult>>;
using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTopPerformingTheatersQuery(
    DateRange DateRange,
    int TopCount) : IQuery<IEnumerable<TheaterPerformanceResult>>;
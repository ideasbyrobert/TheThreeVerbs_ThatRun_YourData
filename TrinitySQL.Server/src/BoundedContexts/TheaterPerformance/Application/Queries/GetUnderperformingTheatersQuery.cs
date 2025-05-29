using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetUnderperformingTheatersQuery(
    DateOnly Date,
    decimal Threshold = 0m) : IQuery<IEnumerable<TheaterPerformanceResult>>;
using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTheatersByDateQuery(DateOnly Date) : IQuery<IEnumerable<TheaterPerformanceResult>>;
using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTheatersByDateQuery(DateOnly Date) : IQuery<IEnumerable<TheaterPerformanceResult>>;
using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetUnderperformingTheatersQuery(
    DateOnly Date,
    decimal Threshold = 0m) : IQuery<IEnumerable<TheaterPerformanceResult>>;
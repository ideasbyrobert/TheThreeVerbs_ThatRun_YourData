using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetUnderperformingTheatersQuery(
    DateOnly Date,
    decimal Threshold = 0m) : IQuery<IEnumerable<TheaterPerformanceResult>>;
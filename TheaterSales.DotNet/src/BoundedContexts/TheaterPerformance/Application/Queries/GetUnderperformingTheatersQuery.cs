using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetUnderperformingTheatersQuery(
    DateOnly Date,
    decimal Threshold = 0m) : IQuery<IEnumerable<TheaterPerformanceResult>>;
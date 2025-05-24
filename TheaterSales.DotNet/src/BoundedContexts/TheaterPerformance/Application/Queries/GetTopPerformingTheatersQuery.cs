using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTopPerformingTheatersQuery(
    DateRange DateRange,
    int TopCount) : IQuery<IEnumerable<TheaterPerformanceResult>>;
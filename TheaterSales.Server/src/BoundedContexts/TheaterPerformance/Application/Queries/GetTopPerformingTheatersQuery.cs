using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTopPerformingTheatersQuery(
    DateRange DateRange,
    int TopCount) : IQuery<IEnumerable<TheaterPerformanceResult>>;
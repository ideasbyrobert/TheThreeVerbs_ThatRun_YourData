using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTheatersByDateQuery(DateOnly Date) : IQuery<IEnumerable<TheaterPerformanceResult>>;
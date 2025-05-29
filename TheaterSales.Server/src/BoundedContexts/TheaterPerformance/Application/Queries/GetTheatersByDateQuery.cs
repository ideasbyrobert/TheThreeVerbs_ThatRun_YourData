using TheaterSales.Server.Core.SharedKernel;
using TheaterSales.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.Server.BoundedContexts.TheaterPerformance.Application.Queries;

public record GetTheatersByDateQuery(DateOnly Date) : IQuery<IEnumerable<TheaterPerformanceResult>>;
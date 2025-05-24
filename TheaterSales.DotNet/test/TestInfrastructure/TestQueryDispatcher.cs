using TheaterSales.DotNet.Data;
using TheaterSales.DotNet.Infrastructure;
using TheaterSales.DotNet.Infrastructure.EventBus;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

namespace TheaterSales.DotNet.Tests.TestInfrastructure;

public static class TestQueryDispatcher
{
    public static QueryDispatcher Create(TheaterSalesContext context)
    {
        var dispatcher = new QueryDispatcher();
        var eventBus = new InMemoryEventBus();

        dispatcher.RegisterHandler(new GetTheatersByDateQueryHandler(context, eventBus));
        dispatcher.RegisterHandler(new GetTopPerformingTheatersQueryHandler(context, eventBus));
        dispatcher.RegisterHandler(new GetUnderperformingTheatersQueryHandler(context, eventBus));

        return dispatcher;
    }
}
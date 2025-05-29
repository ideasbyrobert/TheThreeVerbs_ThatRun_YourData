using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Baseline;
using TrinitySQL.Blueprint.Server.TestInfrastructure;

namespace TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

[TestClass]
public class GetTheatersByDateQueryHandlerTests : QueryHandlerTestBase
{
    private TheatersByDateQuery _baselineQuery = null!;

    protected override void InitializeTest()
    {
        _baselineQuery = new TheatersByDateQuery(Context);
    }

    [TestMethod]
    public void VerifyTheatersByDateQueryHandlerMatchesBaselineOrmQuery()
    {
        var testDates = new[]
        {
            TestDates.IndependenceDay2024,
            TestDates.Christmas2024,
            TestDates.RegularSpringDay2024,
            TestDates.NewYearsDay2024,
            TestDates.Thanksgiving2024,
            TestDates.ZeroSalesDay
        };

        foreach (var date in testDates)
        {
            var baselineResults = _baselineQuery.Execute(date);
            var query = new GetTheatersByDateQuery(date);
            var queryHandlerResults = ExecuteQuery<GetTheatersByDateQuery, TheaterPerformanceResult>(query);

            TheaterPerformanceAssertions.AssertResultsMatchByTheater(
                baselineResults, queryHandlerResults, $"Date {date}");
            TheaterPerformanceAssertions.AssertSortedByRevenueDescending(
                queryHandlerResults, $"Date {date}");
        }
    }
}


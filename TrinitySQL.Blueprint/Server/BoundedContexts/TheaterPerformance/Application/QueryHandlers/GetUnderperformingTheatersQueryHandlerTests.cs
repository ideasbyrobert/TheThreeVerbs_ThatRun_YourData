using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Baseline;
using TrinitySQL.Blueprint.Server.TestInfrastructure;

namespace TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

[TestClass]
public class GetUnderperformingTheatersQueryHandlerTests : QueryHandlerTestBase
{
    private UnderperformingTheatersQuery _baselineQuery = null!;

    protected override void InitializeTest()
    {
        _baselineQuery = new UnderperformingTheatersQuery(Context);
    }

    [TestMethod]
    public void VerifyUnderperformingTheatersQueryHandlerMatchesBaselineOrmQuery()
    {
        var testScenarios = new[]
        {
            new { Date = TestDates.ZeroSalesDay, Threshold = 0m, Description = "Theaters with zero sales" },
            new { Date = TestDates.IndependenceDay2024, Threshold = 10000m, Description = "Below $10k on July 4th" },
            new { Date = TestDates.Christmas2024, Threshold = 5000m, Description = "Below $5k on Christmas" },
            new { Date = TestDates.RegularSpringDay2024, Threshold = 1000m, Description = "Below $1k on regular day" },
            new { Date = TestDates.Thanksgiving2024, Threshold = 15000m, Description = "Below $15k on Thanksgiving" }
        };

        foreach (var scenario in testScenarios)
        {
            var baselineResults = _baselineQuery.Execute(scenario.Date, scenario.Threshold);
            var query = new GetUnderperformingTheatersQuery(scenario.Date, scenario.Threshold);
            var queryHandlerResults = ExecuteQuery<GetUnderperformingTheatersQuery, TheaterPerformanceResult>(query);

            TheaterPerformanceAssertions.AssertResultsMatchAsSet(
                baselineResults, queryHandlerResults, scenario.Description);
            TheaterPerformanceAssertions.AssertSortedByRevenueDescending(
                queryHandlerResults, scenario.Description);
        }
    }

}
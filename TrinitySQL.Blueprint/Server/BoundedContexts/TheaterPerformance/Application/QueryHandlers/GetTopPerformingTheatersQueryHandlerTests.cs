using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Blueprint.Server.BaselineQueries;
using TrinitySQL.Blueprint.Server.TestInfrastructure;

namespace TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

[TestClass]
public class GetTopPerformingTheatersQueryHandlerTests : QueryHandlerTestBase
{
    private TopPerformingTheatersQuery _baselineQuery = null!;

    protected override void InitializeTest()
    {
        _baselineQuery = new TopPerformingTheatersQuery(Context);
    }

    [TestMethod]
    public void VerifyTopPerformingTheatersQueryHandlerMatchesBaselineOrmQuery()
    {
        var testScenarios = new[]
        {
            new { Period = TestDateRanges.May2024, TopCount = 3, Description = "Top 3 theaters in May" },
            new { Period = TestDateRanges.July2024, TopCount = 5, Description = "Top 5 theaters in July" },
            new { Period = TestDateRanges.December2024, TopCount = 10, Description = "Top 10 theaters in December" },
            new { Period = TestDateRanges.Q1_2024, TopCount = 0, Description = "Zero theaters returns empty" },
            new { Period = TestDateRanges.FullYear2024, TopCount = 100, Description = "More than total theaters returns all" }
        };

        foreach (var scenario in testScenarios)
        {
            var baselineResults = _baselineQuery.Execute(scenario.Period, scenario.TopCount);
            var query = new GetTopPerformingTheatersQuery(scenario.Period, scenario.TopCount);
            var queryHandlerResults = ExecuteQuery<GetTopPerformingTheatersQuery, TheaterPerformanceResult>(query);

            TheaterPerformanceAssertions.AssertResultsMatch(
                baselineResults, queryHandlerResults, scenario.Description);
            TheaterPerformanceAssertions.AssertSortedByRevenueDescending(
                queryHandlerResults, scenario.Description);
        }
    }

}
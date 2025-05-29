using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using TrinitySQL.Server.Data;
using TrinitySQL.Server.Domain;
using TrinitySQL.Strategy.MapFilterReduce;
using TrinitySQL.Server.Infrastructure;
using TrinitySQL.Server.Core.SharedKernel;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using TrinitySQL.Blueprint.BaselineQueries;

namespace TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

[TestClass]
public class GetTheatersByDateQueryHandlerTests
{
    private string _dbPath = null!;
    private string _connectionString = null!;
    private TheaterSalesContext _context = null!;
    private IQueryDispatcher _dispatcher = null!;

    [TestInitialize]
    public void Initialize()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"theater_sales_test_{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath}";

        var initializer = new DatabaseInitializer(_connectionString);
        initializer.Initialize();
        initializer.SeedData();

        var optionsBuilder = new DbContextOptionsBuilder<TheaterSalesContext>();
        optionsBuilder.UseSqlite(_connectionString);
        _context = new TheaterSalesContext(optionsBuilder.Options);
        var serviceConfig = new ServiceConfiguration(_context);
        _dispatcher = serviceConfig.QueryDispatcher;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    [TestMethod]
    public void WhenQueryingTheatersForSpecificDate_ShouldReturnAllTheatersIncludingThoseWithNoSales()
    {
        var dateToQuery = new DateOnly(2024, 5, 9);
        var baselineQuery = new TheatersByDateQuery(_context);
        var expectedResults = baselineQuery.Execute(dateToQuery);

        var performanceResults = QueryTheatersForDate(dateToQuery);

        AssertResultsMatchBaseline(expectedResults, performanceResults);
    }

    private List<TheaterPerformanceResult> QueryTheatersForDate(DateOnly date)
    {
        var query = new GetTheatersByDateQuery(date);
        return _dispatcher
            .Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();
    }

    private void AssertResultsMatchBaseline(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            "Should return same number of theaters as baseline");
        
        for (int i = 0; i < baseline.Count; i++)
        {
            Assert.AreEqual(baseline[i].Theater.Name, actual[i].Theater.Name,
                $"Theater at position {i} should match baseline");
            Assert.AreEqual(baseline[i].TotalRevenue, actual[i].TotalRevenue,
                $"Revenue for {baseline[i].Theater.Name} should match baseline");
        }
    }

    [TestMethod]
    public void WhenQueryingDateWithHighSales_ShouldIdentifyTopRevenueTheaterUsingMapReduce()
    {
        var dateWithHighSalesVolume = new DateOnly(2024, 5, 10);
        var baselineQuery = new TheatersByDateQuery(_context);
        var baselineResults = baselineQuery.Execute(dateWithHighSalesVolume);
        var expectedTopPerformer = baselineResults.FirstOrDefault()?.Theater;

        var performanceResults = QueryTheatersForDate(dateWithHighSalesVolume);
        var actualTopPerformer = IdentifyTopPerformerUsingMapReduce(performanceResults);

        AssertTopPerformersMatch(expectedTopPerformer, actualTopPerformer);
    }

    private Theater? IdentifyTopPerformerUsingMapReduce(
        IEnumerable<TheaterPerformanceResult> results)
    {
        return results
            .LazyReduce(
                (TheaterPerformanceResult?)null,
                SelectTheaterWithHigherRevenue,
                IsSignificantPerformer)
            ?.Theater;
    }

    private TheaterPerformanceResult? SelectTheaterWithHigherRevenue(
        TheaterPerformanceResult? champion,
        TheaterPerformanceResult challenger)
    {
        if (champion == null) return challenger;
        return challenger.TotalRevenue > champion.TotalRevenue ? challenger : champion;
    }

    private bool IsSignificantPerformer(TheaterPerformanceResult? result)
    {
        const decimal significantRevenueThreshold = 100000m;
        return result?.TotalRevenue > significantRevenueThreshold;
    }

    private void AssertTopPerformersMatch(Theater? expected, Theater? actual)
    {
        Assert.IsNotNull(actual, "Should find a top performing theater");
        Assert.IsNotNull(expected, "Expected top performer should exist");
        Assert.AreEqual(expected.Name, actual.Name,
            "Map-Reduce should identify the same top performer as direct query");
    }

    [TestMethod]
    public void WhenQueryingTheaters_ShouldReturnResultsOrderedByRevenueHighestToLowest()
    {
        var independenceDayWithHighSales = new DateOnly(2024, 7, 4);
        var baselineQuery = new TheatersByDateQuery(_context);
        var baselineResults = baselineQuery.Execute(independenceDayWithHighSales);

        var performanceResults = QueryTheatersForDate(independenceDayWithHighSales);

        AssertResultsAreInDescendingRevenueOrder(performanceResults);
        AssertOrderMatchesBaseline(baselineResults, performanceResults);
    }

    private void AssertResultsAreInDescendingRevenueOrder(
        List<TheaterPerformanceResult> results)
    {
        for (int i = 1; i < results.Count; i++)
        {
            var higherRankedTheater = results[i - 1];
            var lowerRankedTheater = results[i];

            Assert.IsTrue(
                higherRankedTheater.TotalRevenue >= lowerRankedTheater.TotalRevenue,
                FormatOrderingViolationMessage(i, higherRankedTheater, lowerRankedTheater));
        }
    }

    private string FormatOrderingViolationMessage(
        int position,
        TheaterPerformanceResult higherRanked,
        TheaterPerformanceResult lowerRanked)
    {
        return $"Theater '{higherRanked.Theater.Name}' at position {position - 1} " +
               $"with revenue {higherRanked.TotalRevenue:C} " +
               $"should rank higher than '{lowerRanked.Theater.Name}' at position {position} " +
               $"with revenue {lowerRanked.TotalRevenue:C}";
    }

    private void AssertOrderMatchesBaseline(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual)
    {
        for (int i = 0; i < Math.Min(baseline.Count, actual.Count); i++)
        {
            Assert.AreEqual(baseline[i].Theater.Name, actual[i].Theater.Name,
                $"Theater at position {i} should match baseline order");
        }
    }

}
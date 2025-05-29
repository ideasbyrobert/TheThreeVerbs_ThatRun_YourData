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
public class GetUnderperformingTheatersQueryHandlerTests
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
    public void WhenQueryingWithRevenueThreshold_ShouldReturnOnlyTheatersPerformingAtOrBelowThreshold()
    {
        var performanceDate = new DateOnly(2024, 5, 9);
        const decimal revenueThreshold = 1000m;

        var baselineQuery = new UnderperformingTheatersQuery(_context);
        var expectedUnderperformers = baselineQuery.Execute(performanceDate, revenueThreshold);

        var actualUnderperformers = QueryUnderperformingTheaters(
            performanceDate, revenueThreshold);

        AssertResultsMatchBaseline(expectedUnderperformers, actualUnderperformers, revenueThreshold);
    }

    private List<TheaterPerformanceResult> QueryUnderperformingTheaters(
        DateOnly date, decimal threshold)
    {
        var query = new GetUnderperformingTheatersQuery(date, threshold);
        return _dispatcher
            .Dispatch<GetUnderperformingTheatersQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();
    }

    private void AssertResultsMatchBaseline(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual,
        decimal threshold)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            "Should identify the same number of underperforming theaters as baseline");
        AssertAllTheatersAreAtOrBelowThreshold(actual, threshold);
        AssertEachTheaterMatchesBaseline(baseline, actual);
    }

    private void AssertAllTheatersAreAtOrBelowThreshold(
        List<TheaterPerformanceResult> underperformers,
        decimal threshold)
    {
        foreach (var theater in underperformers)
        {
            Assert.IsTrue(
                theater.TotalRevenue <= threshold,
                FormatThresholdViolationMessage(theater, threshold));
        }
    }

    private string FormatThresholdViolationMessage(
        TheaterPerformanceResult theater,
        decimal threshold)
    {
        return $"Theater '{theater.Theater.Name}' with revenue {theater.TotalRevenue:C} " +
               $"exceeds threshold of {threshold:C}";
    }

    private void AssertEachTheaterMatchesBaseline(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual)
    {
        for (int i = 0; i < actual.Count; i++)
        {
            var baselineTheater = baseline.FirstOrDefault(b => b.Theater.Name == actual[i].Theater.Name);
            
            Assert.IsNotNull(baselineTheater,
                $"Theater '{actual[i].Theater.Name}' should be in baseline underperformers");
            
            Assert.AreEqual(baselineTheater!.TotalRevenue, actual[i].TotalRevenue,
                $"Revenue calculation for '{actual[i].Theater.Name}' should match baseline");
        }
    }

    [TestMethod]
    public void WhenThresholdIsZero_ShouldReturnOnlyTheatersWithNoSalesAtAll()
    {
        var dateWithMinimalSalesActivity = new DateOnly(2024, 1, 15);
        const decimal zeroRevenueThreshold = 0m;

        var baselineQuery = new UnderperformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(dateWithMinimalSalesActivity, zeroRevenueThreshold);
        var actualResults = QueryUnderperformingTheaters(
            dateWithMinimalSalesActivity, zeroRevenueThreshold);

        AssertAllReturnedTheatersHaveZeroRevenue(actualResults);
        Assert.AreEqual(baselineResults.Count, actualResults.Count);
    }

    private void AssertAllReturnedTheatersHaveZeroRevenue(
        IEnumerable<TheaterPerformanceResult> theaters)
    {
        foreach (var theater in theaters)
        {
            Assert.AreEqual(0m, theater.TotalRevenue,
                $"Theater '{theater.Theater.Name}' should have exactly zero revenue");
        }
    }

    [TestMethod]
    public void WhenThresholdExceedsAllPossibleRevenue_ShouldReturnEveryTheater()
    {
        var typicalBusinessDay = new DateOnly(2024, 5, 9);
        const decimal impossiblyHighThreshold = 1000000m;

        var baselineQuery = new UnderperformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(typicalBusinessDay, impossiblyHighThreshold);
        var actualResults = QueryUnderperformingTheaters(
            typicalBusinessDay, impossiblyHighThreshold);

        AssertResultsMatchBaseline(baselineResults, actualResults, impossiblyHighThreshold);
    }

    [TestMethod]
    public void WhenReturningUnderperformers_ShouldOrderByRevenueFromHighestToLowestWithinThreshold()
    {
        var performanceDate = new DateOnly(2024, 5, 9);
        const decimal moderateThreshold = 50000m;

        var baselineQuery = new UnderperformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(performanceDate, moderateThreshold);
        var actualResults = QueryUnderperformingTheaters(
            performanceDate, moderateThreshold);

        AssertUnderperformersAreInDescendingRevenueOrder(actualResults);
        AssertResultsMatchBaseline(baselineResults, actualResults, moderateThreshold);
    }

    private void AssertUnderperformersAreInDescendingRevenueOrder(
        List<TheaterPerformanceResult> underperformers)
    {
        for (int i = 1; i < underperformers.Count; i++)
        {
            var betterPerformer = underperformers[i - 1];
            var worsePerformer = underperformers[i];
            
            Assert.IsTrue(
                betterPerformer.TotalRevenue >= worsePerformer.TotalRevenue,
                FormatOrderingViolationMessage(i, betterPerformer, worsePerformer));
        }
    }

    private string FormatOrderingViolationMessage(
        int position,
        TheaterPerformanceResult betterPerformer,
        TheaterPerformanceResult worsePerformer)
    {
        return $"Theater '{betterPerformer.Theater.Name}' at position {position - 1} " +
               $"with revenue {betterPerformer.TotalRevenue:C} " +
               $"should be listed before '{worsePerformer.Theater.Name}' at position {position} " +
               $"with revenue {worsePerformer.TotalRevenue:C}";
    }
}
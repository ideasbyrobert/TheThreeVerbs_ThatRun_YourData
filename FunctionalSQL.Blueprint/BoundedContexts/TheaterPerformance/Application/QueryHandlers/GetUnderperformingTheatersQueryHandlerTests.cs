using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using FunctionalSQL.Server.Data;
using FunctionalSQL.Server.Domain;
using FunctionalSQL.Strategy.MapFilterReduce;
using FunctionalSQL.Server.Infrastructure;
using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace FunctionalSQL.Blueprint.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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

        var expectedUnderperformers = IdentifyUnderperformersUsingDirectQuery(
            performanceDate, revenueThreshold);

        var actualUnderperformers = QueryUnderperformingTheaters(
            performanceDate, revenueThreshold);

        AssertCorrectUnderperformersIdentified(
            expectedUnderperformers, actualUnderperformers, revenueThreshold);
    }

    private List<TheaterPerformanceResult> QueryUnderperformingTheaters(
        DateOnly date, decimal threshold)
    {
        var query = new GetUnderperformingTheatersQuery(date, threshold);
        return _dispatcher
            .Dispatch<GetUnderperformingTheatersQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();
    }

    private List<dynamic> IdentifyUnderperformersUsingDirectQuery(
        DateOnly date, decimal threshold)
    {
        var allTheaters = LoadAllTheaters();
        var salesAggregatedByTheater = AggregateSalesByTheaterForDate(date);

        return FilterTheatersPerformingBelowThreshold(
            allTheaters, salesAggregatedByTheater, threshold);
    }

    private List<Theater> LoadAllTheaters()
    {
        return _context.Theaters.ToList();
    }

    private List<dynamic> AggregateSalesByTheaterForDate(DateOnly date)
    {
        return _context.Sales
            .Where(sale => sale.SaleDate == date)
            .GroupBy(sale => sale.TheaterId)
            .Select(group => new 
            { 
                TheaterId = group.Key, 
                TotalRevenue = group.Sum(sale => sale.Amount) 
            })
            .ToList<dynamic>();
    }

    private List<dynamic> FilterTheatersPerformingBelowThreshold(
        List<Theater> allTheaters,
        List<dynamic> salesByTheater,
        decimal threshold)
    {
        return allTheaters
            .Select(theater => CreateTheaterPerformanceRecord(theater, salesByTheater))
            .Where(performance => IsUnderperforming(performance, threshold))
            .ToList<dynamic>();
    }

    private dynamic CreateTheaterPerformanceRecord(
        Theater theater, 
        List<dynamic> salesByTheater)
    {
        return new
        {
            Theater = theater,
            TotalRevenue = CalculateTheaterRevenue(theater.Id, salesByTheater)
        };
    }

    private decimal CalculateTheaterRevenue(int theaterId, List<dynamic> salesByTheater)
    {
        var theaterSales = salesByTheater.FirstOrDefault(s => s.TheaterId == theaterId);
        return theaterSales?.TotalRevenue ?? 0m;
    }

    private bool IsUnderperforming(dynamic performance, decimal threshold)
    {
        return performance.TotalRevenue <= threshold;
    }

    private void AssertCorrectUnderperformersIdentified(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual,
        decimal threshold)
    {
        Assert.AreEqual(expected.Count, actual.Count,
            "Should identify the same number of underperforming theaters");
        AssertAllTheatersAreAtOrBelowThreshold(actual, threshold);
        AssertEachIdentifiedTheaterMatchesExpected(expected, actual);
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

    private void AssertEachIdentifiedTheaterMatchesExpected(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual)
    {
        foreach (var actualTheater in actual)
        {
            var expectedMatch = FindMatchingTheater(expected, actualTheater.Theater.Name);
            
            Assert.IsNotNull(expectedMatch,
                $"Theater '{actualTheater.Theater.Name}' should be in expected underperformers");
            
            Assert.AreEqual(expectedMatch!.TotalRevenue, actualTheater.TotalRevenue,
                $"Revenue calculation for '{actualTheater.Theater.Name}' should match");
        }
    }

    private dynamic? FindMatchingTheater(List<dynamic> theaters, string theaterName)
    {
        return theaters.FirstOrDefault(t => t.Theater.Name == theaterName);
    }

    [TestMethod]
    public void WhenThresholdIsZero_ShouldReturnOnlyTheatersWithNoSalesAtAll()
    {
        var dateWithMinimalSalesActivity = new DateOnly(2024, 1, 15);
        const decimal zeroRevenueThreshold = 0m;

        var theatersWithNoSales = QueryUnderperformingTheaters(
            dateWithMinimalSalesActivity, zeroRevenueThreshold);

        AssertAllReturnedTheatersHaveZeroRevenue(theatersWithNoSales);
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
        var totalTheatersInDatabase = CountTotalTheaters();

        var allTheaters = QueryUnderperformingTheaters(
            typicalBusinessDay, impossiblyHighThreshold);

        AssertReturnsCompleteTheaterList(allTheaters, totalTheatersInDatabase);
    }

    private int CountTotalTheaters()
    {
        return _context.Theaters.Count();
    }

    private void AssertReturnsCompleteTheaterList(
        IEnumerable<TheaterPerformanceResult> results,
        int expectedCount)
    {
        Assert.AreEqual(expectedCount, results.Count(),
            "Very high threshold should include all theaters as underperformers");
    }

    [TestMethod]
    public void WhenReturningUnderperformers_ShouldOrderByRevenueFromHighestToLowestWithinThreshold()
    {
        var performanceDate = new DateOnly(2024, 5, 9);
        const decimal moderateThreshold = 50000m;

        var underperformingTheaters = QueryUnderperformingTheaters(
            performanceDate, moderateThreshold);

        AssertUnderperformersAreInDescendingRevenueOrder(underperformingTheaters);
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
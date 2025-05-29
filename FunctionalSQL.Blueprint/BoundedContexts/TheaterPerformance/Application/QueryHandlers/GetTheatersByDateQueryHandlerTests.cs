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
        var expectedTheatersWithRevenue = CalculateExpectedRevenueForAllTheaters(dateToQuery);

        var performanceResults = QueryTheatersForDate(dateToQuery);

        AssertAllTheatersArePresent(expectedTheatersWithRevenue, performanceResults);
        AssertRevenueCalculationsAreCorrect(expectedTheatersWithRevenue, performanceResults);
    }

    private List<TheaterPerformanceResult> QueryTheatersForDate(DateOnly date)
    {
        var query = new GetTheatersByDateQuery(date);
        return _dispatcher
            .Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();
    }

    private List<dynamic> CalculateExpectedRevenueForAllTheaters(DateOnly date)
    {
        var allTheaters = LoadAllTheaters();
        var salesGroupedByTheater = GroupSalesByTheaterForDate(date);

        return CombineTheatersWithTheirRevenue(allTheaters, salesGroupedByTheater);
    }

    private List<Theater> LoadAllTheaters()
    {
        return _context.Theaters.ToList();
    }

    private List<dynamic> GroupSalesByTheaterForDate(DateOnly date)
    {
        return _context.Sales
            .Where(s => s.SaleDate == date)
            .GroupBy(s => s.TheaterId)
            .Select(g => new { TheaterId = g.Key, TotalRevenue = g.Sum(s => s.Amount) })
            .ToList<dynamic>();
    }

    private List<dynamic> CombineTheatersWithTheirRevenue(
        List<Theater> theaters,
        List<dynamic> salesByTheater)
    {
        return theaters
            .Select(theater => new
            {
                Theater = theater,
                TotalRevenue = FindRevenueForTheater(theater.Id, salesByTheater)
            })
            .ToList<dynamic>();
    }

    private decimal FindRevenueForTheater(int theaterId, List<dynamic> salesByTheater)
    {
        var theaterSales = salesByTheater.FirstOrDefault(s => s.TheaterId == theaterId);
        return theaterSales?.TotalRevenue ?? 0m;
    }

    private void AssertAllTheatersArePresent(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual)
    {
        var totalTheatersInDatabase = CountTotalTheaters();
        Assert.AreEqual(totalTheatersInDatabase, actual.Count,
            "Query should return all theaters regardless of sales");
        Assert.IsTrue(actual.All(HasNonNegativeRevenue),
            "All theaters should have non-negative revenue");
    }

    private int CountTotalTheaters()
    {
        return _context.Theaters.Count();
    }

    private bool HasNonNegativeRevenue(TheaterPerformanceResult result)
    {
        return result.TotalRevenue >= 0;
    }

    private void AssertRevenueCalculationsAreCorrect(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual)
    {
        foreach (var actualResult in actual)
        {
            var expectedResult = FindMatchingTheater(expected, actualResult.Theater.Name);
            Assert.IsNotNull(expectedResult,
                $"Theater {actualResult.Theater.Name} should exist in expected results");
            Assert.AreEqual(expectedResult!.TotalRevenue, actualResult.TotalRevenue,
                $"Revenue for {actualResult.Theater.Name} should match expected");
        }
    }

    private dynamic? FindMatchingTheater(List<dynamic> theaters, string theaterName)
    {
        return theaters.FirstOrDefault(t => t.Theater.Name == theaterName);
    }

    [TestMethod]
    public void WhenQueryingDateWithHighSales_ShouldIdentifyTopRevenueTheaterUsingMapReduce()
    {
        var dateWithHighSalesVolume = new DateOnly(2024, 5, 10);
        var expectedTopPerformer = IdentifyTopPerformerUsingDirectQuery(dateWithHighSalesVolume);

        var performanceResults = QueryTheatersForDate(dateWithHighSalesVolume);
        var actualTopPerformer = IdentifyTopPerformerUsingMapReduce(performanceResults);

        AssertTopPerformersMatch(expectedTopPerformer, actualTopPerformer);
    }

    private Theater? IdentifyTopPerformerUsingDirectQuery(DateOnly date)
    {
        var salesWithTheaters = JoinSalesWithTheaters(date);
        
        return salesWithTheaters
            .GroupBy(x => x.Theater)
            .Select(g => new
            {
                Theater = g.Key,
                TotalRevenue = g.Sum(x => (decimal)x.Sale.Amount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .FirstOrDefault()
            ?.Theater;
    }

    private IEnumerable<dynamic> JoinSalesWithTheaters(DateOnly date)
    {
        return _context.Sales
            .Where(s => s.SaleDate == date)
            .Join(_context.Theaters,
                s => s.TheaterId,
                t => t.Id,
                (s, t) => new { Sale = s, Theater = t })
            .ToList();
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

        var performanceResults = QueryTheatersForDate(independenceDayWithHighSales);

        AssertResultsAreInDescendingRevenueOrder(performanceResults);
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

}
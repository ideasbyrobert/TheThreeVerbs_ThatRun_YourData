using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using TheaterSales.DotNet.Data;
using TheaterSales.DotNet.Domain;
using TheaterSales.Extended.MapFilterReduce;
using TheaterSales.DotNet.Infrastructure;
using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Application.Queries;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TheaterSales.Blueprint.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

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
    public void Handle_WithSpecificDate_ReturnsAllTheatersIncludingZeroRevenue()
    {
        var queryDate = new DateOnly(2024, 5, 9);

        var expectedTheaters = BuildExpectedTheatersWithRevenue(queryDate);

        var query = new GetTheatersByDateQuery(queryDate);
        var results = _dispatcher
            .Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();

        AssertAllTheatersAreIncluded(expectedTheaters, results);
        AssertEachTheaterRevenueMatches(expectedTheaters, results);
    }

    private List<dynamic> BuildExpectedTheatersWithRevenue(DateOnly date)
    {
        var allTheaters = _context.Theaters.ToList();
        var salesByTheater = _context.Sales
            .Where(s => s.SaleDate == date)
            .GroupBy(s => s.TheaterId)
            .Select(g => new { TheaterId = g.Key, TotalRevenue = g.Sum(s => s.Amount) })
            .ToList();

        return allTheaters
            .Select(theater => new
            {
                Theater = theater,
                TotalRevenue = salesByTheater
                    .FirstOrDefault(s => s.TheaterId == theater.Id)?.TotalRevenue ?? 0m
            })
            .ToList<dynamic>();
    }

    private void AssertAllTheatersAreIncluded(
        List<dynamic> expectedTheaters,
        List<TheaterPerformanceResult> actualResults)
    {
        var allTheaters = _context.Theaters.ToList();
        Assert.AreEqual(expectedTheaters.Count, actualResults.Count);
        Assert.AreEqual(allTheaters.Count, actualResults.Count);
        Assert.IsTrue(actualResults.All(t => t.TotalRevenue >= 0));
    }

    private void AssertEachTheaterRevenueMatches(
        List<dynamic> expectedTheaters,
        List<TheaterPerformanceResult> actualResults)
    {
        foreach (var actual in actualResults)
        {
            var expected = expectedTheaters
                .FirstOrDefault(e => e.Theater.Name == actual.Theater.Name);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected!.TotalRevenue, actual.TotalRevenue);
        }
    }

    [TestMethod]
    public void Handle_WithHighSalesDate_FindsHighestRevenueTheater()
    {
        var highSalesDate = new DateOnly(2024, 5, 10);

        var expectedTopTheater = FindExpectedTopRevenueTheater(highSalesDate);

        var query = new GetTheatersByDateQuery(highSalesDate);
        var results = _dispatcher
            .Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query);

        var actualTopTheater = FindHighestRevenueTheaterUsingMapReduce(results);

        AssertTopTheaterMatches(expectedTopTheater, actualTopTheater);
    }

    private Theater? FindExpectedTopRevenueTheater(DateOnly date)
    {
        return _context.Sales
            .Where(s => s.SaleDate == date)
            .Join(_context.Theaters,
                s => s.TheaterId,
                t => t.Id,
                (s, t) => new { Sale = s, Theater = t })
            .GroupBy(x => x.Theater)
            .Select(g => new
            {
                Theater = g.Key,
                TotalRevenue = g.Sum(x => x.Sale.Amount)
            })
            .ToList()
            .OrderByDescending(x => x.TotalRevenue)
            .FirstOrDefault()
            ?.Theater;
    }

    private Theater? FindHighestRevenueTheaterUsingMapReduce(
        IEnumerable<TheaterPerformanceResult> results)
    {
        return results
            .LazyReduce(
                (TheaterPerformanceResult?)null,
                SelectHigherRevenueTheater,
                HasSignificantRevenue)
            ?.Theater;
    }

    private TheaterPerformanceResult? SelectHigherRevenueTheater(
        TheaterPerformanceResult? current,
        TheaterPerformanceResult next)
    {
        return current == null || next.TotalRevenue > current.TotalRevenue
            ? next
            : current;
    }

    private bool HasSignificantRevenue(TheaterPerformanceResult? result)
    {
        return result?.TotalRevenue > 100000m;
    }

    private void AssertTopTheaterMatches(Theater? expected, Theater? actual)
    {
        Assert.IsNotNull(actual);
        Assert.IsNotNull(expected);
        Assert.AreEqual(expected.Name, actual.Name);
    }

    [TestMethod]
    public void Handle_ReturnsResultsSortedByRevenueDescending()
    {
        var highRevenueDate = new DateOnly(2024, 7, 4);

        var query = new GetTheatersByDateQuery(highRevenueDate);
        var results = _dispatcher
            .Dispatch<GetTheatersByDateQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();

        AssertResultsAreSortedDescendingByRevenue(results);
    }

    private void AssertResultsAreSortedDescendingByRevenue(
        List<TheaterPerformanceResult> results)
    {
        for (int i = 1; i < results.Count; i++)
        {
            var previousRevenue = results[i - 1].TotalRevenue;
            var currentRevenue = results[i].TotalRevenue;

            Assert.IsTrue(
                previousRevenue >= currentRevenue,
                BuildSortingErrorMessage(i, previousRevenue, currentRevenue));
        }
    }

    private string BuildSortingErrorMessage(
        int index,
        decimal previousRevenue,
        decimal currentRevenue)
    {
        return $"Theater at index {index - 1} has revenue {previousRevenue} " +
               $"which should be >= theater at index {index} with revenue {currentRevenue}";
    }

}
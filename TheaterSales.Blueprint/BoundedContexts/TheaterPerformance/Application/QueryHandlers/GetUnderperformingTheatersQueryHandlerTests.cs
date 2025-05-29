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
    public void Handle_WithThreshold_ReturnsTheatersWithRevenueBelowOrEqualThreshold()
    {
        var date = new DateOnly(2024, 5, 9);
        var threshold = 1000m;

        var expectedUnderperformers = CalculateExpectedUnderperformers(date, threshold);

        var query = new GetUnderperformingTheatersQuery(date, threshold);
        var results = _dispatcher.Dispatch<GetUnderperformingTheatersQuery,
            IEnumerable<TheaterPerformanceResult>>(query);
        var underperformers = results.ToList();

        AssertUnderperformersMatch(expectedUnderperformers, underperformers, threshold);
    }

    private List<dynamic> CalculateExpectedUnderperformers(DateOnly date, decimal threshold)
    {
        var allTheaters = _context.Theaters.ToList();
        var salesByTheater = CalculateSalesByTheater(date);

        return BuildUnderperformingTheaters(allTheaters, salesByTheater, threshold);
    }

    private List<dynamic> CalculateSalesByTheater(DateOnly date)
    {
        return _context.Sales
            .Where(s => s.SaleDate == date)
            .GroupBy(s => s.TheaterId)
            .Select(g => new { TheaterId = g.Key, TotalRevenue = g.Sum(s => s.Amount) })
            .ToList<dynamic>();
    }

    private List<dynamic> BuildUnderperformingTheaters(
        List<Theater> allTheaters,
        List<dynamic> salesByTheater,
        decimal threshold)
    {
        return allTheaters
            .Select(theater => new
            {
                Theater = theater,
                TotalRevenue = GetTheaterRevenue(theater.Id, salesByTheater)
            })
            .Where(x => x.TotalRevenue <= threshold)
            .ToList<dynamic>();
    }

    private decimal GetTheaterRevenue(int theaterId, List<dynamic> salesByTheater)
    {
        var sale = salesByTheater.FirstOrDefault(s => s.TheaterId == theaterId);
        return sale?.TotalRevenue ?? 0m;
    }

    private void AssertUnderperformersMatch(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual,
        decimal threshold)
    {
        Assert.AreEqual(expected.Count, actual.Count);
        AssertAllBelowThreshold(actual, threshold);
        AssertEachUnderperformerMatches(expected, actual);
    }

    private void AssertAllBelowThreshold(
        List<TheaterPerformanceResult> underperformers,
        decimal threshold)
    {
        Assert.IsTrue(underperformers.All(t => t.TotalRevenue <= threshold));
    }

    private void AssertEachUnderperformerMatches(
        List<dynamic> expected,
        List<TheaterPerformanceResult> actual)
    {
        foreach (var underperformer in actual)
        {
            var expectedMatch = expected.FirstOrDefault(
                e => e.Theater.Name == underperformer.Theater.Name);
            Assert.IsNotNull(expectedMatch);
            Assert.AreEqual(expectedMatch!.TotalRevenue, underperformer.TotalRevenue);
        }
    }

    [TestMethod]
    public void Handle_WithZeroThreshold_ReturnsOnlyTheatersWithZeroRevenue()
    {
        var dateWithLimitedSales = new DateOnly(2024, 1, 15);
        var threshold = 0m;

        var query = new GetUnderperformingTheatersQuery(dateWithLimitedSales, threshold);
        var results = _dispatcher.Dispatch<GetUnderperformingTheatersQuery,
            IEnumerable<TheaterPerformanceResult>>(query);

        AssertAllTheatersHaveZeroRevenue(results);
    }

    private void AssertAllTheatersHaveZeroRevenue(
        IEnumerable<TheaterPerformanceResult> results)
    {
        Assert.IsTrue(results.All(t => t.TotalRevenue == 0m));
    }

    [TestMethod]
    public void Handle_WithHighThreshold_ReturnsAllTheaters()
    {
        var date = new DateOnly(2024, 5, 9);
        var veryHighThreshold = 1000000m;
        var totalTheaters = _context.Theaters.Count();

        var query = new GetUnderperformingTheatersQuery(date, veryHighThreshold);
        var results = _dispatcher.Dispatch<GetUnderperformingTheatersQuery,
            IEnumerable<TheaterPerformanceResult>>(query);

        AssertAllTheatersReturned(results, totalTheaters);
    }

    private void AssertAllTheatersReturned(
        IEnumerable<TheaterPerformanceResult> results,
        int expectedCount)
    {
        Assert.AreEqual(expectedCount, results.Count());
    }

    [TestMethod]
    public void Handle_ReturnsResultsSortedByRevenueDescending()
    {
        var date = new DateOnly(2024, 5, 9);
        var threshold = 50000m;

        var query = new GetUnderperformingTheatersQuery(date, threshold);
        var results = _dispatcher.Dispatch<GetUnderperformingTheatersQuery,
            IEnumerable<TheaterPerformanceResult>>(query);
        var underperformers = results.ToList();

        AssertResultsSortedByRevenueDescending(underperformers);
    }

    private void AssertResultsSortedByRevenueDescending(
        List<TheaterPerformanceResult> underperformers)
    {
        if (underperformers.Count > 1)
        {
            for (int i = 1; i < underperformers.Count; i++)
            {
                var previousRevenue = underperformers[i - 1].TotalRevenue;
                var currentRevenue = underperformers[i].TotalRevenue;
                AssertDescendingOrder(previousRevenue, currentRevenue);
            }
        }
    }

    private void AssertDescendingOrder(decimal previousRevenue, decimal currentRevenue)
    {
        Assert.IsTrue(previousRevenue >= currentRevenue,
            $"Results should be sorted descending by revenue");
    }
}
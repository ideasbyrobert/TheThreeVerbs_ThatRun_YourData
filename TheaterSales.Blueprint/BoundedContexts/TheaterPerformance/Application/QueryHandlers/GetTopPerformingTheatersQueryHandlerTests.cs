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
public class GetTopPerformingTheatersQueryHandlerTests
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
    public void Handle_WithTopCount3_ReturnsTop3TheatersIncludingZeroRevenue()
    {
        var startDate = new DateOnly(2024, 5, 1);
        var endDate = new DateOnly(2024, 5, 31);
        
        var expectedTopTheaters = CalculateExpectedTopTheaters(startDate, endDate, 3);
        
        var query = new GetTopPerformingTheatersQuery(new DateRange(startDate, endDate), 3);
        var results = _dispatcher.Dispatch<GetTopPerformingTheatersQuery, 
            IEnumerable<TheaterPerformanceResult>>(query);
        var topTheaters = results.ToList();
        
        AssertTopTheatersMatch(expectedTopTheaters, topTheaters);
        AssertCorrectOrdering(topTheaters);
    }
    
    private List<dynamic> CalculateExpectedTopTheaters(
        DateOnly startDate, 
        DateOnly endDate, 
        int topCount)
    {
        var allTheaters = _context.Theaters.ToList();
        var salesByTheater = CalculateSalesByTheaterForPeriod(startDate, endDate);
        
        return BuildTopTheaters(allTheaters, salesByTheater, topCount);
    }
    
    private List<dynamic> CalculateSalesByTheaterForPeriod(
        DateOnly startDate, 
        DateOnly endDate)
    {
        return _context.Sales
            .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
            .GroupBy(s => s.TheaterId)
            .Select(g => new { TheaterId = g.Key, TotalRevenue = g.Sum(s => s.Amount) })
            .ToList<dynamic>();
    }
    
    private List<dynamic> BuildTopTheaters(
        List<Theater> allTheaters, 
        List<dynamic> salesByTheater, 
        int topCount)
    {
        return allTheaters
            .Select(theater => new
            {
                Theater = theater,
                TotalRevenue = GetTheaterRevenueFromSales(theater.Id, salesByTheater)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(topCount)
            .ToList<dynamic>();
    }
    
    private decimal GetTheaterRevenueFromSales(int theaterId, List<dynamic> salesByTheater)
    {
        var sale = salesByTheater.FirstOrDefault(s => s.TheaterId == theaterId);
        return sale?.TotalRevenue ?? 0m;
    }
    
    private void AssertTopTheatersMatch(
        List<dynamic> expected, 
        List<TheaterPerformanceResult> actual)
    {
        Assert.AreEqual(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual(expected[i].Theater.Name, actual[i].Theater.Name);
            Assert.AreEqual(expected[i].TotalRevenue, actual[i].TotalRevenue);
        }
    }
    
    private void AssertCorrectOrdering(List<TheaterPerformanceResult> topTheaters)
    {
        if (topTheaters.Count >= 2)
        {
            Assert.IsTrue(topTheaters[0].TotalRevenue >= topTheaters[1].TotalRevenue);
        }
        if (topTheaters.Count >= 3)
        {
            Assert.IsTrue(topTheaters[1].TotalRevenue >= topTheaters[2].TotalRevenue);
        }
    }

    [TestMethod]
    public void Handle_WithTopCount0_ReturnsEmptyCollection()
    {
        var startDate = new DateOnly(2024, 5, 1);
        var endDate = new DateOnly(2024, 5, 31);
        
        var query = new GetTopPerformingTheatersQuery(new DateRange(startDate, endDate), 0);
        var results = _dispatcher.Dispatch<GetTopPerformingTheatersQuery, 
            IEnumerable<TheaterPerformanceResult>>(query);
        
        AssertEmptyResults(results);
    }
    
    private void AssertEmptyResults(IEnumerable<TheaterPerformanceResult> results)
    {
        Assert.AreEqual(0, results.Count());
    }

    [TestMethod]
    public void Handle_WithTopCountGreaterThanAvailable_ReturnsAllTheaters()
    {
        var startDate = new DateOnly(2024, 5, 1);
        var endDate = new DateOnly(2024, 5, 31);
        var totalTheaters = _context.Theaters.Count();
        
        var query = new GetTopPerformingTheatersQuery(new DateRange(startDate, endDate), 100);
        var results = _dispatcher.Dispatch<GetTopPerformingTheatersQuery, 
            IEnumerable<TheaterPerformanceResult>>(query);
        
        AssertAllTheatersIncluded(results, totalTheaters);
    }
    
    private void AssertAllTheatersIncluded(
        IEnumerable<TheaterPerformanceResult> results, 
        int expectedCount)
    {
        Assert.AreEqual(expectedCount, results.Count());
    }

    [TestMethod]
    public void Handle_WithDateRange_FiltersCorrectly()
    {
        var startDate = new DateOnly(2024, 7, 1);
        var endDate = new DateOnly(2024, 7, 31);
        
        var query = new GetTopPerformingTheatersQuery(new DateRange(startDate, endDate), 5);
        var results = _dispatcher.Dispatch<GetTopPerformingTheatersQuery, 
            IEnumerable<TheaterPerformanceResult>>(query);
        
        AssertResultsExist(results);
    }
    
    private void AssertResultsExist(IEnumerable<TheaterPerformanceResult> results)
    {
        Assert.IsTrue(results.Any());
    }

}
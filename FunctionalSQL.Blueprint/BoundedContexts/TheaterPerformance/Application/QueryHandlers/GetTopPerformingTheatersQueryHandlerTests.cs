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
    public void WhenRequestingTop3Theaters_ShouldReturnExactly3HighestPerformersInDescendingOrder()
    {
        var mayPerformancePeriod = CreateDateRangeForMay2024();
        const int requestedTopCount = 3;
        
        var expectedTopPerformers = CalculateTopPerformersUsingDirectQuery(
            mayPerformancePeriod, requestedTopCount);
        
        var actualTopPerformers = QueryTopPerformingTheaters(
            mayPerformancePeriod, requestedTopCount);
        
        AssertCorrectTheatersSelected(expectedTopPerformers, actualTopPerformers);
        AssertDescendingRevenueOrder(actualTopPerformers);
    }

    private DateRange CreateDateRangeForMay2024()
    {
        return new DateRange(
            new DateOnly(2024, 5, 1),
            new DateOnly(2024, 5, 31));
    }
    
    private List<TheaterPerformanceResult> QueryTopPerformingTheaters(
        DateRange period, int topCount)
    {
        var query = new GetTopPerformingTheatersQuery(period, topCount);
        return _dispatcher
            .Dispatch<GetTopPerformingTheatersQuery, IEnumerable<TheaterPerformanceResult>>(query)
            .ToList();
    }

    private List<dynamic> CalculateTopPerformersUsingDirectQuery(
        DateRange period, int topCount)
    {
        var allTheaters = LoadAllTheaters();
        var salesAggregatedByTheater = AggregateSalesByTheaterForPeriod(period);
        
        return SelectTopPerformers(allTheaters, salesAggregatedByTheater, topCount);
    }

    private List<Theater> LoadAllTheaters()
    {
        return _context.Theaters.ToList();
    }
    
    private List<dynamic> AggregateSalesByTheaterForPeriod(DateRange period)
    {
        return _context.Sales
            .Where(sale => sale.SaleDate >= period.StartDate && sale.SaleDate <= period.EndDate)
            .GroupBy(sale => sale.TheaterId)
            .Select(group => new 
            { 
                TheaterId = group.Key, 
                TotalRevenue = group.Sum(sale => sale.Amount) 
            })
            .ToList<dynamic>();
    }
    
    private List<dynamic> SelectTopPerformers(
        List<Theater> allTheaters, 
        List<dynamic> salesByTheater, 
        int topCount)
    {
        return allTheaters
            .Select(theater => CreateTheaterPerformance(theater, salesByTheater))
            .OrderByDescending(performance => performance.TotalRevenue)
            .Take(topCount)
            .ToList<dynamic>();
    }

    private dynamic CreateTheaterPerformance(Theater theater, List<dynamic> salesByTheater)
    {
        return new
        {
            Theater = theater,
            TotalRevenue = FindTheaterRevenue(theater.Id, salesByTheater)
        };
    }
    
    private decimal FindTheaterRevenue(int theaterId, List<dynamic> salesByTheater)
    {
        var theaterSales = salesByTheater.FirstOrDefault(s => s.TheaterId == theaterId);
        return theaterSales?.TotalRevenue ?? 0m;
    }
    
    private void AssertCorrectTheatersSelected(
        List<dynamic> expected, 
        List<TheaterPerformanceResult> actual)
    {
        Assert.AreEqual(expected.Count, actual.Count,
            "Should return exactly the requested number of top theaters");
        
        for (int rank = 0; rank < expected.Count; rank++)
        {
            AssertTheaterAtRankMatches(expected[rank], actual[rank], rank + 1);
        }
    }

    private void AssertTheaterAtRankMatches(
        dynamic expected, 
        TheaterPerformanceResult actual, 
        int rank)
    {
        Assert.AreEqual(expected.Theater.Name, actual.Theater.Name,
            $"Theater at rank {rank} should match expected");
        Assert.AreEqual(expected.TotalRevenue, actual.TotalRevenue,
            $"Revenue for theater at rank {rank} should match expected");
    }
    
    private void AssertDescendingRevenueOrder(List<TheaterPerformanceResult> theaters)
    {
        for (int i = 1; i < theaters.Count; i++)
        {
            var higherRanked = theaters[i - 1];
            var lowerRanked = theaters[i];
            
            Assert.IsTrue(
                higherRanked.TotalRevenue >= lowerRanked.TotalRevenue,
                FormatRankingViolationMessage(i, higherRanked, lowerRanked));
        }
    }

    private string FormatRankingViolationMessage(
        int position,
        TheaterPerformanceResult higherRanked,
        TheaterPerformanceResult lowerRanked)
    {
        return $"Theater '{higherRanked.Theater.Name}' at rank {position} " +
               $"with revenue {higherRanked.TotalRevenue:C} " +
               $"should rank higher than '{lowerRanked.Theater.Name}' at rank {position + 1} " +
               $"with revenue {lowerRanked.TotalRevenue:C}";
    }

    [TestMethod]
    public void WhenRequestingZeroTopTheaters_ShouldReturnEmptyCollection()
    {
        var mayPerformancePeriod = CreateDateRangeForMay2024();
        const int zeroTheaters = 0;
        
        var results = QueryTopPerformingTheaters(mayPerformancePeriod, zeroTheaters);
        
        AssertCollectionIsEmpty(results);
    }
    
    private void AssertCollectionIsEmpty(IEnumerable<TheaterPerformanceResult> results)
    {
        Assert.AreEqual(0, results.Count(),
            "Requesting zero theaters should return empty collection");
    }

    [TestMethod]
    public void WhenRequestingMoreTheatersThanExist_ShouldReturnAllAvailableTheaters()
    {
        var mayPerformancePeriod = CreateDateRangeForMay2024();
        const int unrealisticallyHighCount = 100;
        var totalTheatersInDatabase = CountTotalTheaters();
        
        var results = QueryTopPerformingTheaters(mayPerformancePeriod, unrealisticallyHighCount);
        
        AssertReturnsExactlyAllTheaters(results, totalTheatersInDatabase);
    }

    private int CountTotalTheaters()
    {
        return _context.Theaters.Count();
    }
    
    private void AssertReturnsExactlyAllTheaters(
        IEnumerable<TheaterPerformanceResult> results, 
        int expectedCount)
    {
        Assert.AreEqual(expectedCount, results.Count(),
            "Should return all theaters when requested count exceeds available");
    }

    [TestMethod]
    public void WhenQueryingJulyPerformance_ShouldReturnTopTheatersForThatPeriodOnly()
    {
        var julyPerformancePeriod = CreateDateRangeForJuly2024();
        const int topFiveTheaters = 5;
        
        var julyTopPerformers = QueryTopPerformingTheaters(
            julyPerformancePeriod, topFiveTheaters);
        
        AssertResultsExistForPeriod(julyTopPerformers);
    }

    private DateRange CreateDateRangeForJuly2024()
    {
        return new DateRange(
            new DateOnly(2024, 7, 1),
            new DateOnly(2024, 7, 31));
    }
    
    private void AssertResultsExistForPeriod(IEnumerable<TheaterPerformanceResult> results)
    {
        Assert.IsTrue(results.Any(),
            "Should find top performing theaters for the specified period");
    }

}
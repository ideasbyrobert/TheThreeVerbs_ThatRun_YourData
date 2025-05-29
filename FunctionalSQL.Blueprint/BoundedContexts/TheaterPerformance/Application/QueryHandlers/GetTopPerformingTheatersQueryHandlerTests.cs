using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using FunctionalSQL.Server.Data;
using FunctionalSQL.Server.Domain;
using FunctionalSQL.Strategy.MapFilterReduce;
using FunctionalSQL.Server.Infrastructure;
using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Application.Queries;
using FunctionalSQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;
using FunctionalSQL.Blueprint.BaselineQueries;

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
        
        var baselineQuery = new TopPerformingTheatersQuery(_context);
        var expectedTopPerformers = baselineQuery.Execute(mayPerformancePeriod, requestedTopCount);
        
        var actualTopPerformers = QueryTopPerformingTheaters(
            mayPerformancePeriod, requestedTopCount);
        
        AssertResultsMatchBaseline(expectedTopPerformers, actualTopPerformers);
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

    private void AssertResultsMatchBaseline(
        List<TheaterPerformanceResult> baseline, 
        List<TheaterPerformanceResult> actual)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            "Should return exactly the requested number of top theaters");
        
        for (int rank = 0; rank < baseline.Count; rank++)
        {
            Assert.AreEqual(baseline[rank].Theater.Name, actual[rank].Theater.Name,
                $"Theater at rank {rank + 1} should match baseline");
            Assert.AreEqual(baseline[rank].TotalRevenue, actual[rank].TotalRevenue,
                $"Revenue for theater at rank {rank + 1} should match baseline");
        }
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
        
        var baselineQuery = new TopPerformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(mayPerformancePeriod, zeroTheaters);
        var actualResults = QueryTopPerformingTheaters(mayPerformancePeriod, zeroTheaters);
        
        AssertCollectionIsEmpty(actualResults);
        Assert.AreEqual(baselineResults.Count, actualResults.Count);
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
        
        var baselineQuery = new TopPerformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(mayPerformancePeriod, unrealisticallyHighCount);
        var actualResults = QueryTopPerformingTheaters(mayPerformancePeriod, unrealisticallyHighCount);
        
        AssertResultsMatchBaseline(baselineResults, actualResults);
    }

    [TestMethod]
    public void WhenQueryingJulyPerformance_ShouldReturnTopTheatersForThatPeriodOnly()
    {
        var julyPerformancePeriod = CreateDateRangeForJuly2024();
        const int topFiveTheaters = 5;
        
        var baselineQuery = new TopPerformingTheatersQuery(_context);
        var baselineResults = baselineQuery.Execute(julyPerformancePeriod, topFiveTheaters);
        var actualResults = QueryTopPerformingTheaters(julyPerformancePeriod, topFiveTheaters);
        
        AssertResultsExistForPeriod(actualResults);
        AssertResultsMatchBaseline(baselineResults, actualResults);
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
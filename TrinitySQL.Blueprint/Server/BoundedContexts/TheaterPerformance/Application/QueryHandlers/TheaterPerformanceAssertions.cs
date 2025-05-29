using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Blueprint.Server.BoundedContexts.TheaterPerformance.Application.QueryHandlers;

public static class TheaterPerformanceAssertions
{
    public static void AssertResultsMatch(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual,
        string scenario)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            $"{scenario}: Count mismatch");

        for (int i = 0; i < baseline.Count; i++)
        {
            Assert.AreEqual(baseline[i].Theater.Name, actual[i].Theater.Name,
                $"{scenario}: Theater name mismatch at position {i}");
            Assert.AreEqual(baseline[i].TotalRevenue, actual[i].TotalRevenue,
                $"{scenario}: Revenue mismatch for {baseline[i].Theater.Name}");
        }
    }

    public static void AssertResultsMatchByTheater(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual,
        string context)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            $"{context}: Count mismatch");

        var baselineByTheater = baseline.ToDictionary(r => r.Theater.Name);
        var actualByTheater = actual.ToDictionary(r => r.Theater.Name);

        foreach (var theaterName in baselineByTheater.Keys)
        {
            Assert.IsTrue(actualByTheater.ContainsKey(theaterName),
                $"{context}: Missing theater {theaterName}");

            Assert.AreEqual(
                baselineByTheater[theaterName].TotalRevenue,
                actualByTheater[theaterName].TotalRevenue,
                $"{context}: Revenue mismatch for {theaterName}");
        }
    }

    public static void AssertResultsMatchAsSet(
        List<TheaterPerformanceResult> baseline,
        List<TheaterPerformanceResult> actual,
        string scenario)
    {
        Assert.AreEqual(baseline.Count, actual.Count,
            $"{scenario}: Count mismatch");

        var baselineSet = new HashSet<string>(baseline.Select(r => r.Theater.Name));
        var actualSet = new HashSet<string>(actual.Select(r => r.Theater.Name));

        Assert.IsTrue(baselineSet.SetEquals(actualSet),
            $"{scenario}: Theater sets don't match");

        foreach (var result in actual)
        {
            var baselineResult = baseline.First(b => b.Theater.Name == result.Theater.Name);
            Assert.AreEqual(baselineResult.TotalRevenue, result.TotalRevenue,
                $"{scenario}: Revenue mismatch for {result.Theater.Name}");
        }
    }

    public static void AssertSortedByRevenueDescending(
        List<TheaterPerformanceResult> results,
        string context)
    {
        for (int i = 1; i < results.Count; i++)
        {
            Assert.IsTrue(
                results[i - 1].TotalRevenue >= results[i].TotalRevenue,
                $"{context}: Results not sorted by revenue descending");
        }
    }
}
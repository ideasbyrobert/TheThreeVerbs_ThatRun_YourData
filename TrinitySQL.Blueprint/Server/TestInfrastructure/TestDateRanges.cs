using TrinitySQL.Server.BoundedContexts.TheaterPerformance.Domain.ValueObjects;

namespace TrinitySQL.Blueprint.Server.TestInfrastructure;

public static class TestDateRanges
{
    public static DateRange May2024 => 
        new(new DateOnly(2024, 5, 1), new DateOnly(2024, 5, 31));

    public static DateRange July2024 => 
        new(new DateOnly(2024, 7, 1), new DateOnly(2024, 7, 31));

    public static DateRange December2024 => 
        new(new DateOnly(2024, 12, 1), new DateOnly(2024, 12, 31));

    public static DateRange Q1_2024 => 
        new(new DateOnly(2024, 1, 1), new DateOnly(2024, 3, 31));

    public static DateRange FullYear2024 => 
        new(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));

    public static DateRange SummerQuarter2024 => 
        new(new DateOnly(2024, 6, 1), new DateOnly(2024, 8, 31));

    public static DateRange ChristmasWeek2024 => 
        new(new DateOnly(2024, 12, 19), new DateOnly(2024, 12, 26));
}
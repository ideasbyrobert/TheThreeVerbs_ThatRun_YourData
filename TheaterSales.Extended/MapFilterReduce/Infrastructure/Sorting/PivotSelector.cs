namespace TheaterSales.Extended.MapFilterReduce.Infrastructure.Sorting;

internal static class PivotSelector
{
    public static T SelectMiddleElement<T>(List<T> elements)
    {
        var middleIndex = elements.Count / 2;
        return elements[middleIndex];
    }
}
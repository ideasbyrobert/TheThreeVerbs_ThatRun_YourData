namespace TheaterSales.DotNet.Core.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
    string EventType { get; }
}
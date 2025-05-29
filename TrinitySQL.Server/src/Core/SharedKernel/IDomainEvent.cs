namespace TrinitySQL.Server.Core.SharedKernel;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
    string EventType { get; }
}
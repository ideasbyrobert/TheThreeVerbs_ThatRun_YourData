using TrinitySQL.Server.Core.SharedKernel;

namespace TrinitySQL.Server.Infrastructure.EventBus;

public interface IEventBus
{
    void Publish(IDomainEvent domainEvent);
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
}
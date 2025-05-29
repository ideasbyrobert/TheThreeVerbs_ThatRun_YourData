using FunctionalSQL.Server.Core.SharedKernel;

namespace FunctionalSQL.Server.Infrastructure.EventBus;

public interface IEventBus
{
    void Publish(IDomainEvent domainEvent);
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
}
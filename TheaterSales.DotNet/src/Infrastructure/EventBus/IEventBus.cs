using TheaterSales.DotNet.Core.SharedKernel;

namespace TheaterSales.DotNet.Infrastructure.EventBus;

public interface IEventBus
{
    void Publish(IDomainEvent domainEvent);
    void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
}
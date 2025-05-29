using FunctionalSQL.Server.Core.SharedKernel;
using FunctionalSQL.Strategy.MapFilterReduce;

namespace FunctionalSQL.Server.Infrastructure.EventBus;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        
        if (_handlers.ContainsKey(eventType))
        {
            _handlers[eventType]
                .Map(handler => handler)
                .Reduce(domainEvent, (evt, handler) =>
                {
                    handler.DynamicInvoke(evt);
                    return evt;
                });
        }
    }

    public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        
        _handlers[eventType].Add(handler);
    }
}
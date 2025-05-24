using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheaterSales.DotNet.Infrastructure.EventBus;
using TheaterSales.DotNet.Core.SharedKernel;
using TheaterSales.DotNet.BoundedContexts.TheaterPerformance.Domain.Events;

namespace TheaterSales.DotNet.Tests.BoundedContexts.Infrastructure.EventBus;

[TestClass]
public class EventBusIntegrationTests
{
    private InMemoryEventBus _eventBus = null!;
    private List<IDomainEvent> _capturedEvents = null!;

    [TestInitialize]
    public void Initialize()
    {
        _eventBus = new InMemoryEventBus();
        _capturedEvents = new List<IDomainEvent>();

        SubscribeToAllEventTypes();
    }

    [TestMethod]
    public void EventBus_PublishesAndSubscribesToEvents()
    {
        var theaterEvent = new TheaterPerformanceQueriedEvent(
            new DateOnly(2024, 5, 9),
            8,
            125000m);

        var topTheatersEvent = new TopTheatersQueriedEvent(
            new DateOnly(2024, 5, 1),
            new DateOnly(2024, 5, 31),
            10,
            5);

        _eventBus.Publish(theaterEvent);
        _eventBus.Publish(topTheatersEvent);

        Assert.AreEqual(2, _capturedEvents.Count);
        Assert.IsInstanceOfType(_capturedEvents[0], typeof(TheaterPerformanceQueriedEvent));
        Assert.IsInstanceOfType(_capturedEvents[1], typeof(TopTheatersQueriedEvent));
    }

    [TestMethod]
    public void EventBus_SupportsMultipleSubscribersPerEventType()
    {
        var callCount = 0;

        SubscribeMultipleHandlersToSameEvent(() => callCount++);

        var @event = CreateTheaterPerformanceEvent(
            new DateOnly(2024, 5, 9),
            8,
            125000m);

        _eventBus.Publish(@event);

        AssertExpectedCallCount(callCount, expectedCount: 2);
    }

    [TestMethod]
    public void EventBus_IsolatesEventTypeSubscriptions()
    {
        var theaterEventCount = 0;
        var topTheatersEventCount = 0;

        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e => theaterEventCount++);
        _eventBus.Subscribe<TopTheatersQueriedEvent>(e => topTheatersEventCount++);

        _eventBus.Publish(CreateTheaterPerformanceEvent(
            new DateOnly(2024, 5, 9), 8, 125000m));
        _eventBus.Publish(new TopTheatersQueriedEvent(
            new DateOnly(2024, 5, 1),
            new DateOnly(2024, 5, 31),
            10,
            5));

        Assert.AreEqual(1, theaterEventCount);
        Assert.AreEqual(1, topTheatersEventCount);
    }

    [TestMethod]
    public void EventBus_ProcessesEventsWithMapFilterReduce()
    {
        var processedEvents = new List<(DateOnly Date, decimal Revenue)>();

        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e =>
        {
            if (e.HighestRevenue > 20000m)
            {
                processedEvents.Add((e.Date, e.HighestRevenue));
            }
        });

        for (int i = 0; i < 5; i++)
        {
            _eventBus.Publish(new TheaterPerformanceQueriedEvent(
                new DateOnly(2024, 5, i + 1),
                8,
                10000m * (i + 1)));
        }

        Assert.AreEqual(3, processedEvents.Count);
        Assert.IsTrue(processedEvents.All(e => e.Revenue > 20000m));
    }

    [TestMethod]
    public void EventBus_HandlesHighVolumeOfEvents()
    {
        var eventCount = 1000;
        var receivedCount = 0;
        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e => receivedCount++);

        for (int i = 0; i < eventCount; i++)
        {
            _eventBus.Publish(new TheaterPerformanceQueriedEvent(
                new DateOnly(2024, 1, 1).AddDays(i % 365),
                8,
                1000m + i));
        }

        Assert.AreEqual(eventCount, receivedCount);
    }

    private void SubscribeToAllEventTypes()
    {
        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e => _capturedEvents.Add(e));
        _eventBus.Subscribe<TopTheatersQueriedEvent>(e => _capturedEvents.Add(e));
    }

    private void SubscribeMultipleHandlersToSameEvent(Action handler)
    {
        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e => handler());
        _eventBus.Subscribe<TheaterPerformanceQueriedEvent>(e => handler());
    }

    private TheaterPerformanceQueriedEvent CreateTheaterPerformanceEvent(
        DateOnly date,
        int theaterCount,
        decimal highestRevenue)
    {
        return new TheaterPerformanceQueriedEvent(date, theaterCount, highestRevenue);
    }

    private void AssertExpectedCallCount(int actualCount, int expectedCount)
    {
        var message = $"{expectedCount} handlers (subscription in Initialize doesn't count)";
        Assert.AreEqual(expectedCount, actualCount, message);
    }
}
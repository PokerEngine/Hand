using Domain.Error;
using Domain.Event;
using Domain.ValueObject;

namespace DomainTest.EventTest;

public class EventBusTest
{
    [Fact]
    public void TestSubscribeUnsubscribePublish()
    {
        var eventBus = new EventBus();

        List<BaseEvent> events1 = [];
        List<BaseEvent> events2 = [];

        var listener1 = (BaseEvent @event) =>
        {
            events1.Add(@event);
        };
        var listener2 = (BaseEvent @event) =>
        {
            events2.Add(@event);
        };

        var handUid = new HandUid(Guid.NewGuid());
        var nickname = new Nickname("Nickname");

        var event1 = new PlayerConnectedEvent(
            Nickname: nickname,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        var event2 = new PlayerDisconnectedEvent(
            Nickname: nickname,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );

        eventBus.Subscribe(listener1);
        eventBus.Publish(event1);

        Assert.Equal([event1], events1);
        Assert.Empty(events2);

        eventBus.Unsubscribe(listener1);
        eventBus.Subscribe(listener2);
        eventBus.Publish(event2);

        Assert.Equal([event1], events1);
        Assert.Equal([event2], events2);
    }

    [Fact]
    public void TestSubscribeWhenAlreadySubscribed()
    {
        var eventBus = new EventBus();

        var listener = (BaseEvent @event) =>
        {
            
        };

        eventBus.Subscribe(listener);

        var exc = Assert.Throws<NotAvailableError>(() => eventBus.Subscribe(listener));

        Assert.Equal("The listener has already been subscribed", exc.Message);
    }

    [Fact]
    public void TestUnsubscribeWhenNotSubscribed()
    {
        var eventBus = new EventBus();

        var listener = (BaseEvent @event) =>
        {
            
        };

        var exc = Assert.Throws<NotAvailableError>(() => eventBus.Unsubscribe(listener));

        Assert.Equal("The listener has not been subscribed yet", exc.Message);
    }
}
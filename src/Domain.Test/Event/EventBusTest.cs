using Domain.Event;
using Domain.ValueObject;

namespace Domain.Test.Event;

public class EventBusTest
{
    [Fact]
    public void TestSubscribeUnsubscribePublish()
    {
        var eventBus = new EventBus();

        List<IEvent> events1 = [];
        List<IEvent> events2 = [];

        var listener1 = (IEvent @event) => events1.Add(@event);
        var listener2 = (IEvent @event) => events2.Add(@event);

        var nickname = new Nickname("nickname");

        var event1 = new PlayerConnectedEvent(
            Nickname: nickname,
            OccuredAt: DateTime.Now
        );
        var event2 = new PlayerDisconnectedEvent(
            Nickname: nickname,
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
    public void TestTypedPublish()
    {
        var eventBus = new EventBus();

        List<PlayerConnectedEvent> events1 = [];
        List<PlayerDisconnectedEvent> events2 = [];

        var listener1 = (PlayerConnectedEvent @event) => events1.Add(@event);
        var listener2 = (PlayerDisconnectedEvent @event) => events2.Add(@event);

        var nickname = new Nickname("nickname");

        var event1 = new PlayerConnectedEvent(
            Nickname: nickname,
            OccuredAt: DateTime.Now
        );
        var event2 = new PlayerDisconnectedEvent(
            Nickname: nickname,
            OccuredAt: DateTime.Now
        );

        eventBus.Subscribe(listener1);
        eventBus.Subscribe(listener2);

        eventBus.Publish(event1);

        Assert.Equal([event1], events1);
        Assert.Empty(events2);

        eventBus.Publish(event2);

        Assert.Equal([event1], events1);
        Assert.Equal([event2], events2);
    }
}

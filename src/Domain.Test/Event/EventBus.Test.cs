using Domain.Event;
using Domain.ValueObject;

namespace Domain.Test.Event;

public class EventBusTest
{
    [Fact]
    public void TestSubscribeUnsubscribePublish()
    {
        var eventBus = new EventBus();

        List<BaseEvent> events1 = [];
        List<BaseEvent> events2 = [];

        var listener1 = (BaseEvent @event) => events1.Add(@event);
        var listener2 = (BaseEvent @event) => events2.Add(@event);

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

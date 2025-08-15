using Application.IntegrationEvent;
using Infrastructure.IntegrationEvent;
using Infrastructure.Test.Fixture;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Immutable;

namespace Infrastructure.Test.IntegrationEvent;

// We use different queue names in each test in order to avoid side effects
// when running tests in parallel
public class RabbitMqIntegrationEventBusTest : IClassFixture<RabbitMqFixture>, IDisposable
{
    private readonly RabbitMqIntegrationEventBus _eventBus;
    private readonly TestIntegrationEventHandler _testHandler;
    private readonly TestIntegrationEvent _testEvent;

    public RabbitMqIntegrationEventBusTest(RabbitMqFixture rabbitMqFixture)
    {
        var logger = NullLogger<RabbitMqIntegrationEventBus>.Instance;
        _eventBus = new RabbitMqIntegrationEventBus(rabbitMqFixture.Configuration, logger);
        _eventBus.Connect().GetAwaiter().GetResult();

        _testHandler = new TestIntegrationEventHandler();
        _testEvent = new TestIntegrationEvent(
            "Event",
            123,
            ImmutableList.Create(
                new IntegrationEventParticipant("SmallBlind", "SmallBlind", 1000),
                new IntegrationEventParticipant("BigBlind", "BigBlind", 1000)
            ),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now
        );
    }

    public void Dispose()
    {
        _testHandler.Events.Clear();
        _eventBus.Disconnect().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task TestPublishToSameQueue()
    {
        await _eventBus.Subscribe(_testHandler, new IntegrationEventQueue("test.same_queue"));

        await _eventBus.Publish(_testEvent, new IntegrationEventQueue("test.same_queue"));
        await Task.WhenAny(_testHandler.EventHandledTcs.Task, Task.Delay(500));

        Assert.Single(_testHandler.Events);
        Assert.Equal(_testEvent, _testHandler.Events[0]);
    }

    [Fact]
    public async Task TestPublishToDifferentQueue()
    {
        await _eventBus.Subscribe(_testHandler, new IntegrationEventQueue("test.different_queue"));

        await _eventBus.Publish(_testEvent, new IntegrationEventQueue("test.another_test"));
        await Task.Delay(500);

        Assert.Empty(_testHandler.Events);
    }

    [Fact]
    public async Task TestPublishWhenNotSubscribed()
    {
        await _eventBus.Publish(_testEvent, new IntegrationEventQueue("test.not_subscribed_queue"));
        await Task.Delay(500);

        Assert.Empty(_testHandler.Events);
    }

    [Fact]
    public async Task TestPublishWhenUnsubscribed()
    {
        await _eventBus.Subscribe(_testHandler, new IntegrationEventQueue("test.unsubscribed_queue"));
        await _eventBus.Unsubscribe(_testHandler, new IntegrationEventQueue("test.unsubscribed_queue"));

        await _eventBus.Publish(_testEvent, new IntegrationEventQueue("test.unsubscribed_queue"));
        await Task.Delay(500);

        Assert.Empty(_testHandler.Events);
    }

    [Fact]
    public async Task TestSubscribeWhenAlreadySubscribed()
    {
        await _eventBus.Subscribe(_testHandler, new IntegrationEventQueue("test.already_subscribed_queue"));

        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _eventBus.Subscribe(_testHandler, new IntegrationEventQueue("test.already_subscribed_queue")));

        Assert.Equal("TestIntegrationEventHandler has already subscribed to test.already_subscribed_queue", exc.Message);
    }

    [Fact]
    public async Task TestUnsubscribeWhenNotSubscribed()
    {
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(() => _eventBus.Unsubscribe(_testHandler, new IntegrationEventQueue("test.not_subscribed_yet_queue")));

        Assert.Equal("TestIntegrationEventHandler has not subscribed to test.not_subscribed_yet_queue yet", exc.Message);
    }
}

internal record TestIntegrationEvent(
    string Name,
    int Number,
    ImmutableList<IntegrationEventParticipant> Participants,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent
{
    public virtual bool Equals(TestIntegrationEvent? other)
    {
        // We override it to check participants equality by value
        return other is not null
               && Name == other.Name
               && Number == other.Number
               && Participants.SequenceEqual(other.Participants)
               && TableUid == other.TableUid
               && HandUid == other.HandUid
               && OccuredAt == other.OccuredAt;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Number);
        foreach (var p in Participants)
            hash.Add(p);
        hash.Add(TableUid);
        hash.Add(HandUid);
        hash.Add(OccuredAt);
        return hash.ToHashCode();
    }
}

internal class TestIntegrationEventHandler : IIntegrationEventHandler<TestIntegrationEvent>
{
    public readonly List<TestIntegrationEvent> Events = new();
    public readonly TaskCompletionSource<bool> EventHandledTcs = new();

    public async Task Handle(TestIntegrationEvent integrationEvent)
    {
        Events.Add(integrationEvent);
        EventHandledTcs.TrySetResult(true);
        await Task.CompletedTask;
    }
}

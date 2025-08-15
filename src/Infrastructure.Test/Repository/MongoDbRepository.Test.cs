using Domain.Event;
using Domain.ValueObject;
using Infrastructure.Repository;
using Infrastructure.Test.Fixture;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.Test.Repository;


public class MongoDbRepositoryTest : IClassFixture<MongoDbFixture>, IDisposable
{
    private readonly MongoDbFixture _mongoDbFixture;
    private readonly MongoDbRepository _repository;

    public MongoDbRepositoryTest(MongoDbFixture mongoDbFixture)
    {
        _mongoDbFixture = mongoDbFixture;
        _mongoDbFixture.Database.CreateCollection("events");

        var logger = NullLogger<MongoDbRepository>.Instance;
        _repository = new MongoDbRepository(_mongoDbFixture.Configuration, logger);
        _repository.Connect().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _repository.Disconnect().GetAwaiter().GetResult();
        _mongoDbFixture.Database.DropCollection("events");
    }

    [Fact]
    public async Task TestHandIsCreatedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsCreatedEvent(
            Game: Game.HoldemNoLimit6Max,
            SmallBlind: new Chips(5),
            BigBlind: new Chips(10),
            Participants: [
                new Participant(nickname: new Nickname("SmallBlind"), position: Position.SmallBlind, stake: new Chips(1000)),
                new Participant(nickname: new Nickname("BigBlind"), position: Position.BigBlind, stake: new Chips(900)),
                new Participant(nickname: new Nickname("Button"), position: Position.Button, stake: new Chips(800)),
            ],
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestHandIsStartedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsStartedEvent(
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestHandIsFinishedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsFinishedEvent(
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestStageIsStartedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsStartedEvent(
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestStageIsFinishedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsFinishedEvent(
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestPlayerConnectedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new PlayerConnectedEvent(
            Nickname: new Nickname("SmallBlind"),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestPlayerDisconnectedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new PlayerDisconnectedEvent(
            Nickname: new Nickname("SmallBlind"),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestSmallBlindIsPostedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new SmallBlindIsPostedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(5),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestBigBlindIsPostedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BigBlindIsPostedEvent(
            Nickname: new Nickname("BigBlind"),
            Amount: new Chips(5),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestHoleCardsAreDealtEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreDealtEvent(
            Nickname: new Nickname("SmallBlind"),
            Cards: new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestBoardCardsAreDealtEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BoardCardsAreDealtEvent(
            Cards: new CardSet([Card.KingOfClubs, Card.TenOfDiamonds, Card.DeuceOfClubs]),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestDecisionIsRequestedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new DecisionIsRequestedEvent(
            Nickname: new Nickname("SmallBlind"),
            FoldIsAvailable: true,
            CheckIsAvailable: false,
            CallIsAvailable: true,
            CallToAmount: new Chips(10),
            RaiseIsAvailable: true,
            MinRaiseToAmount: new Chips(20),
            MaxRaiseToAmount: new Chips(1000),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestDecisionIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new DecisionIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Decision: new Decision(DecisionType.RaiseTo, new Chips(30)),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestRefundIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new RefundIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(20),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task WinWithoutShowdownIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new WinWithoutShowdownIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(20),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task WinAtShowdownIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new WinAtShowdownIsCommittedEvent(
            SidePot: new SidePot
            {
                { new Nickname("BigBlind"), new Chips(30) },
                { new Nickname("SmallBlind"), new Chips(30) }
            },
            WinPot: new SidePot { { new Nickname("BigBlind"), new Chips(60) } },
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestHoleCardsAreMuckedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreMuckedEvent(
            Nickname: new Nickname("BigBlind"),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task TestHoleCardsAreShownEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreShownEvent(
            Nickname: new Nickname("SmallBlind"),
            Cards: new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            Combo: new Combo(ComboType.OnePair, 100500),
            OccuredAt: now
        );

        await _repository.AddEvents(handUid, [@event]);

        var events = await _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    private static DateTime GetNow()
    {
        // We truncate nanoseconds because they are not supported in Mongo
        var now = DateTime.Now;
        return new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond), now.Kind);
    }
}

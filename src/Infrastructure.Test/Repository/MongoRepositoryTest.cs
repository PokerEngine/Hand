using Domain.Event;
using Domain.ValueObject;
using Infrastructure.Repository;
using Infrastructure.Test.Fixture;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Test.Repository;


public class MongoRepositoryTest : BaseMongoTest
{
    private readonly MongoRepository _repository;

    public MongoRepositoryTest(MongoFixture fixture) : base(fixture)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Warning);
        });

        _repository = new MongoRepository(
            host: fixture.Host,
            port: fixture.Port,
            username: fixture.Username,
            password: fixture.Password,
            databaseName: fixture.DatabaseName,
            collectionName: fixture.CollectionName,
            logger: loggerFactory.CreateLogger<MongoRepository>()
        );
    }

    [Fact]
    public void TestHandIsCreatedEvent()
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

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestHandIsStartedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsStartedEvent(
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestHandIsFinishedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsFinishedEvent(
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestStageIsStartedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsStartedEvent(
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestStageIsFinishedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsFinishedEvent(
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestPlayerConnectedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new PlayerConnectedEvent(
            Nickname: new Nickname("SmallBlind"),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestPlayerDisconnectedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new PlayerDisconnectedEvent(
            Nickname: new Nickname("SmallBlind"),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestSmallBlindIsPostedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new SmallBlindIsPostedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(5),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestBigBlindIsPostedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BigBlindIsPostedEvent(
            Nickname: new Nickname("BigBlind"),
            Amount: new Chips(5),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestHoleCardsAreDealtEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreDealtEvent(
            Nickname: new Nickname("SmallBlind"),
            Cards: new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestBoardCardsAreDealtEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BoardCardsAreDealtEvent(
            Cards: new CardSet([Card.KingOfClubs, Card.TenOfDiamonds, Card.DeuceOfClubs]),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestDecisionIsRequestedEvent()
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

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestDecisionIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new DecisionIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Decision: new Decision(DecisionType.RaiseTo, new Chips(30)),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestRefundIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new RefundIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(20),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void WinWithoutShowdownIsCommittedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new WinWithoutShowdownIsCommittedEvent(
            Nickname: new Nickname("SmallBlind"),
            Amount: new Chips(20),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void WinAtShowdownIsCommittedEvent()
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

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestHoleCardsAreMuckedEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreMuckedEvent(
            Nickname: new Nickname("BigBlind"),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public void TestHoleCardsAreShownEvent()
    {
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreShownEvent(
            Nickname: new Nickname("SmallBlind"),
            Cards: new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            Combo: new Combo(ComboType.OnePair, 100500),
            OccuredAt: now
        );

        _repository.AddEvents(handUid, [@event]);

        var events = _repository.GetEvents(handUid);
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

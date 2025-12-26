using Application.Repository;
using Domain.Event;
using Domain.ValueObject;
using Infrastructure.Repository;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Test.Repository;

[Trait("Category", "Integration")]
public class MongoDbRepositoryTest(MongoDbFixture fixture) : IClassFixture<MongoDbFixture>
{
    [Fact]
    public async Task AddEventsAsync_WhenHandIsCreatedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsCreatedEvent
        {
            Game = Game.NoLimitHoldem,
            SmallBlind = new Chips(5),
            BigBlind = new Chips(10),
            MaxSeat = new Seat(6),
            SmallBlindSeat = new Seat(1),
            BigBlindSeat = new Seat(2),
            ButtonSeat = new Seat(6),
            Participants = [
                new Participant(new Nickname("SmallBlind"), new Seat(1), new Chips(1000)),
                new Participant(new Nickname("BigBlind"), new Seat(2), new Chips(900)),
                new Participant(new Nickname("Button"), new Seat(6), new Chips(800)),
            ],
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenHandIsStartedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsStartedEvent
        {
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenHandIsFinishedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HandIsFinishedEvent
        {
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenStageIsStartedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsStartedEvent
        {
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenStageIsFinishedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new StageIsFinishedEvent
        {
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenSmallBlindIsPostedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new SmallBlindIsPostedEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Amount = new Chips(5),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenBigBlindIsPostedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BigBlindIsPostedEvent
        {
            Nickname = new Nickname("BigBlind"),
            Amount = new Chips(5),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenHoleCardsAreDealtEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreDealtEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Cards = new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenBoardCardsAreDealtEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new BoardCardsAreDealtEvent
        {
            Cards = new CardSet([Card.KingOfClubs, Card.TenOfDiamonds, Card.DeuceOfClubs]),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenDecisionIsRequestedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new DecisionIsRequestedEvent
        {
            Nickname = new Nickname("SmallBlind"),
            FoldIsAvailable = true,
            CheckIsAvailable = false,
            CallIsAvailable = true,
            CallToAmount = new Chips(10),
            RaiseIsAvailable = true,
            MinRaiseToAmount = new Chips(20),
            MaxRaiseToAmount = new Chips(1000),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenDecisionIsCommittedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new DecisionIsCommittedEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Decision = new Decision(DecisionType.RaiseTo, new Chips(30)),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenRefundIsCommittedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new RefundIsCommittedEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Amount = new Chips(20),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenWinWithoutShowdownIsCommittedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new WinWithoutShowdownIsCommittedEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Amount = new Chips(20),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenWinAtShowdownIsCommittedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new WinAtShowdownIsCommittedEvent
        {
            SidePot = new SidePot
            {
                { new Nickname("BigBlind"), new Chips(30) },
                { new Nickname("SmallBlind"), new Chips(30) }
            },
            WinPot = new SidePot { { new Nickname("BigBlind"), new Chips(60) } },
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenHoleCardsAreMuckedEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());
        var @event = new HoleCardsAreMuckedEvent
        {
            Nickname = new Nickname("BigBlind"),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenHoleCardsAreShownEvent_ShouldAdd()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new HoleCardsAreShownEvent
        {
            Nickname = new Nickname("SmallBlind"),
            Cards = new CardSet([Card.AceOfClubs, Card.AceOfDiamonds]),
            Combo = new Combo(ComboType.OnePair, 100500),
            OccuredAt = now
        };

        // Act
        await repository.AddEventsAsync(handUid, [@event]);

        // Assert
        var events = await repository.GetEventsAsync(handUid);
        Assert.Single(events);
        Assert.Equal(@event, events[0]);
    }

    [Fact]
    public async Task AddEventsAsync_WhenUnknownEvent_ShouldThrowNotImplementedException()
    {
        // Arrange
        var repository = CreateRepository();
        var now = GetNow();
        var handUid = new HandUid(Guid.NewGuid());

        var @event = new UnknownEvent
        {
            OccuredAt = now
        };

        // Act
        var exc = await Assert.ThrowsAsync<NotImplementedException>(
            async () => await repository.AddEventsAsync(handUid, [@event])
        );

        // Assert
        Assert.Equal("Not implemented for UnknownEvent", exc.Message);
    }

    private IRepository CreateRepository()
    {
        var options = Options.Create(fixture.CreateOptions());
        return new MongoDbRepository(
            options,
            NullLogger<MongoDbRepository>.Instance
        );
    }

    private static DateTime GetNow()
    {
        // We truncate nanoseconds because they are not supported in Mongo
        var now = DateTime.Now;
        return new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond), now.Kind);
    }
}

internal record struct UnknownEvent : IEvent
{
    public required DateTime OccuredAt { get; init; }
}

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
    public async Task GetEventsAsync_WhenAdded_ShouldExtractEvents()
    {
        // Arrange
        var repository = CreateRepository();

        var handUid = new HandUid(Guid.NewGuid());
        var @event = new TestEvent
        {
            Rules = new()
            {
                Game = Game.NoLimitHoldem,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            Positions = new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(6),
                Max = new Seat(6)
            },
            Participants = [
                new()
                {
                    Nickname = new Nickname("Alice"),
                    Seat = new Seat(1),
                    Stack = new Chips(1000)
                },
                new()
                {
                    Nickname = new Nickname("Bobby"),
                    Seat = new Seat(2),
                    Stack = new Chips(900)
                },
                new()
                {
                    Nickname = new Nickname("Charlie"),
                    Seat = new Seat(6),
                    Stack = new Chips(800)
                }
            ],
            Nickname = new Nickname("Alice"),
            Seat = new Seat(2),
            Chips = new Chips(1000),
            CardSet = new CardSet([Card.AceOfSpades, Card.SevenOfHearts, Card.DeuceOfDiamonds]),
            Decision = new Decision(DecisionType.RaiseTo, new Chips(30)),
            Combo = new Combo(ComboType.OnePair, 100500),
            OccurredAt = GetNow()
        };
        await repository.AddEventsAsync(handUid, [@event]);

        // Act
        var events = await repository.GetEventsAsync(handUid);

        // Assert
        Assert.Single(events);
        var typedEvent = Assert.IsType<TestEvent>(events[0]);
        Assert.Equal(@event, typedEvent);
    }

    [Fact]
    public async Task GetEventsAsync_WhenNotAdded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var repository = CreateRepository();

        var handUid = new HandUid(Guid.NewGuid());
        var @event = new TestEvent
        {
            Rules = new()
            {
                Game = Game.NoLimitHoldem,
                SmallBlind = new Chips(5),
                BigBlind = new Chips(10)
            },
            Positions = new()
            {
                SmallBlind = new Seat(1),
                BigBlind = new Seat(2),
                Button = new Seat(6),
                Max = new Seat(6)
            },
            Participants = [
                new()
                {
                    Nickname = new Nickname("Alice"),
                    Seat = new Seat(1),
                    Stack = new Chips(1000)
                },
                new()
                {
                    Nickname = new Nickname("Bobby"),
                    Seat = new Seat(2),
                    Stack = new Chips(900)
                },
                new()
                {
                    Nickname = new Nickname("Charlie"),
                    Seat = new Seat(6),
                    Stack = new Chips(800)
                }
            ],
            Nickname = new Nickname("Alice"),
            Seat = new Seat(2),
            Chips = new Chips(1000),
            CardSet = new CardSet([Card.AceOfSpades, Card.SevenOfHearts, Card.DeuceOfDiamonds]),
            Decision = new Decision(DecisionType.RaiseTo, new Chips(30)),
            Combo = new Combo(ComboType.OnePair, 100500),
            OccurredAt = GetNow()
        };
        await repository.AddEventsAsync(handUid, [@event]);

        // Act & Assert
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await repository.GetEventsAsync(new HandUid(Guid.NewGuid()))
        );
        Assert.Equal("The hand is not found", exc.Message);
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

internal record struct TestEvent : IEvent
{
    public required Rules Rules { get; init; }
    public required Positions Positions { get; init; }
    public required List<Participant> Participants { get; init; }
    public required Nickname Nickname { get; init; }
    public required Seat Seat { get; init; }
    public required Chips Chips { get; init; }
    public required CardSet CardSet { get; init; }
    public required Decision Decision { get; init; }
    public required Combo Combo { get; init; }
    public required DateTime OccurredAt { get; init; }

    public bool Equals(TestEvent other)
    {
        return Rules.Equals(other.Rules)
               && Positions.Equals(other.Positions)
               && Participants.SequenceEqual(other.Participants)
               && Nickname.Equals(other.Nickname)
               && Seat.Equals(other.Seat)
               && Chips.Equals(other.Chips)
               && CardSet.Equals(other.CardSet)
               && Decision.Equals(other.Decision)
               && Combo.Equals(other.Combo)
               && OccurredAt.Equals(other.OccurredAt);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Rules);
        hash.Add(Positions);

        foreach (var participant in Participants)
        {
            hash.Add(participant);
        }

        hash.Add(Nickname);
        hash.Add(Seat);
        hash.Add(Chips);
        hash.Add(CardSet);
        hash.Add(Decision);
        hash.Add(Combo);
        hash.Add(OccurredAt);

        return hash.ToHashCode();
    }
}

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
            Game = Game.NoLimitHoldem,
            Participants = [
                new Participant(new Nickname("alice"), new Seat(1), new Chips(1000)),
                new Participant(new Nickname("bobby"), new Seat(2), new Chips(900)),
                new Participant(new Nickname("charlie"), new Seat(6), new Chips(800))
            ],
            Nickname = new Nickname("alice"),
            Seat = new Seat(2),
            Chips = new Chips(1000),
            CardSet = new CardSet([Card.AceOfSpades, Card.SevenOfHearts, Card.DeuceOfDiamonds]),
            Decision = new Decision(DecisionType.RaiseTo, new Chips(30)),
            Combo = new Combo(ComboType.OnePair, 100500),
            SidePot = new SidePot([
                new KeyValuePair<Nickname, Chips>(new Nickname("alice"), new Chips(25)),
                new KeyValuePair<Nickname, Chips>(new Nickname("bobby"), new Chips(5)),
                new KeyValuePair<Nickname, Chips>(new Nickname("charlie"), new Chips(10))
            ]),
            OccuredAt = GetNow()
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
            Game = Game.NoLimitHoldem,
            Participants = [
                new Participant(new Nickname("alice"), new Seat(1), new Chips(1000)),
                new Participant(new Nickname("bobby"), new Seat(2), new Chips(900)),
                new Participant(new Nickname("charlie"), new Seat(6), new Chips(800))
            ],
            Nickname = new Nickname("alice"),
            Seat = new Seat(2),
            Chips = new Chips(1000),
            CardSet = new CardSet([Card.AceOfSpades, Card.SevenOfHearts, Card.DeuceOfDiamonds]),
            Decision = new Decision(DecisionType.RaiseTo, new Chips(30)),
            Combo = new Combo(ComboType.OnePair, 100500),
            SidePot = new SidePot([
                new KeyValuePair<Nickname, Chips>(new Nickname("alice"), new Chips(25)),
                new KeyValuePair<Nickname, Chips>(new Nickname("bobby"), new Chips(5)),
                new KeyValuePair<Nickname, Chips>(new Nickname("charlie"), new Chips(10))
            ]),
            OccuredAt = GetNow()
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
    public required Game Game { get; init; }
    public required List<Participant> Participants { get; init; }
    public required Nickname Nickname { get; init; }
    public required Seat Seat { get; init; }
    public required Chips Chips { get; init; }
    public required CardSet CardSet { get; init; }
    public required Decision Decision { get; init; }
    public required Combo Combo { get; init; }
    public required SidePot SidePot { get; init; }
    public required DateTime OccuredAt { get; init; }

    public bool Equals(TestEvent other)
    {
        return Game.Equals(other.Game)
               && Participants.SequenceEqual(other.Participants)
               && Nickname.Equals(other.Nickname)
               && Seat.Equals(other.Seat)
               && Chips.Equals(other.Chips)
               && CardSet.Equals(other.CardSet)
               && Decision.Equals(other.Decision)
               && Combo.Equals(other.Combo)
               && SidePot.Equals(other.SidePot)
               && OccuredAt.Equals(other.OccuredAt)
               ;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Game);

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
        hash.Add(SidePot);
        hash.Add(OccuredAt);

        return hash.ToHashCode();
    }
}

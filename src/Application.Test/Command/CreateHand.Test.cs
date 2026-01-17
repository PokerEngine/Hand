using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Test.Command;

public class CreateHandTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldCreateHand()
    {
        // Arrange
        var repository = new StubRepository();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var command = new CreateHandCommand
        {
            TableUid = Guid.NewGuid(),
            TableType = "Cash",
            Game = "NoLimitHoldem",
            SmallBlind = 5,
            BigBlind = 10,
            MaxSeat = 6,
            SmallBlindSeat = 1,
            BigBlindSeat = 2,
            ButtonSeat = 6,
            Participants = [
                new()
                {
                    Nickname = "Alice",
                    Seat = 1,
                    Stack = 1000
                },
                new()
                {
                    Nickname = "Bobby",
                    Seat = 2,
                    Stack = 1000
                },
                new()
                {
                    Nickname = "Charlie",
                    Seat = 6,
                    Stack = 1000
                }
            ]
        };
        var handler = new CreateHandHandler(repository, eventDispatcher, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        var hand = Hand.FromEvents(response.Uid, randomizer, evaluator, await repository.GetEventsAsync(response.Uid));
        Assert.Equal(new HandUid(response.Uid), hand.Uid);
        var state = hand.GetState();
        Assert.Equal(Game.NoLimitHoldem, state.Rules.Game);
        Assert.Equal(new Chips(5), state.Rules.SmallBlind);
        Assert.Equal(new Chips(10), state.Rules.BigBlind);
        Assert.Equal(new Seat(1), state.Table.Positions.SmallBlind);
        Assert.Equal(new Seat(2), state.Table.Positions.BigBlind);
        Assert.Equal(new Seat(6), state.Table.Positions.Button);
        Assert.Equal(new Seat(6), state.Table.Positions.Max);
        Assert.Equal(3, state.Table.Players.Count);
        Assert.Empty(state.Table.BoardCards);

        var events = await eventDispatcher.GetDispatchedEventsAsync(response.Uid);
        Assert.Single(events);
        Assert.IsType<HandIsCreatedEvent>(events[0]);
    }
}

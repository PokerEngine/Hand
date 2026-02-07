using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Application.Test.Storage;
using Domain.Entity;
using Domain.Event;
using Domain.ValueObject;

namespace Application.Test.Command;

public class StartHandTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldStartHand()
    {
        // Arrange
        var repository = new StubRepository();
        var storage = new StubStorage();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var command = new StartHandCommand
        {
            TableUid = Guid.NewGuid(),
            TableType = "Cash",
            Rules = new StartHandCommandRules
            {
                Game = "NoLimitHoldem",
                SmallBlind = 5,
                BigBlind = 10,
                MaxSeat = 6
            },
            Table = new StartHandCommandTable
            {
                Positions = new StartHandCommandPositions
                {
                    SmallBlindSeat = 1,
                    BigBlindSeat = 2,
                    ButtonSeat = 6
                },
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
            }
        };
        var handler = new StartHandHandler(repository, storage, eventDispatcher, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        var hand = Hand.FromEvents(response.Uid, randomizer, evaluator, await repository.GetEventsAsync(response.Uid));
        Assert.Equal(new HandUid(response.Uid), hand.Uid);
        var state = hand.GetState();
        Assert.Equal(Game.NoLimitHoldem, state.Rules.Game);
        Assert.Equal(new Seat(6), state.Rules.MaxSeat);
        Assert.Equal(new Chips(5), state.Rules.SmallBlind);
        Assert.Equal(new Chips(10), state.Rules.BigBlind);
        Assert.Equal(new Seat(1), state.Table.Positions.SmallBlindSeat);
        Assert.Equal(new Seat(2), state.Table.Positions.BigBlindSeat);
        Assert.Equal(new Seat(6), state.Table.Positions.ButtonSeat);
        Assert.Equal(3, state.Table.Players.Count);
        Assert.Empty(state.Table.BoardCards);

        var detailView = await storage.GetDetailViewAsync(hand.Uid);
        Assert.Equal((Guid)hand.Uid, detailView.Uid);
        Assert.Equal(2, detailView.Pot.CurrentBets.Count);

        var events = await eventDispatcher.GetDispatchedEventsAsync(response.Uid);
        Assert.Equal(12, events.Count);
        Assert.IsType<HandStartedEvent>(events[0]);
    }
}

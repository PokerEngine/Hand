using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Domain.Entity;
using Domain.Event;

namespace Application.Test.Command;

public class StartHandTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldStartHand()
    {
        // Arrange
        var repository = new StubRepository();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var handUid = await CreateHandAsync(repository, eventDispatcher, randomizer, evaluator);
        await eventDispatcher.ClearDispatchedEventsAsync(handUid);

        var command = new StartHandCommand
        {
            Uid = handUid
        };
        var handler = new StartHandHandler(repository, eventDispatcher, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(handUid, response.Uid);

        var hand = Hand.FromEvents(response.Uid, randomizer, evaluator, await repository.GetEventsAsync(response.Uid));
        var state = hand.GetState();
        Assert.Equal(2, state.Pot.CurrentBets.Count);

        var events = await eventDispatcher.GetDispatchedEventsAsync(response.Uid);
        Assert.Equal(12, events.Count);
        Assert.IsType<HandStartedEvent>(events[0]);
    }

    private async Task<Guid> CreateHandAsync(
        StubRepository repository,
        StubEventDispatcher eventDispatcher,
        StubRandomizer randomizer,
        StubEvaluator evaluator)
    {
        var handler = new CreateHandHandler(repository, eventDispatcher, randomizer, evaluator);
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
        var response = await handler.HandleAsync(command);
        return response.Uid;
    }
}

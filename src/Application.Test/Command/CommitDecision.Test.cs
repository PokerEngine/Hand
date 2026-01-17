using Application.Command;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Domain.Entity;
using Domain.Event;

namespace Application.Test.Command;

public class CommitDecisionTest
{
    [Fact]
    public async Task HandleAsync_Valid_ShouldCommitDecision()
    {
        // Arrange
        var repository = new StubRepository();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var handUid = await CreateHandAsync(repository, eventDispatcher, randomizer, evaluator);
        await StartHandAsync(handUid, repository, eventDispatcher, randomizer, evaluator);
        await eventDispatcher.ClearDispatchedEventsAsync(handUid);

        var command = new CommitDecisionCommand()
        {
            Uid = handUid,
            Nickname = "Charlie",
            DecisionType = "RaiseTo",
            DecisionAmount = 25,
        };
        var handler = new CommitDecisionHandler(repository, eventDispatcher, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(handUid, response.Uid);

        var hand = Hand.FromEvents(response.Uid, randomizer, evaluator, await repository.GetEventsAsync(response.Uid));
        var state = hand.GetState();
        Assert.Equal(3, state.Pot.UncommittedBets.Count);

        var events = await eventDispatcher.GetDispatchedEventsAsync(response.Uid);
        Assert.Equal(2, events.Count);
        Assert.IsType<DecisionIsCommittedEvent>(events[0]);
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

    private async Task StartHandAsync(
        Guid uid,
        StubRepository repository,
        StubEventDispatcher eventDispatcher,
        StubRandomizer randomizer,
        StubEvaluator evaluator)
    {
        var handler = new StartHandHandler(repository, eventDispatcher, randomizer, evaluator);
        var command = new StartHandCommand
        {
            Uid = uid
        };
        await handler.HandleAsync(command);
    }
}

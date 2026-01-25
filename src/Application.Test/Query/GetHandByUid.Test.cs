using Application.Command;
using Application.Query;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Domain.Entity;

namespace Application.Test.Query;

public class GetHandByUidTest
{
    [Fact]
    public async Task HandleAsync_Exists_ShouldReturn()
    {
        // Arrange
        var repository = new StubRepository();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var handUid = await CreateHandAsync(repository, eventDispatcher, randomizer, evaluator);
        await StartHandAsync(handUid, repository, eventDispatcher, randomizer, evaluator);

        var query = new GetHandByUidQuery { Uid = handUid };
        var handler = new GetHandByUidHandler(repository, randomizer, evaluator);

        // Act
        var response = await handler.HandleAsync(query);

        // Assert
        var hand = Hand.FromEvents(response.Uid, randomizer, evaluator, await repository.GetEventsAsync(response.Uid));
        Assert.Equal(handUid, response.Uid);
        Assert.Equal(hand.Rules.Game.ToString(), response.Game);
        Assert.Equal((int)hand.Rules.SmallBlind, response.SmallBlind);
        Assert.Equal((int)hand.Rules.BigBlind, response.BigBlind);
        Assert.Equal((int)hand.Table.Positions.SmallBlind, response.SmallBlindSeat);
        Assert.Equal((int)hand.Table.Positions.BigBlind, response.BigBlindSeat);
        Assert.Equal((int)hand.Table.Positions.Button, response.ButtonSeat);
        Assert.Equal((int)hand.Table.Positions.Max, response.MaxSeat);
        Assert.Equal(3, response.State.Table.Players.Count);
        Assert.Equal(2, response.State.Pot.CurrentBets.Count);
        Assert.Empty(response.State.Pot.CollectedBets);
        Assert.Empty(response.State.Pot.Awards);
    }

    [Fact]
    public async Task HandleAsync_NotExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var repository = new StubRepository();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();

        var query = new GetHandByUidQuery { Uid = Guid.NewGuid() };
        var handler = new GetHandByUidHandler(repository, randomizer, evaluator);

        // Act
        var exc = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await handler.HandleAsync(query);
        });

        // Assert
        Assert.Equal("The hand is not found", exc.Message);
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

using Application.Command;
using Application.Exception;
using Application.Query;
using Application.Test.Event;
using Application.Test.Repository;
using Application.Test.Service.Evaluator;
using Application.Test.Service.Randomizer;
using Application.Test.Storage;

namespace Application.Test.Query;

public class GetHandDetailTest
{
    [Fact]
    public async Task HandleAsync_Exists_ShouldReturn()
    {
        // Arrange
        var repository = new StubRepository();
        var storage = new StubStorage();
        var eventDispatcher = new StubEventDispatcher();
        var randomizer = new StubRandomizer();
        var evaluator = new StubEvaluator();
        var handUid = await StartHandAsync(repository, storage, eventDispatcher, randomizer, evaluator);

        var query = new GetHandDetailQuery { Uid = handUid };
        var handler = new GetHandDetailHandler(storage);

        // Act
        var response = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(handUid, response.Uid);
        Assert.Equal("NoLimitHoldem", response.Rules.Game);
        Assert.Equal(6, response.Rules.MaxSeat);
        Assert.Equal(5, response.Rules.SmallBlind);
        Assert.Equal(10, response.Rules.BigBlind);
        Assert.Equal(1, response.Table.Positions.SmallBlindSeat);
        Assert.Equal(2, response.Table.Positions.BigBlindSeat);
        Assert.Equal(6, response.Table.Positions.ButtonSeat);
        Assert.Equal(3, response.Table.Players.Count);
        Assert.Equal(2, response.Pot.CurrentBets.Count);
        Assert.Empty(response.Pot.CollectedBets);
        Assert.Empty(response.Pot.Awards);
    }

    [Fact]
    public async Task HandleAsync_NotExists_ShouldThrowException()
    {
        // Arrange
        var storage = new StubStorage();

        var query = new GetHandDetailQuery { Uid = Guid.NewGuid() };
        var handler = new GetHandDetailHandler(storage);

        // Act
        var exc = await Assert.ThrowsAsync<HandNotFoundException>(async () =>
        {
            await handler.HandleAsync(query);
        });

        // Assert
        Assert.Equal("The hand is not found", exc.Message);
    }

    private async Task<Guid> StartHandAsync(
        StubRepository repository,
        StubStorage storage,
        StubEventDispatcher eventDispatcher,
        StubRandomizer randomizer,
        StubEvaluator evaluator
    )
    {
        var handler = new StartHandHandler(repository, storage, eventDispatcher, randomizer, evaluator);
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
        var response = await handler.HandleAsync(command);
        return response.Uid;
    }
}

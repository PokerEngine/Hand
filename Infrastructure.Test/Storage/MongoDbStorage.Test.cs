using Application.Exception;
using Application.Storage;
using Domain.Entity;
using Domain.ValueObject;
using Infrastructure.Storage;
using Infrastructure.Test.Client.MongoDb;
using Infrastructure.Test.Service.Evaluator;
using Infrastructure.Test.Service.Randomizer;
using Microsoft.Extensions.Options;

namespace Infrastructure.Test.Storage;

[Trait("Category", "Integration")]
public class MongoDbStorageTest(MongoDbClientFixture fixture) : IClassFixture<MongoDbClientFixture>
{
    [Fact]
    public async Task GetDetailViewAsync_WhenExists_ShouldReturn()
    {
        // Arrange
        var storage = CreateStorage();
        var hand = CreateHand();
        hand.Start();
        hand.SubmitPlayerAction("Charlie", new PlayerAction(PlayerActionType.RaiseBy, 25));
        hand.SubmitPlayerAction("Alice", new PlayerAction(PlayerActionType.Fold));
        hand.SubmitPlayerAction("Bobby", new PlayerAction(PlayerActionType.CallBy, 15));
        hand.SubmitPlayerAction("Bobby", new PlayerAction(PlayerActionType.Check));
        hand.SubmitPlayerAction("Charlie", new PlayerAction(PlayerActionType.RaiseBy, 15));
        hand.SubmitPlayerAction("Bobby", new PlayerAction(PlayerActionType.Fold));
        await storage.SaveViewAsync(hand);

        // Act
        var view = await storage.GetDetailViewAsync(hand.Uid);

        // Assert
        Assert.Equal(hand.Uid, (HandUid)view.Uid);
        Assert.Equal("NoLimitHoldem", view.Rules.Game);
        Assert.Equal(6, view.Rules.MaxSeat);
        Assert.Equal(5, view.Rules.SmallBlind);
        Assert.Equal(10, view.Rules.BigBlind);

        Assert.Equal(1, view.Table.Positions.SmallBlindSeat);
        Assert.Equal(2, view.Table.Positions.BigBlindSeat);
        Assert.Equal(6, view.Table.Positions.ButtonSeat);
        Assert.Equal(6, view.Table.BoardCards.Length); // 3 cards

        Assert.Equal(3, view.Table.Players.Count);
        Assert.Equal("Alice", view.Table.Players[0].Nickname);
        Assert.Equal(1, view.Table.Players[0].Seat);
        Assert.Equal(795, view.Table.Players[0].Stack);
        Assert.Equal(4, view.Table.Players[0].HoleCards.Length); // 2 cards
        Assert.True(view.Table.Players[0].IsFolded);
        Assert.Equal("Bobby", view.Table.Players[1].Nickname);
        Assert.Equal(2, view.Table.Players[1].Seat);
        Assert.Equal(875, view.Table.Players[1].Stack);
        Assert.Equal(4, view.Table.Players[1].HoleCards.Length); // 2 cards
        Assert.True(view.Table.Players[1].IsFolded);
        Assert.Equal("Charlie", view.Table.Players[2].Nickname);
        Assert.Equal(6, view.Table.Players[2].Seat);
        Assert.Equal(975, view.Table.Players[2].Stack);
        Assert.Equal(4, view.Table.Players[2].HoleCards.Length); // 2 cards
        Assert.False(view.Table.Players[2].IsFolded);

        Assert.Equal(0, view.Pot.Ante);
        Assert.Empty(view.Pot.CollectedBets);
        Assert.Empty(view.Pot.CurrentBets);
        Assert.Single(view.Pot.Awards);
        Assert.Equal(["Charlie"], view.Pot.Awards[0].Winners);
        Assert.Equal(55, view.Pot.Awards[0].Amount);
    }

    [Fact]
    public async Task GetDetailViewAsync_WhenNotExists_ShouldThrowException()
    {
        // Arrange
        var storage = CreateStorage();

        // Act
        var exc = await Assert.ThrowsAsync<HandNotFoundException>(async () =>
            await storage.GetDetailViewAsync(new HandUid(Guid.NewGuid())));

        // Assert
        Assert.Equal("The hand is not found", exc.Message);
    }

    private IStorage CreateStorage()
    {
        var client = fixture.CreateClient();
        var options = CreateOptions();
        return new MongoDbStorage(client, options);
    }

    private IOptions<MongoDbStorageOptions> CreateOptions()
    {
        var options = new MongoDbStorageOptions
        {
            Database = $"test_storage_{Guid.NewGuid()}"
        };
        return Options.Create(options);
    }

    private Hand CreateHand(
        Game game = Game.NoLimitHoldem,
        int smallBlind = 5,
        int bigBlind = 10
    )
    {
        return Hand.FromScratch(
            uid: new HandUid(Guid.NewGuid()),
            tableUid: new TableUid(Guid.NewGuid()),
            tableType: TableType.Cash,
            rules: new Rules
            {
                Game = game,
                MaxSeat = 6,
                SmallBlind = smallBlind,
                BigBlind = bigBlind
            },
            positions: new Positions
            {
                SmallBlindSeat = 1,
                BigBlindSeat = 2,
                ButtonSeat = 6
            },
            players: [
                new Participant
                {
                    Nickname = "Alice",
                    Seat = 1,
                    Stack = 800,
                },
                new Participant
                {
                    Nickname = "Bobby",
                    Seat = 2,
                    Stack = 900,
                },
                new Participant
                {
                    Nickname = "Charlie",
                    Seat = 6,
                    Stack = 1000,
                }
            ],
            randomizer: new StubRandomizer(),
            evaluator: new StubEvaluator()
        );
    }
}

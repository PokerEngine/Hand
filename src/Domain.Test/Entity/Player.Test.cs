using Domain.Entity;
using Domain.Exception;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class PlayerTest
{
    [Fact]
    public void TestInitialization()
    {
        var player = new Player(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(1),
            stack: new Chips(1000)
        );

        Assert.Equal(new Nickname("BigBlind"), player.Nickname);
        Assert.Equal(new Seat(1), player.Seat);
        Assert.Equal(new Chips(1000), player.Stack);
        Assert.Empty(player.HoleCards);
        Assert.False(player.IsFolded);
    }

    [Fact]
    public void TestTakeHoleCards()
    {
        var player = CreatePlayer();

        player.TakeHoleCards(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]), player.HoleCards);

        player.TakeHoleCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds]));

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.KingOfHearts, Card.TreyOfDiamonds]), player.HoleCards);
    }

    [Fact]
    public void TestTakeHoleCardsWhenFolded()
    {
        var player = CreatePlayer();
        player.Fold();

        var exc = Assert.Throws<InvalidHandStateException>(() => player.TakeHoleCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds])));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.Empty(player.HoleCards);
    }

    [Fact]
    public void TestFold()
    {
        var player = CreatePlayer();

        player.Fold();

        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stack);
    }

    [Fact]
    public void TestFoldWhenFolded()
    {
        var player = CreatePlayer();
        player.Fold();

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Fold());

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stack);
    }

    [Fact]
    public void TestFoldWhenAllIn()
    {
        var player = CreatePlayer();
        player.Post(player.Stack);

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Fold());

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stack);
    }

    [Fact]
    public void TestPost()
    {
        var player = CreatePlayer();

        player.Post(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stack);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostAllIn()
    {
        var player = CreatePlayer();

        player.Post(new Chips(1000));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stack);
        Assert.True(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenFolded()
    {
        var player = CreatePlayer();
        player.Fold();

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stack);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenAllIn()
    {
        var player = CreatePlayer();
        player.Post(player.Stack);

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stack);
    }

    [Fact]
    public void TestPostWhenNotEnoughStack()
    {
        var player = CreatePlayer();

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Post(new Chips(1025)));

        Assert.Equal("The player cannot post more amount than his stack", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stack);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestRefund()
    {
        var player = CreatePlayer();

        player.Refund(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stack);
    }

    [Fact]
    public void TestRefundWhenFolded()
    {
        var player = CreatePlayer();
        player.Fold();

        var exc = Assert.Throws<InvalidHandStateException>(() => player.Refund(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stack);
    }

    [Fact]
    public void TestRepresentation()
    {
        var player = new Player(
            nickname: new Nickname("BigBlind"),
            seat: new Seat(1),
            stack: new Chips(1000)
        );
        player.TakeHoleCards(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));

        Assert.Equal($"Player: {player.Nickname}, {player.Seat}, {player.Stack}, {player.HoleCards}", $"{player}");
    }

    private Player CreatePlayer(string nickname = "BigBlind", int seat = 1, int stack = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stack: new Chips(stack)
        );
    }
}

using Domain.Entity;
using Domain.Error;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class PlayerTest
{
    [Fact]
    public void TestInitialization()
    {
        var player = new Player(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        Assert.Equal(new Nickname("BigBlind"), player.Nickname);
        Assert.Equal(Position.BigBlind, player.Position);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.Empty(player.HoleCards);
        Assert.False(player.IsConnected);
        Assert.False(player.IsFolded);
    }

    [Fact]
    public void TestConnect()
    {
        var player = CreatePlayer();

        player.Connect();

        Assert.True(player.IsConnected);
    }

    [Fact]
    public void TestConnectWhenAlreadyConnected()
    {
        var player = CreatePlayer();
        player.Connect();

        var exc = Assert.Throws<NotAvailableError>(() => player.Connect());

        Assert.Equal("The player has already connected", exc.Message);
        Assert.True(player.IsConnected);
    }

    [Fact]
    public void TestDisconnect()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Disconnect();

        Assert.False(player.IsConnected);
    }

    [Fact]
    public void TestDisconnectWhenNotConnected()
    {
        var player = CreatePlayer();

        var exc = Assert.Throws<NotAvailableError>(() => player.Disconnect());

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsConnected);
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
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.TakeHoleCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds])));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.Empty(player.HoleCards);
    }

    [Fact]
    public void TestFold()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Fold();

        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestFoldWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Fold());

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestFoldWhenAllIn()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Bet(player.Stake);

        var exc = Assert.Throws<NotAvailableError>(() => player.Fold());

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestCheck()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Check();

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenNotConnected()
    {
        var player = CreatePlayer();

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenAllIn()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Bet(player.Stake);

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestBet()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Bet(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetAllIn()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Bet(player.Stake);

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
        Assert.True(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenNotConnected()
    {
        var player = CreatePlayer();

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenAllIn()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Bet(player.Stake);

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestBetWhenNotEnoughStake()
    {
        var player = CreatePlayer();
        player.Connect();

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(1025)));

        Assert.Equal("The player cannot bet more amount than his stake", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPost()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Post(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenNotConnected()
    {
        var player = CreatePlayer();

        player.Post(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostAllIn()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Post(new Chips(1000));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
        Assert.True(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenAllIn()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Post(player.Stake);

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestPostWhenNotEnoughStake()
    {
        var player = CreatePlayer();
        player.Connect();

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(1025)));

        Assert.Equal("The player cannot post more amount than his stake", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestWin()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Win(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestWinWhenNotConnected()
    {
        var player = CreatePlayer();

        player.Win(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestWinWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Win(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestRefund()
    {
        var player = CreatePlayer();
        player.Connect();

        player.Refund(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestRefundWhenNotConnected()
    {
        var player = CreatePlayer();

        player.Refund(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestRefundWhenFolded()
    {
        var player = CreatePlayer();
        player.Connect();
        player.Fold();

        var exc = Assert.Throws<NotAvailableError>(() => player.Refund(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestRepresentation()
    {
        var player = new Player(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        player.TakeHoleCards(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]));

        Assert.Equal("BigBlind, BigBlind, 1000 chip(s), {AceOfSpades, DeuceOfClubs}", $"{player}");
    }

    private Player CreatePlayer(string nickname = "BigBlind", Position position = Position.BigBlind, int stake = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            position: position,
            stake: new Chips(stake)
        );
    }
}

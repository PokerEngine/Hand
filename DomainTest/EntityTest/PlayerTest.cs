using Domain.Entity;
using Domain.Error;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class PlayerTest
{
    [Fact]
    public void TestInitialization()
    {
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]);
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: cards,
            isConnected: true,
            isFolded: true
        );

        Assert.Equal(new Nickname("nickname"), player.Nickname);
        Assert.Equal(Position.Button, player.Position);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.Equal(cards, player.HoleCards);
        Assert.True(player.IsConnected);
        Assert.True(player.IsFolded);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestConnect(bool isFolded)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: false,
            isFolded: isFolded
        );

        player.Connect();

        Assert.True(player.IsConnected);
    }

    [Fact]
    public void TestConnectWhenAlreadyConnected()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Connect());

        Assert.Equal("The player has already connected", exc.Message);
        Assert.True(player.IsConnected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestDisconnect(bool isFolded)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: isFolded
        );

        player.Disconnect();

        Assert.False(player.IsConnected);
    }

    [Fact]
    public void TestDisconnectWhenNotConnected()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: false,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Disconnect());

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsConnected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestTakeHoleCards(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        player.TakeHoleCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds]));

        Assert.Equal(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.KingOfHearts, Card.TreyOfDiamonds]), player.HoleCards);
    }

    [Fact]
    public void TestTakeHoleCardsWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.TakeHoleCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds])));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.Equal(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]), player.HoleCards);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestFold(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        player.Fold();

        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestFoldWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Fold());

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestFoldWhenAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(0),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Fold());

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestCheck()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        player.Check();

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenNotConnected()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: false,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Fact]
    public void TestCheckWhenAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(0),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Check());

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestBet()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        player.Bet(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        player.Bet(new Chips(1000));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
        Assert.True(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenNotConnected()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: false,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has not connected yet", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestBetWhenAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(0),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(25)));

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestBetWhenNotEnoughStake()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Bet(new Chips(1025)));

        Assert.Equal("The player cannot bet more amount than his stake", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestPost(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        player.Post(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(975), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        player.Post(new Chips(1000));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
        Assert.True(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Fact]
    public void TestPostWhenAllIn()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(0),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(25)));

        Assert.Equal("The player has already been all in", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(0), player.Stake);
    }

    [Fact]
    public void TestPostWhenNotEnoughStake()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: false
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Post(new Chips(1025)));

        Assert.Equal("The player cannot post more amount than his stake", exc.Message);
        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
        Assert.False(player.IsAllIn);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestWin(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        player.Win(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestWinWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Win(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestRefund(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        player.Refund(new Chips(25));

        Assert.False(player.IsFolded);
        Assert.Equal(new Chips(1025), player.Stake);
    }

    [Fact]
    public void TestRefundWhenFolded()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        var exc = Assert.Throws<NotAvailableError>(() => player.Refund(new Chips(25)));

        Assert.Equal("The player has already folded", exc.Message);
        Assert.True(player.IsFolded);
        Assert.Equal(new Chips(1000), player.Stake);
    }

    [Theory]
    [InlineData(true, false, 1000)]
    [InlineData(true, false, 0)]
    [InlineData(false, false, 1000)]
    [InlineData(false, false, 0)]
    public void TestAvailableForDealing(bool isConnected, bool isFolded, Chips stake)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: stake,
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: isFolded
        );

        Assert.True(player.IsAvailableForDealing);
    }

    [Theory]
    [InlineData(true, true, 1000)]
    [InlineData(true, true, 0)]
    [InlineData(false, true, 1000)]
    [InlineData(false, true, 0)]
    public void TestNotAvailableForDealing(bool isConnected, bool isFolded, Chips stake)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: stake,
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: isFolded
        );

        Assert.False(player.IsAvailableForDealing);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestAvailableForTrading(bool isConnected)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: false
        );

        Assert.True(player.IsAvailableForTrading);
    }

    [Theory]
    [InlineData(true, false, 0)]
    [InlineData(true, true, 1000)]
    [InlineData(true, true, 0)]
    [InlineData(false, false, 0)]
    [InlineData(false, true, 1000)]
    [InlineData(false, true, 0)]
    public void TestNotAvailableForTrading(bool isConnected, bool isFolded, Chips stake)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: stake,
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: isFolded
        );

        Assert.False(player.IsAvailableForTrading);
    }

    [Theory]
    [InlineData(true, false, 1000)]
    [InlineData(true, false, 0)]
    [InlineData(false, false, 1000)]
    [InlineData(false, false, 0)]
    public void TestAvailableForShowdown(bool isConnected, bool isFolded, Chips stake)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: stake,
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: isFolded
        );

        Assert.True(player.IsAvailableForShowdown);
    }

    [Theory]
    [InlineData(true, true, 1000)]
    [InlineData(true, true, 0)]
    [InlineData(false, true, 1000)]
    [InlineData(false, true, 0)]
    public void TestNotAvailableForShowdown(bool isConnected, bool isFolded, Chips stake)
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: stake,
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: isConnected,
            isFolded: isFolded
        );

        Assert.False(player.IsAvailableForShowdown);
    }

    [Fact]
    public void TestRepresentation()
    {
        var player = new Player(
            nickname: new Nickname("nickname"),
            position: Position.Button,
            stake: new Chips(1000),
            holeCards: new CardSet([Card.AceOfSpades, Card.DeuceOfClubs]),
            isConnected: true,
            isFolded: true
        );

        Assert.Equal("nickname, Button, 1000 chip(s), {AceOfSpades, DeuceOfClubs}", $"{player}");
    }
}
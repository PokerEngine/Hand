using Domain.Entity;
using Domain.Error;
using Domain.ValueObject;

namespace DomainTest.EntityTest;

public class SixMaxTableTest
{
    [Fact]
    public void TestInitialization()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        var playerBu = Player.Create(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(1000)
        );
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.AceOfClubs]);

        var table = new SixMaxTable(
            players: [playerBu, playerSb, playerBb],
            boardCards: cards
        );

        Assert.Equal(3, table.Players.Count);
        Assert.Equal(playerSb, table.Players[0]);
        Assert.Equal(playerBb, table.Players[1]);
        Assert.Equal(playerBu, table.Players[2]);
        Assert.Equal(cards, table.BoardCards);
    }

    [Fact]
    public void TestInitializationWithDuplicatedNicknames()
    {
        var playerA = Player.Create(
            nickname: new Nickname("Alpha"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerB = Player.Create(
            nickname: new Nickname("Alpha"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        SixMaxTable table;
        var exc = Assert.Throws<NotAvailableError>(() => table = new SixMaxTable([playerA, playerB], boardCards: new CardSet()));
        Assert.Equal("The table must contain players with unique nicknames", exc.Message);
    }

    [Fact]
    public void TestInitializationWithDuplicatedPositions()
    {
        var playerA = Player.Create(
            nickname: new Nickname("Alpha"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        var playerB = Player.Create(
            nickname: new Nickname("Beta"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        SixMaxTable table;
        var exc = Assert.Throws<NotAvailableError>(() => table = new SixMaxTable([playerA, playerB], boardCards: new CardSet()));
        Assert.Equal("The table must contain players with unique positions", exc.Message);
    }

    [Fact]
    public void TestInitializationWithSinglePlayer()
    {
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        SixMaxTable table;
        var exc = Assert.Throws<NotAvailableError>(() => table = new SixMaxTable([playerBb], boardCards: new CardSet()));
        Assert.Equal("The table must contain at least 2 players", exc.Message);
    }

    [Fact]
    public void TestInitializationWithoutBigBlind()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBu = Player.Create(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(1000)
        );

        SixMaxTable table;
        var exc = Assert.Throws<NotAvailableError>(() => table = new SixMaxTable([playerSb, playerBu], boardCards: new CardSet()));
        Assert.Equal("The table must contain a player on the big blind", exc.Message);
    }

    [Fact]
    public void TestInitializationWithNotAllowedPositions()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        var playerUtg1 = Player.Create(
            nickname: new Nickname("UnderTheGun1"),
            position: Position.UnderTheGun1,
            stake: new Chips(1000)
        );

        SixMaxTable table;
        var exc = Assert.Throws<NotAvailableError>(() => table = new SixMaxTable([playerSb, playerBb, playerUtg1], boardCards: new CardSet()));
        Assert.Equal("The table must contain players with allowed positions", exc.Message);
    }

    [Fact]
    public void TestCreate()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        var playerBu = Player.Create(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerBu, playerSb, playerBb]);

        Assert.Equal(3, table.Players.Count);
        Assert.Equal(playerSb, table.Players[0]);
        Assert.Equal(playerBb, table.Players[1]);
        Assert.Equal(playerBu, table.Players[2]);
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestGetPlayerByNickname()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerSb, playerBb]);

        Assert.Equal(playerSb, table.GetPlayerByNickname(playerSb.Nickname));
        Assert.Equal(playerBb, table.GetPlayerByNickname(playerBb.Nickname));
    }

    [Fact]
    public void TestGetPlayerByNicknameWhenNotFound()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerSb, playerBb]);

        Player player;
        var exc = Assert.Throws<NotFoundError>(() => player = table.GetPlayerByNickname(new Nickname("Button")));
        Assert.Equal("A player with the given nickname is not found at the table", exc.Message);
    }

    [Fact]
    public void TestGetPlayerByPosition()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerSb, playerBb]);

        Assert.Equal(playerSb, table.GetPlayerByPosition(Position.SmallBlind));
        Assert.Equal(playerBb, table.GetPlayerByPosition(Position.BigBlind));
    }

    [Fact]
    public void TestGetPlayerByPositionWhenNotFound()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerSb, playerBb]);

        Player player;
        var exc = Assert.Throws<NotFoundError>(() => player = table.GetPlayerByPosition(Position.Button));
        Assert.Equal("A player on the given position is not found at the table", exc.Message);
    }

    [Fact]
    public void TestTakeBoardCards()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );

        var table = SixMaxTable.Create([playerSb, playerBb]);

        table.TakeBoardCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs]));

        Assert.Equal(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs]), table.BoardCards);

        table.TakeBoardCards(new CardSet([Card.AceOfDiamonds]));

        Assert.Equal(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs, Card.AceOfDiamonds]), table.BoardCards);
    }

    [Fact]
    public void TestRepresentation()
    {
        var playerSb = Player.Create(
            nickname: new Nickname("SmallBlind"),
            position: Position.SmallBlind,
            stake: new Chips(1000)
        );
        var playerBb = Player.Create(
            nickname: new Nickname("BigBlind"),
            position: Position.BigBlind,
            stake: new Chips(1000)
        );
        var playerBu = Player.Create(
            nickname: new Nickname("Button"),
            position: Position.Button,
            stake: new Chips(1000)
        );
        var cards = new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.AceOfClubs]);

        var table = new SixMaxTable(
            players: [playerBu, playerSb, playerBb],
            boardCards: cards
        );

        Assert.Equal("SixMaxTable: 3 player(s), {AceOfSpades, DeuceOfClubs, AceOfClubs}", $"{table}");
    }
}
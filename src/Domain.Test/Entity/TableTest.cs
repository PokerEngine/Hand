using Domain.Entity;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class SixMaxTableTest
{
    [Fact]
    public void TestInitialization()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );

        var table = new SixMaxTable([playerBu, playerSb, playerBb]);

        Assert.Equal(3, table.Count);
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestInitializationWithDuplicatedNicknames()
    {
        var playerA = CreatePlayer(
            nickname: "Alpha",
            position: Position.SmallBlind
        );
        var playerB = CreatePlayer(
            nickname: "Alpha",
            position: Position.BigBlind
        );

        SixMaxTable table;
        var exc = Assert.Throws<ArgumentException>(() => table = new SixMaxTable([playerA, playerB]));
        Assert.StartsWith("The table must contain players with unique nicknames", exc.Message);
    }

    [Fact]
    public void TestInitializationWithDuplicatedPositions()
    {
        var playerA = CreatePlayer(
            nickname: "Alpha",
            position: Position.BigBlind
        );
        var playerB = CreatePlayer(
            nickname: "Beta",
            position: Position.BigBlind
        );

        SixMaxTable table;
        var exc = Assert.Throws<ArgumentException>(() => table = new SixMaxTable([playerA, playerB]));
        Assert.StartsWith("The table must contain players with unique positions", exc.Message);
    }

    [Fact]
    public void TestInitializationWithSinglePlayer()
    {
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        SixMaxTable table;
        var exc = Assert.Throws<ArgumentException>(() => table = new SixMaxTable([playerBb]));
        Assert.StartsWith("The table must contain at least 2 players", exc.Message);
    }

    [Fact]
    public void TestInitializationWithoutBigBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );

        SixMaxTable table;
        var exc = Assert.Throws<ArgumentException>(() => table = new SixMaxTable([playerSb, playerBu]));
        Assert.StartsWith("The table must contain a player on the big blind", exc.Message);
    }

    [Fact]
    public void TestInitializationWithNotAllowedPositions()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerUtg1 = CreatePlayer(
            nickname: "UnderTheGun1",
            position: Position.UnderTheGun1
        );

        SixMaxTable table;
        var exc = Assert.Throws<ArgumentException>(() => table = new SixMaxTable([playerSb, playerBb, playerUtg1]));
        Assert.StartsWith("The table must contain players with allowed positions", exc.Message);
    }

    [Fact]
    public void TestGetPlayerByNickname()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        var table = CreateTable([playerSb, playerBb]);

        Assert.Equal(playerSb, table.GetPlayerByNickname(playerSb.Nickname));
        Assert.Equal(playerBb, table.GetPlayerByNickname(playerBb.Nickname));
    }

    [Fact]
    public void TestGetPlayerByNicknameWhenNotFound()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        var table = CreateTable([playerSb, playerBb]);

        Player player;
        var exc = Assert.Throws<ArgumentException>(() => player = table.GetPlayerByNickname(new Nickname("Button")));
        Assert.StartsWith("A player with the given nickname is not found at the table", exc.Message);
    }

    [Fact]
    public void TestGetPlayerByPosition()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        var table = CreateTable([playerSb, playerBb]);

        Assert.Equal(playerSb, table.GetPlayerByPosition(Position.SmallBlind));
        Assert.Equal(playerBb, table.GetPlayerByPosition(Position.BigBlind));
    }

    [Fact]
    public void TestGetPlayerByPositionWhenNotFound()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        var table = CreateTable([playerSb, playerBb]);

        Player player;
        var exc = Assert.Throws<ArgumentException>(() => player = table.GetPlayerByPosition(Position.Button));
        Assert.StartsWith("A player on the given position is not found at the table", exc.Message);
    }

    [Fact]
    public void TestTakeBoardCards()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );

        var table = CreateTable([playerSb, playerBb]);

        table.TakeBoardCards(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs]));

        Assert.Equal(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs]), table.BoardCards);

        table.TakeBoardCards(new CardSet([Card.AceOfDiamonds]));

        Assert.Equal(new CardSet([Card.KingOfHearts, Card.TreyOfDiamonds, Card.DeuceOfClubs, Card.AceOfDiamonds]), table.BoardCards);
    }

    [Fact]
    public void TestEnumerator()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );
        List<Player> players = [playerSb, playerBb, playerBu];

        var table = CreateTable(players);

        var i = 0;
        foreach (var player in table)
        {
            Assert.Equal(player, players[i]);
            i++;
        }
    }

    [Fact]
    public void TestRepresentation()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            position: Position.SmallBlind
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            position: Position.BigBlind
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            position: Position.Button
        );

        var table = new SixMaxTable([playerBu, playerSb, playerBb]);
        table.TakeBoardCards(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.AceOfClubs]));

        Assert.Equal($"SixMaxTable: 3 player(s), {table.BoardCards}", $"{table}");
    }

    private BaseTable CreateTable(IEnumerable<Player> players)
    {
        return new SixMaxTable(players);
    }

    private Player CreatePlayer(string nickname, Position position, int stake = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            position: position,
            stake: new Chips(stake)
        );
    }
}

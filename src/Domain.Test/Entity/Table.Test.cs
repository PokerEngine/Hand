using Domain.Entity;
using Domain.ValueObject;

namespace Domain.Test.Entity;

public class TableTest
{
    [Fact]
    public void TestInitialization()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );

        var table = new Table(
            maxSeat: new Seat(6),
            smallBlindSeat: playerSb.Seat,
            bigBlindSeat: playerBb.Seat,
            buttonSeat: playerBu.Seat,
            players: [playerBu, playerSb, playerBb]
        );

        Assert.Equal(3, table.Count);
        Assert.False(table.IsHeadsUp());
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestInitializationHeadsUp()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = new Table(
            maxSeat: new Seat(6),
            smallBlindSeat: playerSb.Seat,
            bigBlindSeat: playerBb.Seat,
            buttonSeat: playerSb.Seat,
            players: [playerSb, playerBb]
        );

        Assert.Equal(2, table.Count);
        Assert.True(table.IsHeadsUp());
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestInitializationWithDuplicatedNicknames()
    {
        var playerA = CreatePlayer(
            nickname: "Alpha",
            seat: 1
        );
        var playerB = CreatePlayer(
            nickname: "Alpha",
            seat: 2
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerA, playerB],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(1)
            );
        });
        Assert.StartsWith("The table must contain players with unique nicknames", exc.Message);
    }

    [Fact]
    public void TestInitializationWithDuplicatedSeats()
    {
        var playerA = CreatePlayer(
            nickname: "Alpha",
            seat: 1
        );
        var playerB = CreatePlayer(
            nickname: "Beta",
            seat: 1
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerA, playerB],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(1)
            );
        });
        Assert.StartsWith("The table must contain players with unique seats", exc.Message);
    }

    [Fact]
    public void TestInitializationWithNotAllowedSeats()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerWrong = CreatePlayer(
            nickname: "Wrong",
            seat: 9
        );

        var exc = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Table(
                players: [playerSb, playerBb, playerWrong],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(6)
            );
        });
        Assert.StartsWith("The table supports seats till #6", exc.Message);
    }

    [Fact]
    public void TestInitializationWithSinglePlayer()
    {
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 1
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerBb],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(1),
                buttonSeat: new Seat(1)
            );
        });
        Assert.StartsWith("The table must contain at least 2 players", exc.Message);
    }

    [Fact]
    public void TestInitializationWithoutSmallBlind()
    {
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 1
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );

        var table = new Table(
            maxSeat: new Seat(6),
            smallBlindSeat: new Seat(2),
            bigBlindSeat: new Seat(1),
            buttonSeat: new Seat(6),
            players: [playerBb, playerBu]
        );

        Assert.Equal(2, table.Count);
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestInitializationWithoutBigBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerSb, playerBu],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(6)
            );
        });
        Assert.StartsWith("The table must contain a player on the big blind", exc.Message);
    }

    [Fact]
    public void TestInitializationWithoutButton()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 2
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 1
        );

        var table = new Table(
            players: [playerSb, playerBb],
            maxSeat: new Seat(6),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(6)
        );

        Assert.Equal(2, table.Count);
        Assert.Empty(table.BoardCards);
    }

    [Fact]
    public void TestInitializationWithSameSmallAndBigBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerSb, playerBb],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(2),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(1)
            );
        });
        Assert.StartsWith("The table must contain different players on the big and small blinds", exc.Message);
    }

    [Fact]
    public void TestInitializationWithSameBigBlindAndButton()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var exc = Assert.Throws<ArgumentException>(() =>
        {
            new Table(
                players: [playerSb, playerBb],
                maxSeat: new Seat(6),
                smallBlindSeat: new Seat(1),
                bigBlindSeat: new Seat(2),
                buttonSeat: new Seat(2)
            );
        });
        Assert.StartsWith("The table must contain different players on the big blind and button", exc.Message);
    }

    [Fact]
    public void TestGetPlayerByNickname()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );

        Assert.Equal(playerSb, table.GetPlayerByNickname(playerSb.Nickname));
        Assert.Equal(playerBb, table.GetPlayerByNickname(playerBb.Nickname));
    }

    [Fact]
    public void TestGetPlayerByNicknameWhenNotFound()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );

        var exc = Assert.Throws<ArgumentException>(() => table.GetPlayerByNickname(new Nickname("Wrong")));
        Assert.StartsWith("A player with the given nickname is not found at the table", exc.Message);
    }

    [Fact]
    public void TestGetPlayerOnSmallBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );

        Assert.Equal(playerSb, table.GetPlayerOnSmallBlind());
    }

    [Fact]
    public void TestGetPlayerOnSmallBlindWhenMissed()
    {
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 1
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 6
        );

        var table = CreateTable(
            players: [playerBb, playerBu],
            smallBlindSeat: 2,
            bigBlindSeat: 1,
            buttonSeat: 6
        );

        Assert.Null(table.GetPlayerOnSmallBlind());
    }

    [Fact]
    public void TestGetPlayerOnBigBlind()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );

        Assert.Equal(playerBb, table.GetPlayerOnBigBlind());
    }

    [Fact]
    public void TestGetPlayerOnButton()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 1
        );

        Assert.Equal(playerSb, table.GetPlayerOnButton());
    }

    [Fact]
    public void TestGetPlayerOnButtonWhenMissed()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 2
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 1
        );

        var table = CreateTable(
            players: [playerSb, playerBb],
            smallBlindSeat: 1,
            bigBlindSeat: 2,
            buttonSeat: 6
        );

        Assert.Null(table.GetPlayerOnButton());
    }

    [Fact]
    public void TestTakeBoardCards()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
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
            seat: 2
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 4
        );
        var playerCo = CreatePlayer(
            nickname: "CutOff",
            seat: 6
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 1
        );

        var table = CreateTable(
            players: [playerSb, playerBb, playerCo, playerBu],
            smallBlindSeat: 2,
            bigBlindSeat: 4,
            buttonSeat: 1
        );

        var expectedPlayers = new List<Player> { playerBu, playerSb, playerBb, playerCo };
        var i = 0;
        foreach (var player in table)
        {
            Assert.Equal(expectedPlayers[i], player);
            i++;
        }
        Assert.Equal(expectedPlayers.Count, i);
    }

    [Fact]
    public void TestRepresentation()
    {
        var playerSb = CreatePlayer(
            nickname: "SmallBlind",
            seat: 1
        );
        var playerBb = CreatePlayer(
            nickname: "BigBlind",
            seat: 2
        );
        var playerBu = CreatePlayer(
            nickname: "Button",
            seat: 3
        );

        var table = new Table(
            players: [playerSb, playerBb, playerBu],
            maxSeat: new Seat(6),
            smallBlindSeat: new Seat(1),
            bigBlindSeat: new Seat(2),
            buttonSeat: new Seat(3)
        );
        table.TakeBoardCards(new CardSet([Card.AceOfSpades, Card.DeuceOfClubs, Card.AceOfClubs]));

        Assert.Equal($"Table: 3 player(s), {table.BoardCards}", $"{table}");
    }

    private Table CreateTable(
        IEnumerable<Player> players,
        int maxSeat = 6,
        int smallBlindSeat = 1,
        int bigBlindSeat = 2,
        int buttonSeat = 3
    )
    {
        return new Table(
            players: players,
            maxSeat: new Seat(maxSeat),
            smallBlindSeat: new Seat(smallBlindSeat),
            bigBlindSeat: new Seat(bigBlindSeat),
            buttonSeat: new Seat(buttonSeat)
        );
    }

    private Player CreatePlayer(string nickname, int seat, int stake = 1000)
    {
        return new Player(
            nickname: new Nickname(nickname),
            seat: new Seat(seat),
            stake: new Chips(stake)
        );
    }
}

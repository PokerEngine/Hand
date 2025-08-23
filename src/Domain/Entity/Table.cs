using Domain.ValueObject;
using System.Collections;

namespace Domain.Entity;

public abstract class BaseTable : IEnumerable<Player>
{
    private readonly Player?[] _players;
    private readonly Seat _smallBlindSeat;
    private readonly Seat _bigBlindSeat;
    private readonly Seat _buttonSeat;

    public CardSet BoardCards { get; private set; }
    public int Count => _players.Count(x => x != null);

    protected BaseTable(
        IEnumerable<Player> players,
        Seat maxSeat,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    )
    {
        _players = new Player?[maxSeat];
        _bigBlindSeat = bigBlindSeat;
        _smallBlindSeat = smallBlindSeat;
        _buttonSeat = buttonSeat;

        BoardCards = new CardSet();

        var allPlayers = players.ToList();
        foreach (var player in allPlayers)
        {
            if (player.Seat > maxSeat)
            {
                throw new ArgumentOutOfRangeException(nameof(players), players, $"The table supports seats till {maxSeat}");
            }
            _players[player.Seat - 1] = player;
        }

        var nicknames = allPlayers.Select(x => x.Nickname).ToHashSet();
        if (allPlayers.Count != nicknames.Count)
        {
            throw new ArgumentException("The table must contain players with unique nicknames", nameof(players));
        }

        var seats = allPlayers.Select(x => x.Seat).ToHashSet();
        if (allPlayers.Count != seats.Count)
        {
            throw new ArgumentException("The table must contain players with unique seats", nameof(players));
        }

        if (allPlayers.Count < 2)
        {
            throw new ArgumentException("The table must contain at least 2 players", nameof(players));
        }

        if (_players[_bigBlindSeat - 1] == null)
        {
            throw new ArgumentException("The table must contain a player on the big blind", nameof(players));
        }

        if (_smallBlindSeat == _bigBlindSeat)
        {
            throw new ArgumentException("The table must contain different players on the big and small blinds", nameof(smallBlindSeat));
        }

        if (_buttonSeat == _bigBlindSeat)
        {
            throw new ArgumentException("The table must contain different players on the big blind and button", nameof(buttonSeat));
        }
    }

    public Player GetPlayerByNickname(Nickname nickname)
    {
        foreach (var player in this)
        {
            if (player.Nickname == nickname)
            {
                return player;
            }
        }

        throw new ArgumentException("A player with the given nickname is not found at the table", nameof(nickname));
    }

    public Player? GetPlayerOnSmallBlind()
    {
        return GetPlayerOnSeat(_smallBlindSeat);
    }

    public Player? GetPlayerOnBigBlind()
    {
        return GetPlayerOnSeat(_bigBlindSeat);
    }

    public Player? GetPlayerOnButton()
    {
        return GetPlayerOnSeat(_buttonSeat);
    }

    private Player? GetPlayerOnSeat(Seat seat)
    {
        return _players[seat - 1];
    }

    public void TakeBoardCards(CardSet boardCards)
    {
        BoardCards += boardCards;
    }

    public IEnumerator<Player> GetEnumerator()
    {
        foreach (var player in _players)
        {
            if (player != null)
            {
                yield return player;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public override string ToString()
        => $"{GetType().Name}: {Count} player(s), {BoardCards}";
}

public class SixMaxTable : BaseTable
{
    public SixMaxTable(
        IEnumerable<Player> players,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    ) : base(players, new Seat(6), smallBlindSeat, bigBlindSeat, buttonSeat) { }
}

public class NineMaxTable : BaseTable
{
    public NineMaxTable(
        IEnumerable<Player> players,
        Seat smallBlindSeat,
        Seat bigBlindSeat,
        Seat buttonSeat
    ) : base(players, new Seat(9), smallBlindSeat, bigBlindSeat, buttonSeat) { }
}

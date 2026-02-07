using Domain.Exception;
using Domain.ValueObject;
using System.Collections;

namespace Domain.Entity;

public class Table : IEnumerable<Player>
{
    private readonly Player?[] _players;

    public Positions Positions { get; }
    public CardSet BoardCards { get; private set; }
    public IEnumerable<Player> Players => _players.OfType<Player>();
    public int Count => _players.Count(x => x != null);

    public Table(IEnumerable<Player> players, Rules rules, Positions positions)
    {
        _players = new Player?[rules.MaxSeat];
        Positions = positions;
        BoardCards = new CardSet();

        var allPlayers = players.ToList();
        foreach (var player in allPlayers)
        {
            if (player.Seat > rules.MaxSeat)
            {
                throw new InvalidHandConfigurationException($"The table supports seats till {rules.MaxSeat}");
            }
            _players[player.Seat - 1] = player;
        }

        var nicknames = allPlayers.Select(x => x.Nickname).ToHashSet();
        if (allPlayers.Count != nicknames.Count)
        {
            throw new InvalidHandConfigurationException("The table must contain players with unique nicknames");
        }

        var seats = allPlayers.Select(x => x.Seat).ToHashSet();
        if (allPlayers.Count != seats.Count)
        {
            throw new InvalidHandConfigurationException("The table must contain players with unique seats");
        }

        if (allPlayers.Count < 2)
        {
            throw new InvalidHandConfigurationException("The table must contain at least 2 players");
        }

        if (_players[positions.BigBlindSeat - 1] == null)
        {
            throw new InvalidHandConfigurationException("The table must contain a player on the big blind");
        }

        if (positions.SmallBlindSeat == positions.BigBlindSeat)
        {
            throw new InvalidHandConfigurationException("The table must contain different players on the big and small blinds");
        }

        if (positions.ButtonSeat == positions.BigBlindSeat)
        {
            throw new InvalidHandConfigurationException("The table must contain different players on the big blind and button");
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

        throw new PlayerNotFoundException("The player is not found at the table");
    }

    public Player? GetPlayerOnSmallBlind()
    {
        return GetPlayerOnSeat(Positions.SmallBlindSeat);
    }

    public Player? GetPlayerOnBigBlind()
    {
        return GetPlayerOnSeat(Positions.BigBlindSeat);
    }

    public Player? GetPlayerOnButton()
    {
        return GetPlayerOnSeat(Positions.ButtonSeat);
    }

    private Player? GetPlayerOnSeat(Seat seat)
    {
        return _players[seat - 1];
    }

    public IEnumerable<Player> GetPlayersStartingFromSeat(Seat seat)
    {
        var startIdx = seat - 1;
        for (var i = 0; i < _players.Length; i++)
        {
            var idx = (startIdx + i) % _players.Length;
            var player = _players[idx];
            if (player != null)
            {
                yield return player;
            }
        }
    }

    public Player? GetPlayerNextToSeat(Seat seat, Func<Player, bool> predicate)
    {
        var startIdx = seat < _players.Length ? (int)seat : 0;
        for (var i = 0; i < _players.Length; i++)
        {
            var idx = (startIdx + i) % _players.Length;
            var player = _players[idx];
            if (player != null && predicate(player))
            {
                return player;
            }
        }

        return null;
    }

    public bool IsHeadsUp()
    {
        return Count == 2;
    }

    public void TakeBoardCards(CardSet boardCards)
    {
        BoardCards += boardCards;
    }

    public TableState GetState()
    {
        return new TableState
        {
            Positions = Positions,
            Players = Players.Select(p => p.GetState()).ToList(),
            BoardCards = BoardCards
        };
    }

    public IEnumerator<Player> GetEnumerator()
    {
        foreach (var player in Players)
        {
            yield return player;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public override string ToString()
        => $"{GetType().Name}: {Count} player(s), {BoardCards}";
}

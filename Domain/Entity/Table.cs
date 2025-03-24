using System.Collections.Immutable;
using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BaseTable
{
    public ImmutableList<Player> Players { get; }
    public CardSet BoardCards { get; private set; }

    protected BaseTable(IEnumerable<Player> players, CardSet boardCards)
    {
        var orderedPlayers = players.OrderBy(x => x.Position).ToImmutableList();

        var nicknames = orderedPlayers.Select(x => x.Nickname).ToHashSet();
        if (orderedPlayers.Count != nicknames.Count)
        {
            throw new NotAvailableError("The table must contain players with unique nicknames");
        }

        var positions = orderedPlayers.Select(x => x.Position).ToHashSet();
        if (orderedPlayers.Count != positions.Count)
        {
            throw new NotAvailableError("The table must contain players with unique positions");
        }

        if (orderedPlayers.Count < 2)
        {
            throw new NotAvailableError("The table must contain at least 2 players");
        }

        if (!positions.Contains(Position.BigBlind))
        {
            throw new NotAvailableError("The table must contain a player on the big blind");
        }

        Players = orderedPlayers;
        BoardCards = boardCards;
    }

    public Player GetPlayerByNickname(Nickname nickname)
    {
        foreach (var player in Players)
        {
            if (player.Nickname == nickname)
            {
                return player;
            }
        }

        throw new NotFoundError("A player with the given nickname is not found at the table");
    }

    public Player GetPlayerByPosition(Position position)
    {
        foreach (var player in Players)
        {
            if (player.Position == position)
            {
                return player;
            }
        }

        throw new NotFoundError("A player on the given position is not found at the table");
    }

    public void TakeBoardCards(CardSet boardCards)
    {
        BoardCards += boardCards;
    }

    public override string ToString()
        => $"{GetType().Name}: {Players.Count} player(s), {BoardCards}";
}

public class SixMaxTable : BaseTable
{
    private static readonly Position[] AllowedPositions = [
        Position.SmallBlind,
        Position.BigBlind,
        Position.Early,
        Position.Middle,
        Position.CutOff,
        Position.Button,
    ];

    public SixMaxTable(IEnumerable<Player> players, CardSet boardCards) : base(players, boardCards)
    {
        var positions = players.Select(x => x.Position).ToHashSet();

        if (!positions.IsSubsetOf(AllowedPositions))
        {
            throw new NotAvailableError("The table must contain players with allowed positions");
        }
    }
}

public class NineMaxTable : BaseTable
{
    private static readonly Position[] AllowedPositions = [
        Position.SmallBlind,
        Position.BigBlind,
        Position.UnderTheGun1,
        Position.UnderTheGun2,
        Position.UnderTheGun3,
        Position.Early,
        Position.Middle,
        Position.CutOff,
        Position.Button,
    ];

    public NineMaxTable(IEnumerable<Player> players, CardSet boardCards) : base(players, boardCards)
    {
        var positions = players.Select(x => x.Position).ToHashSet();

        if (!positions.IsSubsetOf(AllowedPositions))
        {
            throw new NotAvailableError("The table must contain players with allowed positions");
        }
    }
}
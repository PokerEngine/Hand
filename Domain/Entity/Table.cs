using System.Collections.Immutable;
using Domain.Error;
using Domain.ValueObject;

namespace Domain.Entity;

public abstract class BaseTable
{
    public ImmutableList<Player> Players { get; }
    public CardSet BoardCards { get; private set; }

    protected BaseTable(IList<Player> players, CardSet boardCards)
    {
        var nicknames = players.Select(x => x.Nickname).ToHashSet();
        if (players.Count != nicknames.Count)
        {
            throw new NotAvailableError("The table must contain players with unique nicknames");
        }

        var positions = players.Select(x => x.Position).ToHashSet();
        if (players.Count != positions.Count)
        {
            throw new NotAvailableError("The table must contain players with unique positions");
        }

        if (players.Count < 2)
        {
            throw new NotAvailableError("The table must contain at least 2 players");
        }

        if (!positions.Contains(Position.BigBlind))
        {
            throw new NotAvailableError("The table must contain a player on the big blind");
        }

        Players = players.OrderBy(x => x.Position).ToImmutableList();
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

    public IList<Player> GetPlayersForDealing()
    {
        return Players.Where(x => x.IsAvailableForDealing).ToList();
    }

    public Player? GetNextPlayerForTrading(Player? previousPlayer)
    {
        var previousIdx = previousPlayer == null ? -1 : Players.IndexOf(previousPlayer);
        var nextIdx = previousIdx + 1;

        while (true)
        {
            if (nextIdx == previousIdx)
            {
                break;
            }

            if (nextIdx == Players.Count)
            {
                if (previousIdx == -1)
                {
                    break;
                }

                nextIdx = 0;
            }

            var nextPlayer = Players[nextIdx];
            if (nextPlayer.IsAvailableForTrading)
            {
                return nextPlayer;
            }

            nextIdx ++;
        }

        return null;
    }

    public bool AllPlayersAreConnected()
    {
        return Players.All(x => x.IsConnected);
    }

    public bool HasEnoughPlayersForTrading()
    {
        return Players.Count(x => x.IsAvailableForTrading) > 1;
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

    public SixMaxTable(IList<Player> players, CardSet boardCards) : base(players, boardCards)
    {
        var positions = players.Select(x => x.Position).ToHashSet();

        if (!positions.IsSubsetOf(AllowedPositions))
        {
            throw new NotAvailableError("The table must contain players with allowed positions");
        }
    }

    public static SixMaxTable Create(IList<Player> players)
    {
        return new (players, new CardSet());
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

    public NineMaxTable(IList<Player> players, CardSet boardCards) : base(players, boardCards)
    {
        var positions = players.Select(x => x.Position).ToHashSet();

        if (!positions.IsSubsetOf(AllowedPositions))
        {
            throw new NotAvailableError("The table must contain players with allowed positions");
        }
    }

    public static NineMaxTable Create(IList<Player> players)
    {
        return new (players, new CardSet());
    }
}
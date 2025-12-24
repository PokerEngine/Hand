using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class ShowdownDealer : IDealer
{
    public IEnumerable<IEvent> Start(
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var startEvent = new StageIsStartedEvent
        {
            OccuredAt = DateTime.Now
        };
        yield return startEvent;

        var players = GetPlayersForShowdown(table);

        var refundEvents = Refund(players, pot);
        foreach (var refundEvent in refundEvents)
        {
            yield return refundEvent;
        }

        if (HasEnoughPlayersForShowdown(players))
        {
            var showdownEvents = WinAtShowdown(
                game: game,
                players: players,
                table: table,
                pot: pot,
                evaluator: evaluator
            );
            foreach (var showdownEvent in showdownEvents)
            {
                yield return showdownEvent;
            }
        }
        else
        {
            var noShowdownEvents = WinWithoutShowdown(players.First(), pot);
            foreach (var noShowdownEvent in noShowdownEvents)
            {
                yield return noShowdownEvent;
            }
        }

        var finishEvent = new StageIsFinishedEvent
        {
            OccuredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private List<Player> GetPlayersForShowdown(Table table)
    {
        return table.Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForShowdown(List<Player> players)
    {
        return players.Count > 1;
    }

    private IEnumerable<RefundIsCommittedEvent> Refund(List<Player> players, BasePot pot)
    {
        foreach (var player in players)
        {
            var amount = pot.GetRefundAmount(player);
            if (amount)
            {
                pot.CommitRefund(player, amount);

                var @event = new RefundIsCommittedEvent
                {
                    Nickname = player.Nickname,
                    Amount = amount,
                    OccuredAt = DateTime.Now
                };
                yield return @event;
            }
        }
    }

    private IEnumerable<IEvent> WinWithoutShowdown(Player player, BasePot pot)
    {
        var amount = pot.GetTotalAmount();
        pot.CommitWinWithoutShowdown(player, amount);

        var muckEvent = new HoleCardsAreMuckedEvent
        {
            Nickname = player.Nickname,
            OccuredAt = DateTime.Now
        };
        yield return muckEvent;

        var winEvent = new WinWithoutShowdownIsCommittedEvent
        {
            Nickname = player.Nickname,
            Amount = amount,
            OccuredAt = DateTime.Now
        };
        yield return winEvent;
    }

    private IEnumerable<IEvent> WinAtShowdown(
        Game game,
        List<Player> players,
        Table table,
        BasePot pot,
        IEvaluator evaluator
    )
    {
        var comboMapping = players.Select(x => (x.Nickname, evaluator.Evaluate(game, table.BoardCards, x.HoleCards))).ToDictionary();

        foreach (var player in players)
        {
            var showEvent = new HoleCardsAreShownEvent
            {
                Nickname = player.Nickname,
                Cards = player.HoleCards,
                Combo = comboMapping[player.Nickname],
                OccuredAt = DateTime.Now
            };
            yield return showEvent;
        }

        var sidePots = pot.GetSidePots(players);

        foreach (var sidePot in sidePots)
        {
            // We compose a set of nicknames which have the strongest combo for each side pot
            var winnerNicknames = sidePot.Nicknames.GroupBy(x => comboMapping[x].Weight).OrderByDescending(x => x.Key).First().ToHashSet();
            var winnerPlayers = players.Where(x => winnerNicknames.Contains(x.Nickname)).ToList();

            var winPot = pot.GetWinPot(winnerPlayers, sidePot);
            pot.CommitWinAtShowdown(winnerPlayers, sidePot, winPot);

            var winEvent = new WinAtShowdownIsCommittedEvent
            {
                SidePot = sidePot,
                WinPot = winPot,
                OccuredAt = DateTime.Now
            };
            yield return winEvent;
        }
    }

    public void Handle(
        IEvent @event,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case RefundIsCommittedEvent e:
                pot.CommitRefund(table.GetPlayerByNickname(e.Nickname), e.Amount);
                break;
            case HoleCardsAreMuckedEvent:
                break;
            case HoleCardsAreShownEvent:
                break;
            case WinWithoutShowdownIsCommittedEvent e:
                pot.CommitWinWithoutShowdown(table.GetPlayerByNickname(e.Nickname), e.Amount);
                break;
            case WinAtShowdownIsCommittedEvent e:
                var players = e.WinPot.Nicknames.Select(table.GetPlayerByNickname).ToList();
                pot.CommitWinAtShowdown(players, e.SidePot, e.WinPot);
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new ArgumentException("The event is not supported", nameof(@event));
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}

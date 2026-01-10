using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class SettlementDealer : IDealer
{
    public IEnumerable<IEvent> Start(
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        var startEvent = new StageIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return startEvent;

        IEnumerable<IEvent> events;
        if (!IsShowdownNeeded(table))
        {
            events = WinWithoutShowdown(table, pot);
        }
        else if (IsSomebodyAllIn(table))
        {
            events = WinAtShowdownWithAllIn(table, pot, rules, evaluator);
        }
        else
        {
            events = WinAtShowdownWithoutAllIn(table, pot, rules, evaluator);
        }

        foreach (var e in events)
        {
            yield return e;
        }

        var finishEvent = new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    public void Handle(
        IEvent @event,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case HoleCardsAreMuckedEvent:
                break;
            case HoleCardsAreShownEvent:
                break;
            case WinIsCommittedEvent:
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }

    private bool IsShowdownNeeded(Table table)
    {
        return table.Players.Count(IsAvailable) > 1;
    }

    private IEnumerable<IEvent> WinWithoutShowdown(Table table, Pot pot)
    {
        var player = table.Players.Where(IsAvailable).First();

        var muckEvent = new HoleCardsAreMuckedEvent
        {
            Nickname = player.Nickname,
            OccurredAt = DateTime.Now
        };
        yield return muckEvent;

        var winEvent = new WinIsCommittedEvent
        {
            Nicknames = [player.Nickname],
            Amount = pot.TotalAmount,
            OccurredAt = DateTime.Now
        };
        yield return winEvent;
    }

    private IEnumerable<IEvent> WinAtShowdownWithAllIn(Table table, Pot pot, Rules rules, IEvaluator evaluator)
    {
        var startPlayer = table.GetPlayerNextToSeat(table.ButtonSeat, IsAvailable)!;
        var players = table.GetPlayersStartingFromSeat(startPlayer.Seat).Where(IsAvailable).ToList();

        var comboMapping = players.Select(x => (x.Nickname, evaluator.Evaluate(rules.Game, table.BoardCards, x.HoleCards))).ToDictionary();

        foreach (var player in players)
        {
            var showEvent = new HoleCardsAreShownEvent
            {
                Nickname = player.Nickname,
                Cards = player.HoleCards,
                Combo = comboMapping[player.Nickname],
                OccurredAt = DateTime.Now
            };
            yield return showEvent;
        }

        var sidePots = pot.CalculateSidePots(players.Select(p => p.Nickname).ToHashSet());

        foreach (var sidePot in sidePots)
        {
            var winningCombo = new Combo(ComboType.HighCard, 0);
            var winningNicknames = new HashSet<Nickname>();

            foreach (var nickname in sidePot.Nicknames)
            {
                var combo = comboMapping[nickname];

                if (combo.Weight == winningCombo.Weight)
                {
                    winningNicknames.Add(nickname);
                }
                else if (combo.Weight > winningCombo.Weight)
                {
                    winningCombo = combo;
                    winningNicknames.Clear();
                    winningNicknames.Add(nickname);
                }
            }

            var winEvent = new WinIsCommittedEvent
            {
                Nicknames = winningNicknames,
                Amount = sidePot.Amount,
                OccurredAt = DateTime.Now
            };
            yield return winEvent;
        }
    }

    private IEnumerable<IEvent> WinAtShowdownWithoutAllIn(Table table, Pot pot, Rules rules, IEvaluator evaluator)
    {
        var startPlayer = GetStartPlayer(table, pot);
        var players = table.GetPlayersStartingFromSeat(startPlayer.Seat).Where(IsAvailable);

        // No all-in => no side pots
        var winningCombo = new Combo(ComboType.HighCard, 0);
        var winningNicknames = new HashSet<Nickname>();

        foreach (var player in players)
        {
            var combo = evaluator.Evaluate(rules.Game, table.BoardCards, player.HoleCards);

            if (combo.Weight < winningCombo.Weight)
            {
                var muckEvent = new HoleCardsAreMuckedEvent
                {
                    Nickname = player.Nickname,
                    OccurredAt = DateTime.Now
                };
                yield return muckEvent;
            }
            else if (combo.Weight == winningCombo.Weight)
            {
                winningNicknames.Add(player.Nickname);

                var showEvent = new HoleCardsAreShownEvent
                {
                    Nickname = player.Nickname,
                    Cards = player.HoleCards,
                    Combo = combo,
                    OccurredAt = DateTime.Now
                };
                yield return showEvent;
            }
            else if (combo.Weight > winningCombo.Weight)
            {
                winningCombo = combo;
                winningNicknames.Clear();
                winningNicknames.Add(player.Nickname);

                var showEvent = new HoleCardsAreShownEvent
                {
                    Nickname = player.Nickname,
                    Cards = player.HoleCards,
                    Combo = combo,
                    OccurredAt = DateTime.Now
                };
                yield return showEvent;
            }
        }

        var winEvent = new WinIsCommittedEvent
        {
            Nicknames = winningNicknames,
            Amount = pot.TotalAmount,
            OccurredAt = DateTime.Now
        };
        yield return winEvent;
    }

    private Player GetStartPlayer(Table table, Pot pot)
    {
        if (pot.LastRaisedNickname is not null)
        {
            return table.GetPlayerByNickname((Nickname)pot.LastRaisedNickname);
        }

        return table.GetPlayerNextToSeat(table.ButtonSeat, IsAvailable)!;
    }

    private bool IsSomebodyAllIn(Table table)
    {
        return table.Players.Count(p => IsAvailable(p) && p.IsAllIn) > 1;
    }

    private bool IsAvailable(Player player)
    {
        return !player.IsFolded;
    }
}

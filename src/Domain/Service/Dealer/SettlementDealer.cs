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
            case AwardIsCommittedEvent:
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

        var sidePot = pot.CalculateSidePots([player.Nickname]).First();
        pot.WinSidePot(sidePot, [player.Nickname]);

        var awardEvent = new AwardIsCommittedEvent
        {
            Nicknames = sidePot.Competitors.ToHashSet(),
            Amount = sidePot.TotalAmount,
            OccurredAt = DateTime.Now
        };
        yield return awardEvent;
    }

    private IEnumerable<IEvent> WinAtShowdownWithAllIn(Table table, Pot pot, Rules rules, IEvaluator evaluator)
    {
        var startPlayer = table.GetPlayerNextToSeat(table.Positions.Button, IsAvailable)!;
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
            var winners = new HashSet<Nickname>();
            var winnerCombo = new Combo(ComboType.HighCard, 0);

            foreach (var nickname in sidePot.Competitors)
            {
                var combo = comboMapping[nickname];

                if (combo.Weight == winnerCombo.Weight)
                {
                    winners.Add(nickname);
                }
                else if (combo.Weight > winnerCombo.Weight)
                {
                    winnerCombo = combo;
                    winners.Clear();
                    winners.Add(nickname);
                }
            }

            pot.WinSidePot(sidePot, winners);

            var awardEvent = new AwardIsCommittedEvent
            {
                Nicknames = winners,
                Amount = sidePot.TotalAmount,
                OccurredAt = DateTime.Now
            };
            yield return awardEvent;
        }
    }

    private IEnumerable<IEvent> WinAtShowdownWithoutAllIn(Table table, Pot pot, Rules rules, IEvaluator evaluator)
    {
        var startPlayer = GetStartPlayer(table, pot);
        var players = table.GetPlayersStartingFromSeat(startPlayer.Seat).Where(IsAvailable);

        // No all-in => no side pots
        var winnerCombo = new Combo(ComboType.HighCard, 0);
        var winners = new HashSet<Nickname>();

        foreach (var player in players)
        {
            var combo = evaluator.Evaluate(rules.Game, table.BoardCards, player.HoleCards);

            if (combo.Weight < winnerCombo.Weight)
            {
                var muckEvent = new HoleCardsAreMuckedEvent
                {
                    Nickname = player.Nickname,
                    OccurredAt = DateTime.Now
                };
                yield return muckEvent;
            }
            else if (combo.Weight == winnerCombo.Weight)
            {
                winners.Add(player.Nickname);

                var showEvent = new HoleCardsAreShownEvent
                {
                    Nickname = player.Nickname,
                    Cards = player.HoleCards,
                    Combo = combo,
                    OccurredAt = DateTime.Now
                };
                yield return showEvent;
            }
            else if (combo.Weight > winnerCombo.Weight)
            {
                winnerCombo = combo;
                winners.Clear();
                winners.Add(player.Nickname);

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

        var sidePot = pot.CalculateSidePots(winners).First();
        pot.WinSidePot(sidePot, winners);

        var awardEvent = new AwardIsCommittedEvent
        {
            Nicknames = sidePot.Competitors.ToHashSet(),
            Amount = sidePot.TotalAmount,
            OccurredAt = DateTime.Now
        };
        yield return awardEvent;
    }

    private Player GetStartPlayer(Table table, Pot pot)
    {
        if (pot.LastRaisedNickname is not null)
        {
            return table.GetPlayerByNickname((Nickname)pot.LastRaisedNickname);
        }

        return table.GetPlayerNextToSeat(table.Positions.Button, IsAvailable)!;
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

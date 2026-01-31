using Domain.Entity;
using Domain.Event;
using Domain.Exception;
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
        var startEvent = new StageStartedEvent
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

        var finishEvent = new StageFinishedEvent
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
            case HoleCardsMuckedEvent:
                break;
            case HoleCardsShownEvent:
                break;
            case SidePotAwardedEvent e:
                pot.WinSidePot(e.SidePot, e.Winners);
                break;
            case StageStartedEvent:
                break;
            case StageFinishedEvent:
                break;
            default:
                throw new InvalidHandStateException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> SubmitPlayerAction(
        Nickname nickname,
        PlayerAction action,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new PlayerActionNotAllowedException("The player cannot act during this stage");
    }

    private bool IsShowdownNeeded(Table table)
    {
        return table.Players.Count(IsAvailable) > 1;
    }

    private IEnumerable<IEvent> WinWithoutShowdown(Table table, Pot pot)
    {
        var player = table.Players.Where(IsAvailable).First();

        var muckEvent = new HoleCardsMuckedEvent
        {
            Nickname = player.Nickname,
            OccurredAt = DateTime.Now
        };
        yield return muckEvent;

        var sidePot = pot.CalculateSidePots([player.Nickname]).First();
        pot.WinSidePot(sidePot, [player.Nickname]);

        var awardEvent = new SidePotAwardedEvent
        {
            SidePot = sidePot,
            Winners = sidePot.Competitors.ToHashSet(),
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
            var showEvent = new HoleCardsShownEvent
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

            var awardEvent = new SidePotAwardedEvent
            {
                SidePot = sidePot,
                Winners = winners,
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
                var muckEvent = new HoleCardsMuckedEvent
                {
                    Nickname = player.Nickname,
                    OccurredAt = DateTime.Now
                };
                yield return muckEvent;
            }
            else if (combo.Weight == winnerCombo.Weight)
            {
                winners.Add(player.Nickname);

                var showEvent = new HoleCardsShownEvent
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

                var showEvent = new HoleCardsShownEvent
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

        var awardEvent = new SidePotAwardedEvent
        {
            SidePot = sidePot,
            Winners = sidePot.Competitors.ToHashSet(),
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

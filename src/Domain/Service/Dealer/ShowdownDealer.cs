using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class ShowdownDealer : IDealer
{
    public void Start(
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        var players = GetPlayersForShowdown(table);

        Refund(
            players: players,
            pot: pot,
            eventBus: eventBus
        );

        if (HasEnoughPlayersForShowdown(players))
        {
            WinAtShowdown(
                game: game,
                players: players,
                table: table,
                pot: pot,
                evaluator: evaluator,
                eventBus: eventBus
            );
        }
        else
        {
            WinWithoutShowdown(
                player: players.First(),
                pot: pot,
                eventBus: eventBus
            );
        }

        var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private IList<Player> GetPlayersForShowdown(Table table)
    {
        return table.Where(x => !x.IsFolded).ToList();
    }

    private bool HasEnoughPlayersForShowdown(IList<Player> players)
    {
        return players.Count > 1;
    }

    private void Refund(
        IList<Player> players,
        BasePot pot,
        IEventBus eventBus
    )
    {
        foreach (var player in players)
        {
            var amount = pot.GetRefundAmount(player);
            if (amount)
            {
                pot.CommitRefund(player, amount);

                var @event = new RefundIsCommittedEvent(
                    Nickname: player.Nickname,
                    Amount: amount,
                    OccuredAt: DateTime.Now
                );
                eventBus.Publish(@event);
            }
        }
    }

    private void WinWithoutShowdown(
        Player player,
        BasePot pot,
        IEventBus eventBus
    )
    {
        var amount = pot.GetTotalAmount();
        pot.CommitWinWithoutShowdown(player, amount);

        var showdownEvent = new HoleCardsAreMuckedEvent(
            Nickname: player.Nickname,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(showdownEvent);

        var winEvent = new WinWithoutShowdownIsCommittedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(winEvent);
    }

    private void WinAtShowdown(
        Game game,
        IList<Player> players,
        Table table,
        BasePot pot,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        var comboMapping = players.Select(x => (x.Nickname, evaluator.Evaluate(game, table.BoardCards, x.HoleCards))).ToDictionary();

        foreach (var player in players)
        {
            var showEvent = new HoleCardsAreShownEvent(
                Nickname: player.Nickname,
                Cards: player.HoleCards,
                Combo: comboMapping[player.Nickname],
                OccuredAt: DateTime.Now
            );
            eventBus.Publish(showEvent);
        }

        var sidePots = pot.GetSidePots(players);

        foreach (var sidePot in sidePots)
        {
            // We compose a set of nicknames which have the strongest combo for each side pot
            var winnerNicknames = sidePot.Nicknames.GroupBy(x => comboMapping[x].Weight).OrderByDescending(x => x.Key).First().ToHashSet();
            var winnerPlayers = players.Where(x => winnerNicknames.Contains(x.Nickname)).ToList();

            var winPot = pot.GetWinPot(winnerPlayers, sidePot);
            pot.CommitWinAtShowdown(winnerPlayers, sidePot, winPot);

            var winEvent = new WinAtShowdownIsCommittedEvent(
                SidePot: sidePot,
                WinPot: winPot,
                OccuredAt: DateTime.Now
            );
            eventBus.Publish(winEvent);
        }
    }

    public void Handle(
        BaseEvent @event,
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

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}

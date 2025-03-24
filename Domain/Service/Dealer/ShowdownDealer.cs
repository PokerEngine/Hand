using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class ShowdownDealer : IDealer
{
    public void Start(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        var players = GetPlayersForShowdown(table);
        
        Refund(
            players: players,
            handUid: handUid,
            pot: pot,
            eventBus: eventBus
        );

        if (HasEnoughPlayersForShowdown(players))
        {
            WinAtShowdown(
                handUid: handUid,
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
                handUid: handUid,
                player: players.First(),
                pot: pot,
                eventBus: eventBus
            );
        }

        var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private IList<Player> GetPlayersForShowdown(BaseTable table)
    {
        return table.Players.Where(x => x.IsAvailableForShowdown).ToList();
    }

    private bool HasEnoughPlayersForShowdown(IList<Player> players)
    {
        return players.Count > 1;
    }

    private void Refund(
        IList<Player> players,
        HandUid handUid,
        BasePot pot,
        EventBus eventBus
    )
    {
        foreach (var player in players)
        {
            var amount = pot.GetRefundAmount(player);
            if (amount)
            {
                pot.Refund(player, amount);

                var @event = new RefundIsCommittedEvent(
                    Nickname: player.Nickname,
                    Amount: amount,
                    HandUid: handUid,
                    OccuredAt: DateTime.Now
                );
                eventBus.Publish(@event);
            }
        }
    }

    private void WinWithoutShowdown(
        Player player,
        HandUid handUid,
        BasePot pot,
        EventBus eventBus
    )
    {
        var amount = pot.GetTotalAmount();

        pot.WinWithoutShowdown(player, amount);

        var showdownEvent = new HoleCardsAreMuckedEvent(
            Nickname: player.Nickname,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(showdownEvent);

        var winEvent = new WinIsCommittedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(winEvent);
    }

    private void WinAtShowdown(
        IList<Player> players,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var playerCombos = players.Select(x => (x, evaluator.Evaluate(table.BoardCards, x.HoleCards))).ToList();
        var playerComboAmounts = pot.GetWinAtShowdownAmounts(playerCombos);

        pot.WinAtShowdown(playerComboAmounts);

        foreach (var (player, combo, amount) in playerComboAmounts)
        {
            var showdownEvent = new HoleCardsAreShownEvent(
                Nickname: player.Nickname,
                Cards: player.HoleCards,
                Combo: combo,
                HandUid: handUid,
                OccuredAt: DateTime.Now
            );
            eventBus.Publish(showdownEvent);

            if (amount)
            {
                var winEvent = new WinIsCommittedEvent(
                    Nickname: player.Nickname,
                    Amount: amount,
                    HandUid: handUid,
                    OccuredAt: DateTime.Now
                );
                eventBus.Publish(winEvent);
            }
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot commit a decision during this stage");
    }
}
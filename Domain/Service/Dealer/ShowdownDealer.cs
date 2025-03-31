using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class ShowdownDealer : IDealer
{
    public void Start(
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
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

    private IList<Player> GetPlayersForShowdown(BaseTable table)
    {
        return table.Where(x => x.IsAvailableForShowdown).ToList();
    }

    private bool HasEnoughPlayersForShowdown(IList<Player> players)
    {
        return players.Count > 1;
    }

    private void Refund(
        IList<Player> players,
        BasePot pot,
        EventBus eventBus
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
        EventBus eventBus
    )
    {
        var amount = pot.GetTotalAmount();

        pot.CommitWinWithoutShowdown(player, amount);

        var showdownEvent = new HoleCardsAreMuckedEvent(
            Nickname: player.Nickname,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(showdownEvent);

        var winEvent = new WinIsCommittedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(winEvent);
    }

    private void WinAtShowdown(
        IList<Player> players,
        BaseTable table,
        BasePot pot,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var playerCombos = players.Select(x => (x, evaluator.Evaluate(table.BoardCards, x.HoleCards))).ToList();
        var playerComboAmounts = pot.GetWinAtShowdownAmounts(playerCombos);

        pot.CommitWinAtShowdown(playerComboAmounts);

        foreach (var (player, combo, amount) in playerComboAmounts)
        {
            var showdownEvent = new HoleCardsAreShownEvent(
                Nickname: player.Nickname,
                Cards: player.HoleCards,
                Combo: combo,
                OccuredAt: DateTime.Now
            );
            eventBus.Publish(showdownEvent);

            if (amount)
            {
                var winEvent = new WinIsCommittedEvent(
                    Nickname: player.Nickname,
                    Amount: amount,
                    OccuredAt: DateTime.Now
                );
                eventBus.Publish(winEvent);
            }
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
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
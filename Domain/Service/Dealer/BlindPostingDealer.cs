using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class BlindPostingDealer : IDealer
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

        PostSmallBlind(handUid: handUid, table: table, pot: pot, eventBus: eventBus);
        PostBigBlind(handUid: handUid, table: table, pot: pot, eventBus: eventBus);

        var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private void PostSmallBlind(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        EventBus eventBus
    )
    {
        Player player;

        try
        {
            player = table.GetPlayerByPosition(Position.SmallBlind);
        }
        catch (NotFoundError)
        {
            return;
        }

        var amount = player.Stake < pot.SmallBlind ? player.Stake : pot.SmallBlind;
        pot.PostSmallBlind(player, amount);

        var @event = new SmallBlindIsPostedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    private void PostBigBlind(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        EventBus eventBus
    )
    {
        Player player;

        try
        {
            player = table.GetPlayerByPosition(Position.BigBlind);
        }
        catch (NotFoundError)
        {
            return;
        }

        var amount = player.Stake < pot.BigBlind ? player.Stake : pot.BigBlind;
        pot.PostBigBlind(player, amount);

        var @event = new BigBlindIsPostedEvent(
            Nickname: player.Nickname,
            Amount: amount,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
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
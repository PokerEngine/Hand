using Domain.Entity;
using Domain.Error;
using Domain.Event;
using Domain.ValueObject;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public class BoardCardsDealingDealer : IDealer
{
    private int _count;

    public BoardCardsDealingDealer(int count)
    {
        _count = count;
    }

    public void Start(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        if (HasEnoughPlayersForDealing(table))
        {
            var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(startEvent);

            DealBoardCards(table: table, deck: deck, handUid: handUid, eventBus: eventBus);

            var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(finishEvent);
        }
        else
        {
            var skipEvent = new StageIsSkippedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
            eventBus.Publish(skipEvent);
        }
    }

    private bool HasEnoughPlayersForDealing(BaseTable table)
    {
        return table.Players.Count(x => x.IsAvailableForDealing) > 1;
    }

    private void DealBoardCards(
        BaseTable table,
        BaseDeck deck,
        HandUid handUid,
        EventBus eventBus
    )
    {
        var cards = deck.Extract(_count);
        table.TakeBoardCards(cards);

        var @event = new BoardCardsAreDealtEvent(
            Cards: cards,
            HandUid: handUid,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
    }

    public void Fold(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot fold during this stage");
    }

    public void Check(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot check during this stage");
    }

    public void CallTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot call during this stage");
    }

    public void RaiseTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot raise during this stage");
    }
}
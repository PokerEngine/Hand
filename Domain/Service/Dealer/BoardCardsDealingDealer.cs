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
        var startEvent = new StageIsStartedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        if (HasEnoughPlayersForDealing(table))
        {
            DealBoardCards(
                table: table,
                deck: deck,
                handUid: handUid,
                eventBus: eventBus
            );
        }

        var finishEvent = new StageIsFinishedEvent(HandUid: handUid, OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
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
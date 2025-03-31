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
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    )
    {
        var startEvent = new StageIsStartedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(startEvent);

        if (HasEnoughPlayersForDealing(table))
        {
            DealBoardCards(
                table: table,
                deck: deck,
                eventBus: eventBus
            );
        }

        var finishEvent = new StageIsFinishedEvent(OccuredAt: DateTime.Now);
        eventBus.Publish(finishEvent);
    }

    private bool HasEnoughPlayersForDealing(BaseTable table)
    {
        return table.Count(x => x.IsAvailableForDealing) > 1;
    }

    private void DealBoardCards(
        BaseTable table,
        BaseDeck deck,
        EventBus eventBus
    )
    {
        var cards = deck.ExtractRandomCards(_count);
        table.TakeBoardCards(cards);

        var @event = new BoardCardsAreDealtEvent(
            Cards: cards,
            OccuredAt: DateTime.Now
        );
        eventBus.Publish(@event);
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
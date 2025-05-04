using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BoardCardsDealingDealer : IDealer
{
    private int _count;

    public BoardCardsDealingDealer(int count)
    {
        _count = count;
    }

    public void Start(
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
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
        return table.Count(x => !x.IsFolded) > 1;
    }

    private void DealBoardCards(
        BaseTable table,
        BaseDeck deck,
        IEventBus eventBus
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

    public void Handle(
        IEvent @event,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator
    )
    {
        switch (@event)
        {
            case BoardCardsAreDealtEvent e:
                table.TakeBoardCards(deck.ExtractCertainCards(e.Cards));
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new NotAvailableError($"The event {@event} is not supported");
        }
    }

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        IEventBus eventBus
    )
    {
        throw new NotAvailableError("The player cannot commit a decision during this stage");
    }
}

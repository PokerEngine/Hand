using Domain.Entity;
using Domain.Event;
using Domain.Exception;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BoardCardsDealingDealer(int count) : IDealer
{
    public IEnumerable<IEvent> Start(
        HandUid uid,
        TableContext tableContext,
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
            HandUid = uid,
            TableContext = tableContext,
            OccurredAt = DateTime.UtcNow
        };
        yield return startEvent;

        if (HasEnoughPlayersForDealing(table))
        {
            yield return DealBoardCards(uid, tableContext, table, deck, randomizer);
        }

        var finishEvent = new StageFinishedEvent
        {
            HandUid = uid,
            TableContext = tableContext,
            OccurredAt = DateTime.UtcNow
        };
        yield return finishEvent;
    }

    private bool HasEnoughPlayersForDealing(Table table)
    {
        return table.Count(x => !x.IsFolded) > 1;
    }

    private BoardCardsDealtEvent DealBoardCards(HandUid uid, TableContext tableContext, Table table, BaseDeck deck, IRandomizer randomizer)
    {
        var cards = deck.ExtractRandomCards(count, randomizer);
        table.TakeBoardCards(cards);

        var @event = new BoardCardsDealtEvent
        {
            HandUid = uid,
            TableContext = tableContext,
            Cards = cards,
            OccurredAt = DateTime.UtcNow
        };
        return @event;
    }

    public void Handle(
        IEvent @event,
        HandUid uid,
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
            case BoardCardsDealtEvent e:
                table.TakeBoardCards(deck.ExtractCertainCards(e.Cards));
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
        HandUid uid,
        TableContext tableContext,
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
}

using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public class BoardCardsDealingDealer(int count) : IDealer
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
        var startEvent = new StageIsStartedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return startEvent;

        if (HasEnoughPlayersForDealing(table))
        {
            yield return DealBoardCards(table, deck, randomizer);
        }

        var finishEvent = new StageIsFinishedEvent
        {
            OccurredAt = DateTime.Now
        };
        yield return finishEvent;
    }

    private bool HasEnoughPlayersForDealing(Table table)
    {
        return table.Count(x => !x.IsFolded) > 1;
    }

    private BoardCardsAreDealtEvent DealBoardCards(Table table, BaseDeck deck, IRandomizer randomizer)
    {
        var cards = deck.ExtractRandomCards(count, randomizer);
        table.TakeBoardCards(cards);

        var @event = new BoardCardsAreDealtEvent
        {
            Cards = cards,
            OccurredAt = DateTime.Now
        };
        return @event;
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
            case BoardCardsAreDealtEvent e:
                table.TakeBoardCards(deck.ExtractCertainCards(e.Cards));
                break;
            case StageIsStartedEvent:
                break;
            case StageIsFinishedEvent:
                break;
            default:
                throw new InvalidOperationException($"{@event.GetType().Name} is not supported");
        }
    }

    public IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Rules rules,
        Table table,
        Pot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    )
    {
        throw new InvalidOperationException("The player cannot commit a decision during this stage");
    }
}

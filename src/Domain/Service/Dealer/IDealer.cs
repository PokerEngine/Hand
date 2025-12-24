using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public interface IDealer
{
    IEnumerable<IEvent> Start(
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    );

    void Handle(
        IEvent @event,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    );

    IEnumerable<IEvent> CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        Table table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    );
}

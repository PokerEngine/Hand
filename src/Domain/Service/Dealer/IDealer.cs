using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;
using Domain.Service.Randomizer;
using Domain.ValueObject;

namespace Domain.Service.Dealer;

public interface IDealer
{
    void Start(
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    );

    void Handle(
        BaseEvent @event,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator
    );

    void CommitDecision(
        Nickname nickname,
        Decision decision,
        Game game,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IRandomizer randomizer,
        IEvaluator evaluator,
        IEventBus eventBus
    );
}

using Domain.ValueObject;
using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public interface IDealer
{
    public void Start(
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );

    public void CommitDecision(
        Nickname nickname,
        Decision decision,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );
}
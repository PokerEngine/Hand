using Domain.ValueObject;
using Domain.Entity;
using Domain.Event;
using Domain.Service.Evaluator;

namespace Domain.Service.Dealer;

public interface IDealer
{
    public void Start(
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );

    public void Fold(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );

    public void Check(
        Nickname nickname,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );

    public void CallTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );

    public void RaiseTo(
        Nickname nickname,
        Chips amount,
        HandUid handUid,
        BaseTable table,
        BasePot pot,
        BaseDeck deck,
        IEvaluator evaluator,
        EventBus eventBus
    );
}
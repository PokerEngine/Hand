using Domain.ValueObject;

namespace Domain.Service.Evaluator;

public interface IEvaluator
{
    public Combo Evaluate(CardSet boardCards, CardSet holeCards);
}
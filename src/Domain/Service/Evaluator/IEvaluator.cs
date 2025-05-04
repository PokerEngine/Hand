using Domain.ValueObject;

namespace Domain.Service.Evaluator;

public interface IEvaluator
{
    public Combo Evaluate(Game game, CardSet boardCards, CardSet holeCards);
}

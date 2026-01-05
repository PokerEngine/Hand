using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Test.Service.Evaluator;

public class StubEvaluator : IEvaluator
{
    public Combo Evaluate(Game game, CardSet boardCards, CardSet holeCards)
    {
        return new Combo(type: ComboType.HighCard, weight: 1);
    }
}

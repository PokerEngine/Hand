using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Application.Test.Service.Evaluator;

public class StubEvaluator : IEvaluator
{
    public Combo Evaluate(Game game, CardSet holeCards, CardSet boardCards)
    {
        return new Combo(ComboType.HighCard, 1);
    }
}

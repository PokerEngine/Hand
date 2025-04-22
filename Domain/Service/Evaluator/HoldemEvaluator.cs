using Domain.ValueObject;

namespace Domain.Service.Evaluator;

public class HoldemEvaluator : IEvaluator
{
    public Combo Evaluate(CardSet holeCards, CardSet boardCards)
    {
        return new Combo(type: ComboType.HighCard, weight: 1);
    }
}

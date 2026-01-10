using Domain.Service.Evaluator;
using Domain.ValueObject;

namespace Domain.Test.Service.Evaluator;

public class StubEvaluator : IEvaluator
{
    private readonly Dictionary<(Game, CardSet, CardSet), Combo> _mapping = new();

    public Combo Evaluate(Game game, CardSet boardCards, CardSet holeCards)
    {
        if (!_mapping.TryGetValue((game, boardCards, holeCards), out var combo))
        {
            return new Combo(type: ComboType.HighCard, weight: 1);
        }

        return combo;
    }

    public void SetCombo(Game game, CardSet boardCards, CardSet holeCards, Combo combo)
    {
        _mapping[(game, boardCards, holeCards)] = combo;
    }
}

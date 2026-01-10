namespace Domain.ValueObject;

public enum DecisionType
{
    Fold,
    Check,
    Call,
    RaiseTo,
}

public readonly struct Decision
{
    public readonly DecisionType Type;
    public readonly Chips Amount;

    public Decision(DecisionType type, Chips amount = new())
    {
        if ((type == DecisionType.Fold || type == DecisionType.Check || type == DecisionType.Call) && !!amount)
        {
            throw new ArgumentException($"Amount must be zero for {type}", nameof(amount));
        }

        if (type == DecisionType.RaiseTo && !amount)
        {
            throw new ArgumentException($"Amount must be non-zero for {type}", nameof(amount));
        }

        Type = type;
        Amount = amount;
    }

    public override string ToString()
    {
        if (Amount)
        {
            return $"{Type} [{Amount}]";
        }
        return $"{Type}";
    }
}

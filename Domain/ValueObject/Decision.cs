namespace Domain.ValueObject;

public enum DecisionType
{
    Fold,
    Check,
    CallTo,
    RaiseTo,
}

public readonly struct Decision
{
    public DecisionType Type { get; }
    public Chips Amount { get; }

    public Decision(DecisionType type, Chips amount)
    {
        if ((type == DecisionType.Fold || type == DecisionType.Check) && !!amount)
        {
            throw new ArgumentException($"Amount must be zero for {type}");
        }

        if ((type == DecisionType.CallTo || type == DecisionType.RaiseTo) && !amount)
        {
            throw new ArgumentException($"Amount must be non-zero for {type}");
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
namespace Domain.ValueObject;

public enum PlayerActionType
{
    Fold,
    Check,
    CallBy,
    RaiseBy
}

public readonly struct PlayerAction : IEquatable<PlayerAction>
{
    public readonly PlayerActionType Type;
    public readonly Chips Amount;

    public PlayerAction(PlayerActionType type, Chips amount = new())
    {
        if ((type == PlayerActionType.Fold || type == PlayerActionType.Check) && !amount.IsZero)
        {
            throw new ArgumentException($"Amount must be zero for {type}", nameof(amount));
        }

        if ((type == PlayerActionType.CallBy || type == PlayerActionType.RaiseBy) && amount.IsZero)
        {
            throw new ArgumentException($"Amount must be non-zero for {type}", nameof(amount));
        }

        Type = type;
        Amount = amount;
    }

    public bool Equals(PlayerAction other)
        => Type.Equals(other.Type) && Amount.Equals(other.Amount);

    public override string ToString()
    {
        if (Amount)
        {
            return $"{Type} [{Amount}]";
        }
        return $"{Type}";
    }
}

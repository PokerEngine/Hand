namespace Domain.ValueObject;

public readonly struct Award : IEquatable<Award>
{
    public IReadOnlySet<Nickname> Winners { get; init; }
    public Chips Amount { get; init; }

    public bool Equals(Award other)
        => Winners.Equals(other.Winners) && Amount.Equals(other.Amount);

    public override string ToString()
        => $"{Amount}: {Winners}";
}

namespace Domain.ValueObject;

public readonly struct SidePot : IEquatable<SidePot>
{
    public readonly IReadOnlySet<Nickname> Nicknames;
    public readonly Chips Amount;

    public SidePot(HashSet<Nickname> nicknames, Chips amount)
    {
        Nicknames = nicknames;
        Amount = amount;
    }

    public bool Equals(SidePot other)
        => Nicknames.Equals(other.Nicknames) && Amount.Equals(other.Amount);

    public override string ToString()
        => $"{Amount}: {Nicknames}";
}

namespace Domain.ValueObject;

public readonly struct RefactoredSidePot : IEquatable<RefactoredSidePot>
{
    public readonly IReadOnlySet<Nickname> Nicknames;
    public readonly Chips Amount;

    public RefactoredSidePot(HashSet<Nickname> nicknames, Chips amount)
    {
        Nicknames = nicknames;
        Amount = amount;
    }

    public bool Equals(RefactoredSidePot other)
        => Nicknames.Equals(other.Nicknames) && Amount.Equals(other.Amount);

    public override string ToString()
        => $"{Amount}: {Nicknames}";
}

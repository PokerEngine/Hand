namespace Domain.ValueObject;

public readonly struct SidePot : IEquatable<SidePot>
{
    public readonly IReadOnlySet<Nickname> Competitors;
    public readonly Bets Bets;
    public readonly Chips Ante;
    public Chips TotalAmount => Ante + Bets.TotalAmount;

    public SidePot(HashSet<Nickname> competitors, Bets bets, Chips ante)
    {
        Competitors = competitors;
        Bets = bets;
        Ante = ante;
    }

    public bool Equals(SidePot other)
        => Competitors.SetEquals(other.Competitors) && Bets.Equals(other.Bets) && Ante.Equals(other.Ante);

    public override string ToString()
    {
        var str = String.Join(", ", Competitors);
        return $"{TotalAmount}: {{{str}}}";
    }
}

namespace Domain.ValueObject;

public enum ComboType
{
    HighCard,
    OnePair,
    TwoPair,
    Trips,
    Straight,
    Flush,
    FullHouse,
    Quads,
    StraightFlush,
}

public readonly struct Combo : IComparable<Combo>, IEquatable<Combo>
{
    public readonly ComboType Type;
    public readonly int Weight;

    public Combo(ComboType type, int weight)
    {
        Type = type;
        Weight = weight;
    }

    public int CompareTo(Combo other)
        => Weight.CompareTo(other.Weight);

    public bool Equals(Combo other)
        => Type.Equals(other.Type) && Weight.Equals(other.Weight);

    public override string ToString()
        => $"{Type} [{Weight}]";
}

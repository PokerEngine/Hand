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

    public static bool operator >(Combo a, Combo b)
        => a.Weight > b.Weight;

    public static bool operator <(Combo a, Combo b)
        => a.Weight < b.Weight;

    public static bool operator >=(Combo a, Combo b)
        => a.Weight >= b.Weight;

    public static bool operator <=(Combo a, Combo b)
        => a.Weight <= b.Weight;

    public static bool operator ==(Combo a, Combo b)
        => a.Weight == b.Weight;

    public static bool operator !=(Combo a, Combo b)
        => a.Weight != b.Weight;

    public int CompareTo(Combo other)
        => Weight.CompareTo(other.Weight);

    public bool Equals(Combo other)
        => Weight.Equals(other.Weight);

    public override string ToString()
        => $"{Type} [{Weight}]";

    public override int GetHashCode()
        => Weight.GetHashCode();
}
namespace Domain.ValueObject;

public readonly struct Seat : IComparable<Seat>, IEquatable<Seat>
{
    private readonly int _number;

    public Seat(int number)
    {
        if (number < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, "Seat must be greater or equal to one");
        }

        _number = number;
    }

    public static implicit operator int(Seat a)
        => a._number;

    public static explicit operator Seat(int a)
        => new(a);

    public static bool operator ==(Seat a, Seat b)
        => a._number == b._number;

    public static bool operator !=(Seat a, Seat b)
        => a._number != b._number;

    public static bool operator >(Seat a, Seat b)
        => a._number > b._number;

    public static bool operator <(Seat a, Seat b)
        => a._number < b._number;

    public static bool operator >=(Seat a, Seat b)
        => a._number >= b._number;

    public static bool operator <=(Seat a, Seat b)
        => a._number <= b._number;

    public int CompareTo(Seat other)
        => _number.CompareTo(other._number);

    public bool Equals(Seat other)
        => _number.Equals(other._number);

    public override bool Equals(object? o)
        => o is not null && o.GetType() == GetType() && _number.Equals(((Seat)o)._number);

    public override int GetHashCode()
        => _number.GetHashCode();

    public override string ToString()
        => $"#{_number}";
}

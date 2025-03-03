using System.Numerics;

namespace Domain.ValueObject;

public readonly struct Chips : IMinMaxValue<Chips>, IComparable<Chips>, IEquatable<Chips>
{
    private int Amount { get; }
    public static Chips MinValue
    {
        get => new Chips(0);
    }
    public static Chips MaxValue
    {
        get => new Chips(int.MaxValue);
    }

    public Chips(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Chips amount must be a non-negative integer");
        }

        Amount = amount;
    }

    public static implicit operator int(Chips a)
        => a.Amount;

    public static explicit operator Chips(int a)
        => new (a);

    public static Chips operator +(Chips a)
        => a;

    public static Chips operator -(Chips a)
        => new (-a.Amount);

    public static Chips operator +(Chips a, Chips b)
        => new (a.Amount + b.Amount);

    public static Chips operator -(Chips a, Chips b)
        => new (a.Amount - b.Amount);

    public static Chips operator *(Chips a, int b)
        => new (a.Amount * b);

    public static Chips operator /(Chips a, int b)
        => new Chips(a.Amount / b);

    public static Chips operator %(Chips a, int b)
        => new (a.Amount % b);

    public static bool operator !(Chips a)
        => a.Amount == 0;

    public static bool operator ==(Chips a, Chips b)
        => a.Amount == b.Amount;

    public static bool operator !=(Chips a, Chips b)
        => a.Amount != b.Amount;

    public static bool operator >(Chips a, Chips b)
        => a.Amount > b.Amount;

    public static bool operator <(Chips a, Chips b)
        => a.Amount < b.Amount;

    public static bool operator >=(Chips a, Chips b)
        => a.Amount >= b.Amount;

    public static bool operator <=(Chips a, Chips b)
        => a.Amount <= b.Amount;

    public static bool operator true(Chips a)
        => a.Amount != 0;

    public static bool operator false(Chips a)
        => a.Amount == 0;

    public int CompareTo(Chips other)
        => Amount.CompareTo(other.Amount);

    public bool Equals(Chips other)
        => Amount.Equals(other.Amount);

    public override int GetHashCode()
        => Amount.GetHashCode();

    public override string ToString()
        => $"{Amount} chip(s)";
}
namespace Domain.ValueObject;

public readonly struct HandUid
{
    private readonly Guid Value;

    public HandUid(Guid value)
    {
        Value = value;
    }

    public static implicit operator Guid(HandUid a)
        => a.Value;

    public static explicit operator HandUid(Guid a)
        => new (a);

    public static bool operator ==(HandUid a, HandUid b)
        => a.Value == b.Value;

    public static bool operator !=(HandUid a, HandUid b)
        => a.Value != b.Value;

    public override string ToString()
        => Value.ToString();

    public override int GetHashCode()
        => Value.GetHashCode();
}
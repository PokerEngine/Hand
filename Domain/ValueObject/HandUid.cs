namespace Domain.ValueObject;

public readonly struct HandUid
{
    private readonly Guid _guid;

    public HandUid(Guid guid)
    {
        _guid = guid;
    }

    public static implicit operator Guid(HandUid a)
        => a._guid;

    public static explicit operator HandUid(Guid a)
        => new (a);

    public static bool operator ==(HandUid a, HandUid b)
        => a._guid == b._guid;

    public static bool operator !=(HandUid a, HandUid b)
        => a._guid != b._guid;

    public override string ToString()
        => _guid.ToString();

    public override int GetHashCode()
        => _guid.GetHashCode();
}
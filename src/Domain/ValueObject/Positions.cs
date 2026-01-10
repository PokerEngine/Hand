namespace Domain.ValueObject;

public readonly struct Positions
{
    public Seat SmallBlind { get; init; }
    public Seat BigBlind { get; init; }
    public Seat Button { get; init; }
    public Seat Max { get; init; }
}

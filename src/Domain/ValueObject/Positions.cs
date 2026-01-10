namespace Domain.ValueObject;

public readonly struct Positions
{
    public required Seat SmallBlind { get; init; }
    public required Seat BigBlind { get; init; }
    public required Seat Button { get; init; }
    public required Seat Max { get; init; }
}

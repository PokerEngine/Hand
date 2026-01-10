namespace Domain.ValueObject;

public readonly struct Rules
{
    public required Game Game { get; init; }
    public required Chips SmallBlind { get; init; }
    public required Chips BigBlind { get; init; }
}

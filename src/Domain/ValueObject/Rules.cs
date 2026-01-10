namespace Domain.ValueObject;

public readonly struct Rules
{
    public Game Game { get; init; }
    public Chips SmallBlind { get; init; }
    public Chips BigBlind { get; init; }
}

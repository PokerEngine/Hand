namespace Domain.ValueObject;

public readonly struct State
{
    public required List<StatePlayer> Players { get; init; }
    public required CardSet BoardCards { get; init; }
    public required SidePot CurrentSidePot { get; init; }
    public required SidePot PreviousSidePot { get; init; }
}

public readonly struct StatePlayer
{
    public required Nickname Nickname { get; init; }
    public required Seat Seat { get; init; }
    public required Chips Stack { get; init; }
    public required CardSet HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

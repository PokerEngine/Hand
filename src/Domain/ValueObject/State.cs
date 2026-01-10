namespace Domain.ValueObject;

public readonly struct State
{
    public required TableState Table { get; init; }
    public required PotState Pot { get; init; }
}

public readonly struct TableState
{
    public required CardSet BoardCards { get; init; }
    public required List<PlayerState> Players { get; init; }
}

public readonly struct PlayerState
{
    public required Seat Seat { get; init; }
    public required Nickname Nickname { get; init; }
    public required Chips Stack { get; init; }
    public required CardSet HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public readonly struct PotState
{
    public required Chips Ante { get; init; }
    public required List<BetState> CommittedBets { get; init; }
    public required List<BetState> UncommittedBets { get; init; }
}

public readonly struct BetState
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

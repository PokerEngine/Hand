namespace Domain.ValueObject;

public record State
{
    public required Rules Rules { get; init; }
    public required TableState Table { get; init; }
    public required PotState Pot { get; init; }
}

public record TableState
{
    public required CardSet BoardCards { get; init; }
    public required Positions Positions { get; init; }
    public required List<PlayerState> Players { get; init; }
}

public record PlayerState
{
    public required Seat Seat { get; init; }
    public required Nickname Nickname { get; init; }
    public required Chips Stack { get; init; }
    public required CardSet HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public record PotState
{
    public required Chips Ante { get; init; }
    public required List<BetState> CollectedBets { get; init; }
    public required List<BetState> CurrentBets { get; init; }
    public required List<AwardState> Awards { get; init; }
}

public record BetState
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record AwardState
{
    public required List<Nickname> Winners { get; init; }
    public required Chips Amount { get; init; }
}

using Domain.Entity;
using Domain.ValueObject;

namespace Application.Storage;

public interface IStorage
{
    Task<DetailView> GetDetailViewAsync(HandUid tableUid);
    Task SaveViewAsync(Hand hand);
}

public record DetailView
{
    public required Guid Uid { get; init; }
    public required Guid TableUid { get; init; }
    public required string TableType { get; init; }
    public required DetailViewRules Rules { get; init; }
    public required DetailViewTable Table { get; init; }
    public required DetailViewPot Pot { get; init; }
}

public record DetailViewRules
{
    public required string Game { get; init; }
    public required int SmallBlind { get; init; }
    public required int BigBlind { get; init; }
    public required int MaxSeat { get; init; }
}

public record DetailViewTable
{
    public required DetailViewPositions Positions { get; init; }
    public required List<DetailViewPlayer> Players { get; init; }
    public required string BoardCards { get; init; }
}

public record DetailViewPositions
{
    public required int SmallBlindSeat { get; init; }
    public required int BigBlindSeat { get; init; }
    public required int ButtonSeat { get; init; }
}

public record DetailViewPlayer
{
    public required string Nickname { get; init; }
    public required int Seat { get; init; }
    public required int Stack { get; init; }
    public required string HoleCards { get; init; }
    public required bool IsFolded { get; init; }
}

public record DetailViewPot
{
    public required int Ante { get; init; }
    public required List<DetailViewBet> CollectedBets { get; init; }
    public required List<DetailViewBet> CurrentBets { get; init; }
    public required List<DetailViewAward> Awards { get; init; }
}

public record DetailViewBet
{
    public required string Nickname { get; init; }
    public required int Amount { get; init; }
}

public record DetailViewAward
{
    public required List<string> Winners { get; init; }
    public required int Amount { get; init; }
}

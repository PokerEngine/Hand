using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    DateTime OccurredAt { init; get; }
}

public record struct HandIsCreatedEvent : IEvent
{
    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
    public required Game Game { get; init; }
    public required Seat MaxSeat { get; init; }
    public required Chips SmallBlind { get; init; }
    public required Chips BigBlind { get; init; }
    public required Seat SmallBlindSeat { get; init; }
    public required Seat BigBlindSeat { get; init; }
    public required Seat ButtonSeat { get; init; }
    public required List<Participant> Participants { get; init; }
    public required DateTime OccurredAt { get; init; }

    public bool Equals(HandIsCreatedEvent other)
    {
        return TableUid.Equals(other.TableUid)
               && TableType.Equals(other.TableType)
               && Game.Equals(other.Game)
               && MaxSeat.Equals(other.MaxSeat)
               && SmallBlind.Equals(other.SmallBlind)
               && BigBlind.Equals(other.BigBlind)
               && SmallBlindSeat.Equals(other.SmallBlindSeat)
               && BigBlindSeat.Equals(other.BigBlindSeat)
               && ButtonSeat.Equals(other.ButtonSeat)
               && OccurredAt.Equals(other.OccurredAt)
               && Participants.SequenceEqual(other.Participants);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(TableUid);
        hash.Add(TableType);
        hash.Add(Game);
        hash.Add(MaxSeat);
        hash.Add(SmallBlind);
        hash.Add(BigBlind);
        hash.Add(SmallBlindSeat);
        hash.Add(BigBlindSeat);
        hash.Add(ButtonSeat);
        hash.Add(OccurredAt);

        foreach (var participant in Participants)
        {
            hash.Add(participant);
        }

        return hash.ToHashCode();
    }
}

public record struct HandIsStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct HandIsFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct StageIsStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct StageIsFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct SmallBlindIsPostedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct BigBlindIsPostedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct HoleCardsAreDealtEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct BoardCardsAreDealtEvent : IEvent
{
    public required CardSet Cards { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct DecisionIsRequestedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required Chips CallToAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required Chips MinRaiseToAmount { get; init; }
    public required Chips MaxRaiseToAmount { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct DecisionIsCommittedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required Decision Decision { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct RefundIsCommittedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct WinWithoutShowdownIsCommittedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct WinAtShowdownIsCommittedEvent : IEvent
{
    public required SidePot SidePot { get; init; }
    public required SidePot WinPot { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct HoleCardsAreMuckedEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required DateTime OccurredAt { get; init; }
}

public record struct HoleCardsAreShownEvent : IEvent
{
    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
    public required Combo Combo { get; init; }
    public required DateTime OccurredAt { get; init; }
}

using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    DateTime OccurredAt { init; get; }
}

public record struct HandIsCreatedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
    public required Rules Rules { get; init; }
    public required Positions Positions { get; init; }
    public required List<Participant> Participants { get; init; }

    public bool Equals(HandIsCreatedEvent other)
    {
        return OccurredAt.Equals(other.OccurredAt)
               && TableUid.Equals(other.TableUid)
               && TableType.Equals(other.TableType)
               && Rules.Equals(other.Rules)
               && Positions.Equals(other.Positions)
               && Participants.SequenceEqual(other.Participants);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(TableUid);
        hash.Add(TableType);
        hash.Add(Rules);
        hash.Add(Positions);

        foreach (var participant in Participants)
        {
            hash.Add(participant);
        }

        hash.Add(OccurredAt);

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
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct BigBlindIsPostedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct HoleCardsAreDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
}

public record struct BoardCardsAreDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required CardSet Cards { get; init; }
}

public record struct DecisionIsRequestedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required Chips CallToAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required Chips MinRaiseToAmount { get; init; }
    public required Chips MaxRaiseToAmount { get; init; }
}

public record struct DecisionIsCommittedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Decision Decision { get; init; }
}

public record struct RefundIsCommittedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct AwardIsCommittedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required HashSet<Nickname> Nicknames { get; init; } // In case of splitting the pot
    public required Chips Amount { get; init; }
}

public record struct HoleCardsAreMuckedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
}

public record struct HoleCardsAreShownEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
    public required Combo Combo { get; init; }
}

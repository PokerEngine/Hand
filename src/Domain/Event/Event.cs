using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    DateTime OccurredAt { init; get; }
}

public record struct HandCreatedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
    public required Rules Rules { get; init; }
    public required Positions Positions { get; init; }
    public required List<Participant> Participants { get; init; }

    public bool Equals(HandCreatedEvent other)
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

public record struct HandStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct HandFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct StageStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct StageFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct SmallBlindPostedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct BigBlindPostedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct HoleCardsDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
}

public record struct BoardCardsDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required CardSet Cards { get; init; }
}

public record struct PlayerActionRequestedEvent : IEvent
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

public record struct PlayerActedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required PlayerAction Action { get; init; }
}

public record struct BetRefundedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public record struct BetsCollectedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public record struct SidePotAwardedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required HashSet<Nickname> Winners { get; init; } // In case of splitting the pot
    public required SidePot SidePot { get; init; }
}

public record struct HoleCardsMuckedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
}

public record struct HoleCardsShownEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
    public required Combo Combo { get; init; }
}

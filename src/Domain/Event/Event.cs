using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    DateTime OccurredAt { init; get; }
}

public sealed record HandStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required TableUid TableUid { get; init; }
    public required TableType TableType { get; init; }
    public required Rules Rules { get; init; }
    public required Positions Positions { get; init; }
    public required List<Participant> Participants { get; init; }

    public bool Equals(HandStartedEvent? other)
    {
        return other is not null
               && OccurredAt.Equals(other.OccurredAt)
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

public sealed record HandFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public sealed record StageStartedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public sealed record StageFinishedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public sealed record SmallBlindPostedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public sealed record BigBlindPostedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public sealed record HoleCardsDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
}

public sealed record BoardCardsDealtEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required CardSet Cards { get; init; }
}

public sealed record PlayerActionRequestedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required bool FoldIsAvailable { get; init; }
    public required bool CheckIsAvailable { get; init; }
    public required bool CallIsAvailable { get; init; }
    public required Chips CallByAmount { get; init; }
    public required bool RaiseIsAvailable { get; init; }
    public required Chips MinRaiseByAmount { get; init; }
    public required Chips MaxRaiseByAmount { get; init; }
}

public sealed record PlayerActedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required PlayerAction Action { get; init; }
}

public sealed record BetRefundedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required Chips Amount { get; init; }
}

public sealed record BetsCollectedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }
}

public sealed record SidePotAwardedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required HashSet<Nickname> Winners { get; init; } // In case of splitting the pot
    public required SidePot SidePot { get; init; }
}

public sealed record HoleCardsMuckedEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
}

public sealed record HoleCardsShownEvent : IEvent
{
    public required DateTime OccurredAt { get; init; }

    public required Nickname Nickname { get; init; }
    public required CardSet Cards { get; init; }
    public required Combo Combo { get; init; }
}

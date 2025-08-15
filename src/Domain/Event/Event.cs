using Domain.ValueObject;
using System.Collections.Immutable;

namespace Domain.Event;

public abstract record BaseEvent(
    DateTime OccuredAt
);

public record HandIsCreatedEvent(
    Game Game,
    Chips SmallBlind,
    Chips BigBlind,
    ImmutableList<Participant> Participants,
    DateTime OccuredAt
) : BaseEvent(OccuredAt)
{
    public virtual bool Equals(HandIsCreatedEvent? other)
    {
        // We override it to check participants equality by value
        return other is not null
               && Game == other.Game
               && SmallBlind == other.SmallBlind
               && BigBlind == other.BigBlind
               && Participants.SequenceEqual(other.Participants)
               && OccuredAt == other.OccuredAt;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Game);
        hash.Add(SmallBlind);
        hash.Add(BigBlind);
        foreach (var p in Participants)
            hash.Add(p);
        hash.Add(OccuredAt);
        return hash.ToHashCode();
    }
}

public record HandIsStartedEvent(
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record HandIsFinishedEvent(
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record StageIsStartedEvent(
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record StageIsFinishedEvent(
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record PlayerConnectedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record PlayerDisconnectedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record SmallBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record BigBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record HoleCardsAreDealtEvent(
    Nickname Nickname,
    CardSet Cards,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record BoardCardsAreDealtEvent(
    CardSet Cards,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record DecisionIsRequestedEvent(
    Nickname Nickname,
    bool FoldIsAvailable,
    bool CheckIsAvailable,
    bool CallIsAvailable,
    Chips CallToAmount,
    bool RaiseIsAvailable,
    Chips MinRaiseToAmount,
    Chips MaxRaiseToAmount,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record DecisionIsCommittedEvent(
    Nickname Nickname,
    Decision Decision,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record RefundIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record WinWithoutShowdownIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record WinAtShowdownIsCommittedEvent(
    SidePot SidePot,
    SidePot WinPot,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record HoleCardsAreMuckedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

public record HoleCardsAreShownEvent(
    Nickname Nickname,
    CardSet Cards,
    Combo Combo,
    DateTime OccuredAt
) : BaseEvent(OccuredAt);

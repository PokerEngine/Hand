using System.Collections.Immutable;

namespace Application.IntegrationEvent;

public record IntegrationEventParticipant(
    string Nickname,
    string Position,
    int Stake
);

public interface IIntegrationEvent
{
    Guid TableUid { init; get; }
    Guid HandUid { init; get; }
    DateTime OccuredAt { init; get; }
}

/* Incoming events aka commands */
public record HandCreateIntegrationEvent(
    string Game,
    int SmallBlind,
    int BigBlind,
    ImmutableList<IntegrationEventParticipant> Participants,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent
{
    public virtual bool Equals(HandCreateIntegrationEvent? other)
    {
        // We override it to check participants equality by value
        return other is not null
               && Game == other.Game
               && SmallBlind == other.SmallBlind
               && BigBlind == other.BigBlind
               && Participants.SequenceEqual(other.Participants)
               && TableUid == other.TableUid
               && HandUid == other.HandUid
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
        hash.Add(TableUid);
        hash.Add(HandUid);
        hash.Add(OccuredAt);
        return hash.ToHashCode();
    }
}

public record HandStartIntegrationEvent(
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record PlayerConnectIntegrationEvent(
    string Nickname,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record PlayerDisconnectIntegrationEvent(
    string Nickname,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record DecisionCommitIntegrationEvent(
    string Nickname,
    string DecisionType,
    int DecisionAmount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

/* Outcoming events */
public record HandIsCreatedIntegrationEvent(
    string Game,
    int SmallBlind,
    int BigBlind,
    ImmutableList<IntegrationEventParticipant> Participants,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent
{
    public virtual bool Equals(HandIsCreatedIntegrationEvent? other)
    {
        // We override it to check participants equality by value
        return other is not null
               && Game == other.Game
               && SmallBlind == other.SmallBlind
               && BigBlind == other.BigBlind
               && Participants.SequenceEqual(other.Participants)
               && TableUid == other.TableUid
               && HandUid == other.HandUid
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
        hash.Add(TableUid);
        hash.Add(HandUid);
        hash.Add(OccuredAt);
        return hash.ToHashCode();
    }
}

public record HandIsStartedIntegrationEvent(
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record HandIsFinishedIntegrationEvent(
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record PlayerConnectedIntegrationEvent(
    string Nickname,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record PlayerDisconnectedIntegrationEvent(
    string Nickname,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record BlindIsPostedIntegrationEvent(
    string Nickname,
    int Amount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record HoleCardsAreDealtIntegrationEvent(
    string Nickname,
    string Cards,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record BoardCardsAreDealtIntegrationEvent(
    string Cards,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record DecisionIsRequestedIntegrationEvent(
    string Nickname,
    bool FoldIsAvailable,
    bool CheckIsAvailable,
    bool CallIsAvailable,
    int CallToAmount,
    bool RaiseIsAvailable,
    int MinRaiseToAmount,
    int MaxRaiseToAmount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record DecisionIsCommittedIntegrationEvent(
    string Nickname,
    string DecisionType,
    int DecisionAmount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record RefundIsCommittedIntegrationEvent(
    string Nickname,
    int Amount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record WinWithoutShowdownIsCommittedIntegrationEvent(
    string Nickname,
    int Amount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record WinAtShowdownIsCommittedIntegrationEvent(
    string Nickname,
    int Amount,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record HoleCardsAreShownIntegrationEvent(
    string Nickname,
    string Cards,
    string ComboType,
    int ComboWeight,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record HoleCardsAreMuckedIntegrationEvent(
    string Nickname,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

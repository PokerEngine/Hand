using System.Collections.Immutable;

namespace Application.IntegrationEvent;

public record IntegrationEventParticipant(
    string Nickname,
    string Position,
    int Stake
);

public interface IIntegrationEvent
{
    public Guid TableUid { init; get; }
    public Guid HandUid { init; get; }
    public DateTime OccuredAt { init; get; }
}

/* Incoming events aka commands */
public record HandCreateIntegrationEvent(
    string Game,
    int SmallBlind,
    int BigBlind,
    List<IntegrationEventParticipant> Participants,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

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
) : IIntegrationEvent;

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
    List<string> Cards,
    Guid TableUid,
    Guid HandUid,
    DateTime OccuredAt
) : IIntegrationEvent;

public record BoardCardsAreDealtIntegrationEvent(
    List<string> Cards,
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
    List<string> Cards,
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

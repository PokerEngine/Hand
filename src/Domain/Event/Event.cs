using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    public DateTime OccuredAt { init; get; }
}

public record HandIsCreatedEvent(
    Game Game,
    Chips SmallBlind,
    Chips BigBlind,
    List<Participant> Participants,
    DateTime OccuredAt
) : IEvent;

public record HandIsStartedEvent(
    DateTime OccuredAt
) : IEvent;

public record HandIsFinishedEvent(
    DateTime OccuredAt
) : IEvent;

public record StageIsStartedEvent(
    DateTime OccuredAt
) : IEvent;

public record StageIsFinishedEvent(
    DateTime OccuredAt
) : IEvent;

public record PlayerConnectedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : IEvent;

public record PlayerDisconnectedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : IEvent;

public record SmallBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : IEvent;

public record BigBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreDealtEvent(
    Nickname Nickname,
    CardSet Cards,
    DateTime OccuredAt
) : IEvent;

public record BoardCardsAreDealtEvent(
    CardSet Cards,
    DateTime OccuredAt
) : IEvent;

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
) : IEvent;

public record DecisionIsCommittedEvent(
    Nickname Nickname,
    Decision Decision,
    DateTime OccuredAt
) : IEvent;

public record RefundIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : IEvent;

public record WinWithoutShowdownIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    DateTime OccuredAt
) : IEvent;

public record WinAtShowdownIsCommittedEvent(
    SidePot SidePot,
    SidePot WinPot,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreShownEvent(
    Nickname Nickname,
    CardSet Cards,
    Combo Combo,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreMuckedEvent(
    Nickname Nickname,
    DateTime OccuredAt
) : IEvent;

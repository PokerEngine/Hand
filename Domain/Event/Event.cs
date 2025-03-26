using Domain.ValueObject;

namespace Domain.Event;

public interface IEvent
{
    public HandUid HandUid { init; get; }
    public DateTime OccuredAt { init; get; }
}

public record HandIsCreatedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record HandIsStartedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record HandIsFinishedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record StageIsStartedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record StageIsFinishedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record PlayerConnectedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record PlayerDisconnectedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record SmallBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record BigBlindIsPostedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record DecisionIsCommittedEvent(
    Nickname Nickname,
    Decision Decision,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record RefundIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record WinIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreShownEvent(
    Nickname Nickname,
    CardSet Cards,
    Combo Combo,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreMuckedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record HoleCardsAreDealtEvent(
    Nickname Nickname,
    CardSet Cards,
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

public record BoardCardsAreDealtEvent(
    CardSet Cards,
    HandUid HandUid,
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
    HandUid HandUid,
    DateTime OccuredAt
) : IEvent;

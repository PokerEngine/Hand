using Domain.ValueObject;

namespace Domain.Event;

public abstract record BaseEvent(
    HandUid HandUid,
    DateTime OccuredAt
);

public record HandIsCreatedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record HandIsStartedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record HandIsFinishedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record StageIsStartedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record StageIsFinishedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record StageIsSkippedEvent(
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerConnectedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerDisconnectedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerPostedSmallBlindEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerPostedBigBlindEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerFoldedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerCheckedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerCalledToEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record PlayerRaisedToEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record RefundIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record WinIsCommittedEvent(
    Nickname Nickname,
    Chips Amount,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record HoleCardsAreShownEvent(
    Nickname Nickname,
    CardSet Cards,
    Combo Combo,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record HoleCardsAreMuckedEvent(
    Nickname Nickname,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record HoleCardsAreDealtEvent(
    Nickname Nickname,
    CardSet Cards,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

public record BoardCardsAreDealtEvent(
    CardSet Cards,
    HandUid HandUid,
    DateTime OccuredAt
) : BaseEvent(HandUid, OccuredAt);

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
) : BaseEvent(HandUid, OccuredAt);

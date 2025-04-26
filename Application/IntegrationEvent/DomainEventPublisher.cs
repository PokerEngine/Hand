using Domain.Event;
using Domain.ValueObject;

namespace Application.IntegrationEvent;

public class DomainEventPublisher
{
    private readonly IIntegrationEventBus _integrationEventBus;
    private readonly Guid _tableUid;
    private readonly Guid _handUid;

    public DomainEventPublisher(
        IIntegrationEventBus integrationEventBus,
        Guid tableUid,
        Guid handUid
    )
    {
        _integrationEventBus = integrationEventBus;
        _tableUid = tableUid;
        _handUid = handUid;
    }

    public void Publish(IEnumerable<IEvent> events)
    {
        var eventBus = new EventBus();

        eventBus.Subscribe<HandIsCreatedEvent>(HandleHandIsCreated);
        eventBus.Subscribe<HandIsStartedEvent>(HandleHandIsStarted);
        eventBus.Subscribe<HandIsFinishedEvent>(HandleHandIsFinished);
        eventBus.Subscribe<PlayerConnectedEvent>(HandlePlayerConnected);
        eventBus.Subscribe<PlayerDisconnectedEvent>(HandlePlayerDisconnected);
        eventBus.Subscribe<SmallBlindIsPostedEvent>(HandleSmallBlindIsPosted);
        eventBus.Subscribe<BigBlindIsPostedEvent>(HandleBigBlindIsPosted);
        eventBus.Subscribe<HoleCardsAreDealtEvent>(HandleHoleCardsAreDealt);
        eventBus.Subscribe<BoardCardsAreDealtEvent>(HandleBoardCardsAreDealt);
        eventBus.Subscribe<DecisionIsRequestedEvent>(HandleDecisionIsRequested);
        eventBus.Subscribe<DecisionIsCommittedEvent>(HandleDecisionIsCommitted);
        eventBus.Subscribe<RefundIsCommittedEvent>(HandleRefundIsCommitted);
        eventBus.Subscribe<WinWithoutShowdownIsCommittedEvent>(HandleWinWithoutShowdownIsCommitted);
        eventBus.Subscribe<WinAtShowdownIsCommittedEvent>(HandleWinAtShowdownIsCommitted);
        eventBus.Subscribe<HoleCardsAreShownEvent>(HandleHoleCardsAreShown);
        eventBus.Subscribe<HoleCardsAreMuckedEvent>(HandleHoleCardsAreMucked);

        foreach (var @event in events)
        {
            eventBus.Publish(@event);
        }

        eventBus.Unsubscribe<HandIsCreatedEvent>(HandleHandIsCreated);
        eventBus.Unsubscribe<HandIsStartedEvent>(HandleHandIsStarted);
        eventBus.Unsubscribe<HandIsFinishedEvent>(HandleHandIsFinished);
        eventBus.Unsubscribe<PlayerConnectedEvent>(HandlePlayerConnected);
        eventBus.Unsubscribe<PlayerDisconnectedEvent>(HandlePlayerDisconnected);
        eventBus.Unsubscribe<SmallBlindIsPostedEvent>(HandleSmallBlindIsPosted);
        eventBus.Unsubscribe<BigBlindIsPostedEvent>(HandleBigBlindIsPosted);
        eventBus.Unsubscribe<HoleCardsAreDealtEvent>(HandleHoleCardsAreDealt);
        eventBus.Unsubscribe<BoardCardsAreDealtEvent>(HandleBoardCardsAreDealt);
        eventBus.Unsubscribe<DecisionIsRequestedEvent>(HandleDecisionIsRequested);
        eventBus.Unsubscribe<DecisionIsCommittedEvent>(HandleDecisionIsCommitted);
        eventBus.Unsubscribe<RefundIsCommittedEvent>(HandleRefundIsCommitted);
        eventBus.Unsubscribe<WinWithoutShowdownIsCommittedEvent>(HandleWinWithoutShowdownIsCommitted);
        eventBus.Unsubscribe<WinAtShowdownIsCommittedEvent>(HandleWinAtShowdownIsCommitted);
        eventBus.Unsubscribe<HoleCardsAreShownEvent>(HandleHoleCardsAreShown);
        eventBus.Unsubscribe<HoleCardsAreMuckedEvent>(HandleHoleCardsAreMucked);
    }

    private void HandleHandIsCreated(HandIsCreatedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HandIsCreatedIntegrationEvent(
            Participants: @event.Participants.Select(ProcessParticipant).ToList(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleHandIsStarted(HandIsStartedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HandIsStartedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleHandIsFinished(HandIsFinishedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HandIsFinishedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandlePlayerConnected(PlayerConnectedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new PlayerConnectedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandlePlayerDisconnected(PlayerDisconnectedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new PlayerDisconnectedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleSmallBlindIsPosted(SmallBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleBigBlindIsPosted(BigBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleHoleCardsAreDealt(HoleCardsAreDealtEvent @event)
    {
        // Publish the event to the player's private queue
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}.{@event.Nickname}");
        var integrationEvent = new HoleCardsAreDealtIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Cards: ProcessCards(@event.Cards),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleBoardCardsAreDealt(BoardCardsAreDealtEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new BoardCardsAreDealtIntegrationEvent(
            Cards: ProcessCards(@event.Cards),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleDecisionIsRequested(DecisionIsRequestedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new DecisionIsRequestedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            FoldIsAvailable: @event.FoldIsAvailable,
            CheckIsAvailable: @event.CheckIsAvailable,
            CallIsAvailable: @event.CallIsAvailable,
            CallToAmount: (int)@event.CallToAmount,
            RaiseIsAvailable: @event.RaiseIsAvailable,
            MinRaiseToAmount: (int)@event.MinRaiseToAmount,
            MaxRaiseToAmount: (int)@event.MaxRaiseToAmount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleDecisionIsCommitted(DecisionIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new DecisionIsCommittedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            DecisionType: @event.Decision.Type.ToString(),
            DecisionAmount: (int)@event.Decision.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleRefundIsCommitted(RefundIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new RefundIsCommittedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleWinWithoutShowdownIsCommitted(WinWithoutShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new WinWithoutShowdownIsCommittedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleWinAtShowdownIsCommitted(WinAtShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        foreach (var (nickname, amount) in @event.WinPot)
        {
            var integrationEvent = new WinAtShowdownIsCommittedIntegrationEvent(
                Nickname: (string)nickname,
                Amount: (int)amount,
                HandUid: _handUid,
                TableUid: _tableUid,
                OccuredAt: @event.OccuredAt
            );
            _integrationEventBus.Publish(integrationEvent, queue);
        }
    }

    private void HandleHoleCardsAreShown(HoleCardsAreShownEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HoleCardsAreShownIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Cards: ProcessCards(@event.Cards),
            ComboType: @event.Combo.Type.ToString(),
            ComboWeight: @event.Combo.Weight,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void HandleHoleCardsAreMucked(HoleCardsAreMuckedEvent @event)
    {
        var queue = new IntegrationEventQueue($"table.{_tableUid}.{_handUid}");
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private IntegrationEventParticipant ProcessParticipant(Participant participant)
    {
        return new IntegrationEventParticipant(
            Nickname: (string)participant.Nickname,
            Position: participant.Position.ToString(),
            Stake: (int)participant.Stake
        );
    }

    private List<string> ProcessCards(CardSet cards)
    {
        return cards.Select(x => x.ToString()).ToList();
    }
}

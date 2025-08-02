using Domain.Event;
using Domain.ValueObject;
using System.Collections.Immutable;

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

    public void Publish(IEnumerable<BaseEvent> events)
    {
        foreach (var @event in events)
        {
            PublishEvent((dynamic)@event);
        }
    }

    private void PublishEvent(HandIsCreatedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-created");
        var integrationEvent = new HandIsCreatedIntegrationEvent(
            Game: @event.Game.ToString(),
            SmallBlind: (int)@event.SmallBlind,
            BigBlind: (int)@event.BigBlind,
            Participants: @event.Participants.Select(ProcessParticipant).ToImmutableList(),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(HandIsStartedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-started");
        var integrationEvent = new HandIsStartedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(HandIsFinishedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hand-finished");
        var integrationEvent = new HandIsFinishedIntegrationEvent(
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(PlayerConnectedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.player-connected");
        var integrationEvent = new PlayerConnectedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(PlayerDisconnectedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.player-disconnected");
        var integrationEvent = new PlayerDisconnectedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(SmallBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.blind-posted");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(BigBlindIsPostedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.blind-posted");
        var integrationEvent = new BlindIsPostedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(HoleCardsAreDealtEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-dealt");
        var integrationEvent = new HoleCardsAreDealtIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Cards: ProcessCards(@event.Cards),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(BoardCardsAreDealtEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.board-cards-dealt");
        var integrationEvent = new BoardCardsAreDealtIntegrationEvent(
            Cards: ProcessCards(@event.Cards),
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(DecisionIsRequestedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.decision-requested");
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

    private void PublishEvent(DecisionIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.decision-committed");
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

    private void PublishEvent(RefundIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.refund-committed");
        var integrationEvent = new RefundIsCommittedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(WinWithoutShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.win-without-showdown-committed");
        var integrationEvent = new WinWithoutShowdownIsCommittedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            Amount: (int)@event.Amount,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(WinAtShowdownIsCommittedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.win-at-showdown-committed");
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

    private void PublishEvent(HoleCardsAreShownEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-shown");
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

    private void PublishEvent(HoleCardsAreMuckedEvent @event)
    {
        var queue = new IntegrationEventQueue("hand.hole-cards-mucked");
        var integrationEvent = new HoleCardsAreMuckedIntegrationEvent(
            Nickname: (string)@event.Nickname,
            HandUid: _handUid,
            TableUid: _tableUid,
            OccuredAt: @event.OccuredAt
        );
        _integrationEventBus.Publish(integrationEvent, queue);
    }

    private void PublishEvent(object @event)
    {
        // No handler for the given event
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
